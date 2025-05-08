using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using PFE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFE.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public ChatService(
            IChatRepository chatRepository,
            IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task<List<ChatDto>> GetUserChatsAsync(int userId)
        {
            var chats = await _chatRepository.GetUserChatsAsync(userId);
            var chatDtos = new List<ChatDto>();

            foreach (var chat in chats)
            {
                var unreadMessages = await _messageRepository.GetUnreadMessagesAsync(chat.Id, userId);

                var chatDto = new ChatDto
                {
                    Id = chat.Id,
                    Name = chat.Type == ChatType.Private
                        ? GetPrivateChatName(chat, userId)
                        : chat.Name,
                    ChatType = chat.Type.ToString(),
                    CreatedAt = chat.CreatedAt,
                    LastActivityAt = chat.LastActivityAt,
                    DepartmentId = chat.DepartmentId,
                    DepartmentName = chat.Department?.Name,
                    UnreadCount = unreadMessages.Count,
                    LastMessage = chat.Messages.Any()
                        ? MapMessageToDto(chat.Messages.OrderByDescending(m => m.SentAt).First())
                        : null,
                    Participants = chat.Participants.Select(p => new ChatParticipantDto
                    {
                        UserId = p.UserId,
                        UserName = p.User.Name,
                        IsAdmin = p.IsAdmin,
                        JoinedAt = p.JoinedAt
                    }).ToList()
                };

                chatDtos.Add(chatDto);
            }

            return chatDtos;
        }

        public async Task<ChatDto> GetChatByIdAsync(int chatId, int userId)
        {
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat == null) return null;

            // Ensure user is a participant
            if (!chat.Participants.Any(p => p.UserId == userId))
            {
                throw new UnauthorizedAccessException("User is not a participant in this chat");
            }

            var unreadMessages = await _messageRepository.GetUnreadMessagesAsync(chatId, userId);

            return new ChatDto
            {
                Id = chat.Id,
                Name = chat.Type == ChatType.Private
                    ? GetPrivateChatName(chat, userId)
                    : chat.Name,
                ChatType = chat.Type.ToString(),
                CreatedAt = chat.CreatedAt,
                LastActivityAt = chat.LastActivityAt,
                DepartmentId = chat.DepartmentId,
                DepartmentName = chat.Department?.Name,
                UnreadCount = unreadMessages.Count,
                LastMessage = chat.Messages.Any()
                    ? MapMessageToDto(chat.Messages.OrderByDescending(m => m.SentAt).First())
                    : null,
                Participants = chat.Participants.Select(p => new ChatParticipantDto
                {
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    IsAdmin = p.IsAdmin,
                    JoinedAt = p.JoinedAt
                }).ToList()
            };
        }

        public async Task<List<MessageDto>> GetChatMessagesAsync(int chatId, int userId, int page = 1, int pageSize = 20)
        {
            // Verify user is a participant
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat == null || !chat.Participants.Any(p => p.UserId == userId))
            {
                throw new UnauthorizedAccessException("User is not a participant in this chat");
            }

            var messages = await _messageRepository.GetChatMessagesAsync(chatId, page, pageSize);
            return messages.Select(MapMessageToDto).ToList();
        }

        public async Task<MessageDto> AddMessageAsync(MessageCreateDto messageDto)
        {
            // Verify user is a participant
            var chat = await _chatRepository.GetChatByIdAsync(messageDto.ChatId);
            if (chat == null || !chat.Participants.Any(p => p.UserId == messageDto.SenderId))
            {
                throw new UnauthorizedAccessException("User is not a participant in this chat");
            }

            var message = new Message
            {
                Content = messageDto.Content,
                SenderId = messageDto.SenderId,
                ChatId = messageDto.ChatId,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            var addedMessage = await _messageRepository.AddMessageAsync(message);

            // Update chat's last activity time
            await UpdateChatLastActivityAsync(messageDto.ChatId);

            return MapMessageToDto(addedMessage);
        }

        public async Task<ChatDto> CreatePrivateChatAsync(int userId, int otherUserId)
        {
            // Check if chat already exists
            var existingChat = await _chatRepository.GetPrivateChatBetweenUsersAsync(userId, otherUserId);
            if (existingChat != null)
            {
                return await GetChatByIdAsync(existingChat.Id, userId);
            }

            // Verify both users exist
            var user = await _userRepository.GetByIdAsync(userId);
            var otherUser = await _userRepository.GetByIdAsync(otherUserId);

            if (user == null || otherUser == null)
            {
                throw new ArgumentException("One or both users do not exist");
            }

            // Create new private chat
            var chat = new Chat
            {
                Type = ChatType.Private,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                Participants = new List<ChatParticipant>
                {
                    new ChatParticipant { UserId = userId, JoinedAt = DateTime.UtcNow },
                    new ChatParticipant { UserId = otherUserId, JoinedAt = DateTime.UtcNow }
                }
            };

            var createdChat = await _chatRepository.CreateChatAsync(chat);

            return new ChatDto
            {
                Id = createdChat.Id,
                Name = otherUser.Name, // For the creator, show the other user's name
                ChatType = ChatType.Private.ToString(),
                CreatedAt = createdChat.CreatedAt,
                LastActivityAt = createdChat.LastActivityAt,
                UnreadCount = 0,
                Participants = createdChat.Participants.Select(p => new ChatParticipantDto
                {
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    IsAdmin = false,
                    JoinedAt = p.JoinedAt
                }).ToList()
            };
        }

        public async Task<ChatDto> CreateDepartmentGroupChatAsync(int creatorId, int departmentId, string chatName)
        {
            // Check if user is in this department
            var creator = await _userRepository.GetByIdAsync(creatorId);
            if (creator == null)
            {
                throw new ArgumentException("User does not exist");
            }

            if (creator.DepartmentId != departmentId)
            {
                throw new UnauthorizedAccessException("User is not a member of this department");
            }

            // Check if department exists
            bool departmentExists = await _userRepository.DepartmentExistsAsync(departmentId);
            if (!departmentExists)
            {
                throw new ArgumentException("Department does not exist");
            }

            // Check if a department chat already exists
            var existingChat = await _chatRepository.GetDepartmentChatAsync(departmentId);
            if (existingChat != null)
            {
                return await GetChatByIdAsync(existingChat.Id, creatorId);
            }

            // Get all department members
            var departmentUsers = await _userRepository.GetAllUsersWithDetailsAsync();
            departmentUsers = departmentUsers.Where(u => u.DepartmentId == departmentId).ToList();

            // Create new group chat for all department members
            var chat = new Chat
            {
                Name = chatName,
                Type = ChatType.Group,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                DepartmentId = departmentId,
                Participants = new List<ChatParticipant>()
            };

            // Add all department members
            foreach (var user in departmentUsers)
            {
                chat.Participants.Add(new ChatParticipant
                {
                    UserId = user.Id,
                    IsAdmin = user.Id == creatorId, // Creator is admin
                    JoinedAt = DateTime.UtcNow
                });
            }

            var createdChat = await _chatRepository.CreateChatAsync(chat);

            return new ChatDto
            {
                Id = createdChat.Id,
                Name = chatName,
                ChatType = ChatType.Group.ToString(),
                CreatedAt = createdChat.CreatedAt,
                LastActivityAt = createdChat.LastActivityAt,
                DepartmentId = departmentId,
                DepartmentName = departmentUsers.FirstOrDefault()?.Department?.Name,
                UnreadCount = 0,
                Participants = createdChat.Participants.Select(p => new ChatParticipantDto
                {
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    IsAdmin = p.UserId == creatorId,
                    JoinedAt = p.JoinedAt
                }).ToList()
            };
        }

        public async Task<ChatDto> CreateCustomGroupChatAsync(int creatorId, int departmentId, string chatName, List<int> selectedUserIds)
        {
            // Check if user is in this department
            var creator = await _userRepository.GetByIdAsync(creatorId);
            if (creator == null)
            {
                throw new ArgumentException("User does not exist");
            }

            if (creator.DepartmentId != departmentId)
            {
                throw new UnauthorizedAccessException("User is not a member of this department");
            }

            // Check if department exists
            bool departmentExists = await _userRepository.DepartmentExistsAsync(departmentId);
            if (!departmentExists)
            {
                throw new ArgumentException("Department does not exist");
            }

            // Validate that all selected users are in the same department
            var allUsers = await _userRepository.GetAllUsersWithDetailsAsync();
            var selectedUsers = allUsers.Where(u => selectedUserIds.Contains(u.Id)).ToList();

            if (selectedUsers.Any(u => u.DepartmentId != departmentId))
            {
                throw new ArgumentException("All selected users must be from the same department");
            }

            // Create new group chat with selected users
            var chat = new Chat
            {
                Name = chatName,
                Type = ChatType.Group,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                DepartmentId = departmentId,
                Participants = new List<ChatParticipant>()
            };

            // Add selected users as participants
            foreach (var userId in selectedUserIds)
            {
                chat.Participants.Add(new ChatParticipant
                {
                    UserId = userId,
                    IsAdmin = userId == creatorId, // Creator is admin
                    JoinedAt = DateTime.UtcNow
                });
            }

            // Make sure creator is a participant and admin
            if (!selectedUserIds.Contains(creatorId))
            {
                chat.Participants.Add(new ChatParticipant
                {
                    UserId = creatorId,
                    IsAdmin = true,
                    JoinedAt = DateTime.UtcNow
                });
            }

            var createdChat = await _chatRepository.CreateChatAsync(chat);

            return new ChatDto
            {
                Id = createdChat.Id,
                Name = chatName,
                ChatType = ChatType.Group.ToString(),
                CreatedAt = createdChat.CreatedAt,
                LastActivityAt = createdChat.LastActivityAt,
                DepartmentId = departmentId,
                DepartmentName = creator.Department?.Name,
                UnreadCount = 0,
                Participants = createdChat.Participants.Select(p => new ChatParticipantDto
                {
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    IsAdmin = p.UserId == creatorId,
                    JoinedAt = p.JoinedAt
                }).ToList()
            };
        }

        public async Task MarkMessagesAsReadAsync(int chatId, int userId)
        {
            // Verify user is a participant
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat == null || !chat.Participants.Any(p => p.UserId == userId))
            {
                throw new UnauthorizedAccessException("User is not a participant in this chat");
            }

            // Get unread messages for this user in this chat
            var unreadMessages = await _messageRepository.GetUnreadMessagesAsync(chatId, userId);

            // Mark messages as read
            if (unreadMessages.Any())
            {
                await _messageRepository.MarkMessagesAsReadAsync(unreadMessages);
            }
        }

        public async Task UpdateChatLastActivityAsync(int chatId)
        {
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat != null)
            {
                chat.LastActivityAt = DateTime.UtcNow;
                await _chatRepository.SaveChangesAsync();
            }
        }

        // Helper methods
        private string GetPrivateChatName(Chat chat, int userId)
        {
            // For private chats, show the other user's name as the chat name
            var otherParticipant = chat.Participants
                .FirstOrDefault(p => p.UserId != userId);

            return otherParticipant?.User.Name ?? "Unknown User";
        }

        private MessageDto MapMessageToDto(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                SenderName = message.Sender?.Name ?? "Unknown User",
                ChatId = message.ChatId
            };
        }
    }
}