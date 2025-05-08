using PFE.Application.DTOs;
using PFE.Domain.Entities;
using System.Linq.Expressions;

namespace PFE.Application.Interfaces
{
    public interface IPublicationRepository
    {
        // Basic CRUD
        Task<Publication> AddAsync(Publication publication);
        Task<Publication?> GetByIdAsync(int id, params Expression<Func<Publication, object>>[] includes);

        // Query methods
        Task<List<Publication>> GetApprovedPublicationsAsync();
        Task<List<Publication>> GetPendingPublicationsAsync();
        Task<List<Publication>> GetPendingPublicationsByDepartmentAsync(int departmentId);

        // Approval workflow
        Task<bool> ApprovePublicationAsync(int publicationId, int approverId);
        Task<bool> RejectPublicationAsync(int publicationId, int approverId);

        // Validation
        Task<bool> UserExistsAsync(int userId);
        Task<User?> GetUserWithDepartmentAndRoleAsync(int userId);
        Task<Publication?> GetPublicationWithDetailsAsync(int id);
        //comment management 
        Task<Comment> AddCommentAsync(Comment comment);
        Task<Reaction> AddReactionAsync(Reaction reaction);
        // Add this method in IPublicationRepository interface
        Task<List<Reaction>> GetReactionsForPublicationAsync(int publicationId);







    }
}