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
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public MessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Message>> GetChatMessagesAsync(int chatId, int page = 1, int pageSize = 20)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Message> AddMessageAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetUnreadMessagesAsync(int chatId, int userId)
        {
            return await _context.Messages
                .Where(m => m.ChatId == chatId)
                .Where(m => m.SenderId != userId) // Only messages from other users
                .Where(m => !m.IsRead)
                .ToListAsync();
        }

        public async Task MarkMessagesAsReadAsync(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                message.IsRead = true;
            }
            await SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}