using Microsoft.AspNetCore.Http;

namespace PFE.Application.DTOs
{
    public class CreatePublicationDto
    {
        public string Content { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }

    public record CommentDto(int Id, int UserId, string UserName, string Text, DateTime CreatedAt);

    public record ReactionDto(int Id, int UserId, string UserName, string Type, DateTime CreatedAt);


    public record PublicationDto(
        int Id,
        string Content,
        byte[]? ImageData,
        string AuthorName,
        int AuthorId,
        int AuthorDepartmentId,
        DateTime CreatedAt,
        bool IsApproved,
        List<CommentDto> Comments,
        List<ReactionDto> Reactions);

    public record AddCommentDto(int PublicationId, int UserId, string Text);
    public record AddReactionDto(int PublicationId, int UserId, string Type);



}