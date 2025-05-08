using Microsoft.AspNetCore.SignalR;
using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace PFE.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IUserConnectionManager _connectionManager;

        public ChatHub(IChatService chatService, IUserConnectionManager connectionManager)
        {
            _chatService = chatService;
            _connectionManager = connectionManager;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                // Get user ID from the HTTP context claims
                int userId = GetUserIdFromContext();

                // Store connection ID for the user
                _connectionManager.AddConnection(userId, Context.ConnectionId);

                // Join user to all their chat groups
                var userChats = await _chatService.GetUserChatsAsync(userId);
                foreach (var chat in userChats)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chat.Id}");
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                // Log error and continue
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
                await base.OnConnectedAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                // Get user ID from the HTTP context claims
                int userId = GetUserIdFromContext();

                // Remove the connection for this user
                _connectionManager.RemoveConnection(userId, Context.ConnectionId);

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                // Log error and continue
                Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
                await base.OnDisconnectedAsync(exception);
            }
        }

        public async Task SendMessage(MessageCreateDto messageDto)
        {
            try
            {
                // Validate that the sender ID in the DTO matches the authenticated user
                int userId = GetUserIdFromContext();
                if (userId != messageDto.SenderId)
                {
                    throw new UnauthorizedAccessException("You can only send messages as yourself");
                }

                // Add the message to the database
                var message = await _chatService.AddMessageAsync(messageDto);

                // Update the chat's LastActivityAt
                await _chatService.UpdateChatLastActivityAsync(messageDto.ChatId);

                // Broadcast the message to all users in the chat group
                await Clients.Group($"chat_{messageDto.ChatId}").SendAsync("ReceiveMessage", message);
            }
            catch (Exception ex)
            {
                // Send error only to the caller
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
            }
        }

        public async Task JoinChat(int chatId)
        {
            try
            {
                int userId = GetUserIdFromContext();

                // Verify user is a participant
                var chat = await _chatService.GetChatByIdAsync(chatId, userId);
                if (chat == null)
                {
                    throw new UnauthorizedAccessException("You are not a participant in this chat");
                }

                // Add user to the chat group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");

                // Notify the client that joining was successful
                await Clients.Caller.SendAsync("JoinedChat", chatId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
            }
        }

        public async Task LeaveChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
            // Notify the client that leaving was successful
            await Clients.Caller.SendAsync("LeftChat", chatId);
        }

        public async Task MarkMessagesAsRead(int chatId)
        {
            try
            {
                int userId = GetUserIdFromContext();
                await _chatService.MarkMessagesAsReadAsync(chatId, userId);

                // Notify the user that messages have been read
                await Clients.Caller.SendAsync("MessagesMarkedAsRead", chatId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
            }
        }

        private int GetUserIdFromContext()
        {
            // Extract user ID from the JWT claims or auth cookie 
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier) ??
                              Context.User?.FindFirst("sub") ??
                              Context.User?.FindFirst("userId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user authentication");
            }
            return userId;
        }
    }
}