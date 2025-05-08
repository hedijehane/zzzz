using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using PFE.Domain.Entities;
using System.Linq.Expressions;

namespace PFE.Application.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly IPublicationRepository _publicationRepository;

        public PublicationService(IPublicationRepository publicationRepository)
        {
            _publicationRepository = publicationRepository;
        }

        public async Task<PublicationDto> CreatePublicationAsync(CreatePublicationDto createDto, int authorId)
        {
            // Verify the user exists
            var author = await _publicationRepository.GetUserWithDepartmentAndRoleAsync(authorId);
            if (author == null)
                throw new InvalidOperationException("User not found");

            // Check if the user is a head department
            bool isHeadDepartment = string.Equals(author.Role?.Name?.Trim(), "head department", StringComparison.OrdinalIgnoreCase);

            byte[]? imageData = null;
            if (createDto.Image != null)
            {
                using var memoryStream = new MemoryStream();
                await createDto.Image.CopyToAsync(memoryStream);
                imageData = memoryStream.ToArray();
            }

            var publication = new Publication
            {
                Content = createDto.Content,
                ImageData = imageData,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow,
                // Auto-approve if author is a head department
                IsApproved = isHeadDepartment
            };

            var createdPublication = await _publicationRepository.AddAsync(publication);
            return MapToDto(createdPublication);
        }

        public async Task<List<PublicationDto>> GetApprovedPublicationsAsync()
        {
            var publications = await _publicationRepository.GetApprovedPublicationsAsync();
            return publications.Select(MapToDto).ToList();
        }

        public async Task<List<PublicationDto>> GetPendingPublicationsAsync()
        {
            var publications = await _publicationRepository.GetPendingPublicationsAsync();
            return publications.Select(MapToDto).ToList();
        }

        public async Task<List<PublicationDto>> GetPendingPublicationsForApproverAsync(int approverId)
        {
            var approver = await _publicationRepository.GetUserWithDepartmentAndRoleAsync(approverId);

            if (approver == null || !string.Equals(approver.Role.Name.Trim(), "head department", StringComparison.OrdinalIgnoreCase))
            {
                return new List<PublicationDto>();
            }

            var publications = await _publicationRepository.GetPendingPublicationsByDepartmentAsync(approver.Department.Id);

            return publications.Select(MapToDto).ToList();
        }

        public async Task<bool> ApprovePublicationAsync(int publicationId, int approverId)
        {
            return await _publicationRepository.ApprovePublicationAsync(publicationId, approverId);
        }

        public async Task<bool> RejectPublicationAsync(int publicationId, int approverId)
        {
            return await _publicationRepository.RejectPublicationAsync(publicationId, approverId);
        }

        public async Task<PublicationDto?> GetPublicationByIdAsync(int id)
        {
            var publication = await _publicationRepository.GetPublicationWithDetailsAsync(id);
            return publication == null ? null : MapToDto(publication);
        }

        public async Task<CommentDto> AddCommentAsync(AddCommentDto dto)
        {
            // Validate publication exists
            var publication = await _publicationRepository.GetByIdAsync(dto.PublicationId);
            if (publication == null)
                throw new Exception("Publication not found.");

            // Validate that the user exists
            var user = await _publicationRepository.GetUserWithDepartmentAndRoleAsync(dto.UserId);
            if (user == null)
                throw new Exception("User not found.");

            // Map DTO to Comment entity
            var comment = new Comment
            {
                PublicationId = dto.PublicationId,
                UserId = dto.UserId,
                Text = dto.Text,
                CreatedAt = DateTime.UtcNow
            };

            // Save the entity using repository
            var savedComment = await _publicationRepository.AddCommentAsync(comment);

            // Map to DTO and return
            return new CommentDto(
                savedComment.Id,
                savedComment.UserId,
                user.Name ?? "Unknown",
                savedComment.Text,
                savedComment.CreatedAt
            );
        }

        public async Task<ReactionDto> AddReactionAsync(AddReactionDto dto)
        {
            // Validate publication exists
            var publication = await _publicationRepository.GetByIdAsync(dto.PublicationId);
            if (publication == null)
                throw new Exception("Publication not found.");

            // Validate that the user exists
            var user = await _publicationRepository.GetUserWithDepartmentAndRoleAsync(dto.UserId);
            if (user == null)
                throw new Exception("User not found.");

            // Create the new Reaction entity
            var reaction = new Reaction
            {
                PublicationId = dto.PublicationId,
                UserId = dto.UserId,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow
            };

            // Save the reaction (this now handles existing reactions)
            var savedReaction = await _publicationRepository.AddReactionAsync(reaction);

            // Create appropriate response message based on operation performed
            bool isNewReaction = savedReaction.Id == reaction.Id;
            string operationMessage = isNewReaction ? "added" : "updated";

            // Map to DTO using the saved reaction and user data
            var reactionDto = new ReactionDto(
                savedReaction.Id,
                savedReaction.UserId,
                user.Name ?? "Unknown",
                savedReaction.Type,
                savedReaction.CreatedAt
            );

            return reactionDto;
        }

        private static PublicationDto MapToDto(Publication publication)
        {
            return new PublicationDto(
                publication.Id,
                publication.Content,
                publication.ImageData,
                publication.Author?.Name ?? "Unknown Author",
                publication.AuthorId,
                publication.Author?.DepartmentId ?? 0,
                publication.CreatedAt,
                publication.IsApproved,
                publication.Comments?.Select(c => new CommentDto(
                    c.Id,
                    c.UserId,
                    c.User?.Name ?? "Unknown User",
                    c.Text,
                    c.CreatedAt
                )).ToList() ?? new List<CommentDto>(),
                publication.Reactions?.Select(r => new ReactionDto(
                    r.Id,
                    r.UserId,
                    r.User?.Name ?? "Unknown User", // Provide the user's name here
                    r.Type, // Type of the reaction (like "Like", "Dislike", etc.)
                    r.CreatedAt // The creation date of the reaction
                )).ToList() ?? new List<ReactionDto>()
            );
        }

        public Task<PublicationDto?> GetPublicationWithCommentsAsync(int publicationId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ReactionDto>> GetReactionsForPublicationAsync(int publicationId)
        {
            var reactions = await _publicationRepository.GetReactionsForPublicationAsync(publicationId);

            return reactions.Select(r => new ReactionDto(
                r.Id,
                r.UserId,
                r.User?.Name ?? "Unknown User",
                r.Type,
                r.CreatedAt
            )).ToList();
        }
    }
}