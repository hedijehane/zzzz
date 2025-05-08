using Microsoft.EntityFrameworkCore;
using PFE.Application.Interfaces;
using PFE.Domain.Entities;
using PFE.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFE.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Chat> GetChatByIdAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.Department)
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(20))
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task<List<Chat>> GetUserChatsAsync(int userId)
        {
            return await _context.Chats
                .Include(c => c.Department)
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.Sender)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(c => c.LastActivityAt)
                .ToListAsync();
        }

        public async Task<Chat> GetPrivateChatBetweenUsersAsync(int user1Id, int user2Id)
        {
            return await _context.Chats
                .Include(c => c.Participants)
                .Where(c => c.Type == ChatType.Private)
                .Where(c => c.Participants.Any(p => p.UserId == user1Id))
                .Where(c => c.Participants.Any(p => p.UserId == user2Id))
                .FirstOrDefaultAsync();
        }

        public async Task<Chat> GetDepartmentChatAsync(int departmentId)
        {
            return await _context.Chats
                .Where(c => c.DepartmentId == departmentId)
                .Where(c => c.Type == ChatType.Group)
                .FirstOrDefaultAsync();
        }

        public async Task<Chat> CreateChatAsync(Chat chat)
        {
            await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();

            // Reload chat to get all navigation properties
            return await GetChatByIdAsync(chat.Id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}