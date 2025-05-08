using System;
using System.Collections.Generic;

namespace PFE.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = null!;
        public int ChatId { get; set; }
    }

    public class MessageCreateDto
    {
        public string Content { get; set; } = null!;
        public int SenderId { get; set; }
        public int ChatId { get; set; }
    }

    public class ChatParticipantDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class ChatDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string ChatType { get; set; } = null!; // "Private" or "Group"
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public MessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public List<ChatParticipantDto> Participants { get; set; } = new List<ChatParticipantDto>();
    }

    public class ChatCreateDto
    {
        public string? Name { get; set; }
        public string ChatType { get; set; } = null!;
        public int CreatorId { get; set; }
        public int? DepartmentId { get; set; }
        public List<int> ParticipantIds { get; set; } = new List<int>();
    }
}