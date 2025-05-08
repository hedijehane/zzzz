using PFE.Application.DTOs;
using PFE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PFE.Application.Interfaces
{
    public interface IChatService
    {
        Task<List<ChatDto>> GetUserChatsAsync(int userId);
        Task<ChatDto> GetChatByIdAsync(int chatId, int userId);
        Task<List<MessageDto>> GetChatMessagesAsync(int chatId, int userId, int page = 1, int pageSize = 20);
        Task<MessageDto> AddMessageAsync(MessageCreateDto messageDto);
        Task<ChatDto> CreatePrivateChatAsync(int userId, int otherUserId);
        Task<ChatDto> CreateDepartmentGroupChatAsync(int creatorId, int departmentId, string chatName);
        Task<ChatDto> CreateCustomGroupChatAsync(int creatorId, int departmentId, string chatName, List<int> selectedUserIds);
        Task MarkMessagesAsReadAsync(int chatId, int userId);
        Task UpdateChatLastActivityAsync(int chatId);
    }

        public interface IUserConnectionManager
    {
        void AddConnection(int userId, string connectionId);
        void RemoveConnection(int userId, string connectionId);
        List<string> GetConnections(int userId);
    }

    public interface IChatRepository
    {
        Task<Chat> GetChatByIdAsync(int chatId);
        Task<List<Chat>> GetUserChatsAsync(int userId);
        Task<Chat> GetPrivateChatBetweenUsersAsync(int user1Id, int user2Id);
        Task<Chat> CreateChatAsync(Chat chat);
        Task<Chat> GetDepartmentChatAsync(int departmentId);
        Task SaveChangesAsync();
    }

    public interface IMessageRepository
    {
        Task<List<Message>> GetChatMessagesAsync(int chatId, int page = 1, int pageSize = 20);
        Task<Message> AddMessageAsync(Message message);
        Task<List<Message>> GetUnreadMessagesAsync(int chatId, int userId);
        Task MarkMessagesAsReadAsync(IEnumerable<Message> messages);
        Task SaveChangesAsync();
    }
}