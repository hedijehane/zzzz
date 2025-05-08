using PFE.Domain.Entities;

public class Publication
{
    public int Id { get; set; }
    public required string Content { get; set; }

    public bool IsApproved { get; set; } = false;
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public int? ApprovedById { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public byte[]? ImageData { get; set; }


    public List<Comment> Comments { get; set; } = new();
    public List<Reaction> Reactions { get; set; } = new();
}

public class Comment
{
    public int Id { get; set; }
    public int PublicationId { get; set; }
    public Publication Publication { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public required string Text { get; set; }   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Reaction
{
    public int Id { get; set; }
    public int PublicationId { get; set; }
    public Publication Publication { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public required string Type { get; set; } // e.g., "Like"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
