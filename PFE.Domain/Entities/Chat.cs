using System;
using System.Collections.Generic;

namespace PFE.Domain.Entities;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    // Navigation properties
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public int ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
}

public enum ChatType
{
    Private, // 1-on-1 chat
    Group    // Department group chat
}

public class Chat
{
    public int Id { get; set; }
    public string? Name { get; set; } // Null for private chats, required for group chats
    public ChatType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivityAt { get; set; }

    // For group chats only - optional department association
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    // Navigation properties
    public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class ChatParticipant
{
    public int Id { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsAdmin { get; set; } = false; // For group chats

    // Navigation properties
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
}