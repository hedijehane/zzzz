using PFE.Application.DTOs;
using PFE.Domain.Entities;

namespace PFE.Application.Interfaces
{
    public interface IPublicationService
    {
        Task<PublicationDto> CreatePublicationAsync(CreatePublicationDto createDto, int authorId);
        Task<List<PublicationDto>> GetApprovedPublicationsAsync();
        Task<List<PublicationDto>> GetPendingPublicationsAsync();
        Task<List<PublicationDto>> GetPendingPublicationsForApproverAsync(int approverId);
        Task<bool> ApprovePublicationAsync(int publicationId, int approverId);
        Task<bool> RejectPublicationAsync(int publicationId, int approverId);
        Task<PublicationDto?> GetPublicationByIdAsync(int id);
        // Correct the method signature here
        Task<CommentDto> AddCommentAsync(AddCommentDto dto);  // Correct return type
        Task<PublicationDto?> GetPublicationWithCommentsAsync(int publicationId);  // F
        Task<ReactionDto> AddReactionAsync(AddReactionDto dto);
        // Add this method in IPublicationService interface
        Task<List<ReactionDto>> GetReactionsForPublicationAsync(int publicationId);



    }
}