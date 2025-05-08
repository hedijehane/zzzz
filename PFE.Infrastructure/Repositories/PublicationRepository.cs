    using Microsoft.EntityFrameworkCore;
using PFE.Application.DTOs;
using PFE.Application.Interfaces;
    using PFE.Domain.Entities;
    using PFE.Infrastructure.Data;
    using System.Linq.Expressions;

namespace PFE.Infrastructure.Repositories
{
    public class PublicationRepository : IPublicationRepository
    {
        private readonly ApplicationDbContext _context;

        public PublicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Publication> AddAsync(Publication publication)
        {
            if (!await UserExistsAsync(publication.AuthorId))
                throw new InvalidOperationException("Invalid author");

            await _context.Publications.AddAsync(publication);
            await _context.SaveChangesAsync();
            return publication;
        }

        public async Task<Publication?> GetByIdAsync(int id, params Expression<Func<Publication, object>>[] includes)
        {
            var query = _context.Publications.AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Publication>> GetApprovedPublicationsAsync()
        {
            return await _context.Publications
                .Include(p => p.Author) // Ensure Author is loaded
                .ThenInclude(a => a.Department)
                .Include(p => p.Reactions)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Where(p => p.IsApproved)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Publication>> GetPendingPublicationsAsync()
        {
            return await _context.Publications
                .Include(p => p.Author)
                    .ThenInclude(a => a.Department)
                .Where(p => !p.IsApproved)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ApprovePublicationAsync(int publicationId, int approverId)
        {
            var publication = await GetByIdAsync(publicationId,
                p => p.Author!,
                p => p.Author!.Department!);

            var approver = await GetUserWithDepartmentAndRoleAsync(approverId);

            if (publication == null || approver == null)
                return false;

            // Clean role check and department match
            var roleName = approver.Role.Name?.Trim().ToLowerInvariant();
            if (roleName != "head department" || publication.Author.DepartmentId != approver.DepartmentId)
                return false;

            publication.IsApproved = true;
            publication.ApprovedById = approverId;
            publication.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectPublicationAsync(int publicationId, int approverId)
        {
            var publication = await GetByIdAsync(publicationId,
                p => p.Author!,
                p => p.Author!.Department!);

            var approver = await GetUserWithDepartmentAndRoleAsync(approverId);

            if (publication == null || approver == null)
                return false;

            // Clean role check and department match
            var roleName = approver.Role.Name?.Trim().ToLowerInvariant();
            if (roleName != "head department" || publication.Author.DepartmentId != approver.DepartmentId)
                return false;

            _context.Publications.Remove(publication);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserWithDepartmentAndRoleAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<Publication>> GetPendingPublicationsByDepartmentAsync(int departmentId)
        {
            return await _context.Publications
                .Include(p => p.Author)
                    .ThenInclude(a => a.Department) // ✅ this line is key
                .Where(p => !p.IsApproved &&
                            p.Author != null &&
                            p.Author.DepartmentId == departmentId)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }



        public async Task<Publication?> GetPublicationWithDetailsAsync(int id)
        {
            return await _context.Publications
                .Include(p => p.Author)
                    .ThenInclude(a => a.Department)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Reactions)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }


        public async Task<Reaction> AddReactionAsync(Reaction reaction)
        {
            // Check if the user already has a reaction on this publication
            var existingReaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.PublicationId == reaction.PublicationId &&
                                          r.UserId == reaction.UserId);

            if (existingReaction != null)
            {
                // If reaction type is the same, remove it (toggle functionality)
                if (existingReaction.Type == reaction.Type)
                {
                    _context.Reactions.Remove(existingReaction);
                    await _context.SaveChangesAsync();
                    return existingReaction; // Return the removed reaction
                }
                // Otherwise, update the existing reaction type
                existingReaction.Type = reaction.Type;
                existingReaction.CreatedAt = DateTime.UtcNow; // Optional: update timestamp
                await _context.SaveChangesAsync();
                return existingReaction;
            }

            // If no existing reaction, add the new one
            _context.Reactions.Add(reaction);
            await _context.SaveChangesAsync();
            return reaction;
        }
        public async Task<List<Reaction>> GetReactionsForPublicationAsync(int publicationId)
        {
            return await _context.Reactions
                                 .Where(r => r.PublicationId == publicationId)
                                 .Include(r => r.User)  // Ensure that the User details are loaded
                                 .ToListAsync();
        }



    }

}