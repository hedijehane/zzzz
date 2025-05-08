using Microsoft.AspNetCore.Mvc;
using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using System.Security.Claims;


namespace PFE.Web.Controllers
{
    public class PublicationController : Controller
    {
        private readonly IPublicationService _publicationService;

        public PublicationController(IPublicationService publicationService)
        {
            _publicationService = publicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var publications = await _publicationService.GetApprovedPublicationsAsync();
                return View(publications);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while loading publications.");
                return View(new List<PublicationDto>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreatePublicationDto dto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "You must be logged in to submit a publication." });
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _publicationService.CreatePublicationAsync(dto, userId);
                return Json(new
                {
                    success = true,
                    message = "Publication submitted successfully! It will be reviewed by the department head before appearing in the feed."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to submit publication: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            try
            {
                // Retrieve the publication by ID
                var publication = await _publicationService.GetPublicationByIdAsync(id);

                if (publication == null)
                {
                    return NotFound($"Publication with ID {id} not found.");
                }

                // Check if the publication has an image
                if (publication.ImageData != null)
                {
                    return File(publication.ImageData, "image/jpeg"); // You can adjust the MIME type if needed
                }

                return NotFound("Image not available for this publication.");
            }
            catch (Exception ex)
            {
                // Log or inspect the exception to provide more details for debugging
                return BadRequest($"Error loading image: {ex.Message}");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var success = await _publicationService.ApprovePublicationAsync(id, approverId);

                if (success)
                    TempData["Success"] = "Publication approved successfully.";
                else
                    TempData["Error"] = "You are not authorized to approve this publication.";

                return RedirectToAction("Pending");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Approval failed: {ex.Message}";
                return RedirectToAction("Pending");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var success = await _publicationService.RejectPublicationAsync(id, approverId);

                if (success)
                    TempData["Success"] = "Publication rejected successfully.";
                else
                    TempData["Error"] = "You are not authorized to reject this publication.";

                return RedirectToAction("Pending");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Rejection failed: {ex.Message}";
                return RedirectToAction("Pending");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Pending()
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var approverId))
            {
                TempData["Error"] = "Could not determine your user ID.";
                return View(new List<PublicationDto>());
            }

            var publications = await _publicationService.GetPendingPublicationsForApproverAsync(approverId);

            // ←–– Add this line:
            ViewBag.DebugPendingCount = publications?.Count ?? 0;

            if (publications == null || !publications.Any())
            {
                TempData["Info"] = "There are currently no pending publications in your department.";
            }

            return View(publications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([FromForm] AddCommentDto dto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "You must be logged in to add a comment." });
            }

            try
            {
                // Get the current logged-in user's ID from the authentication context
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Create a new AddCommentDto including the logged-in user's ID
                var commentDto = new AddCommentDto(dto.PublicationId, userId, dto.Text);

                // Call the service to add the comment
                var addedComment = await _publicationService.AddCommentAsync(commentDto);

                return Json(new
                {
                    success = true,
                    message = "Comment added successfully!",
                    data = addedComment // Return the added comment data (mapped from CommentDto)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to add comment. Error: {ex.Message}" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReaction([FromForm] AddReactionDto dto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "You must be logged in to add a reaction." });
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var reactionDto = new AddReactionDto(dto.PublicationId, userId, dto.Type);
                var addedReaction = await _publicationService.AddReactionAsync(reactionDto);

                // Get updated reactions for this publication
                var allReactions = await _publicationService.GetReactionsForPublicationAsync(dto.PublicationId);

                return Json(new
                {
                    success = true,
                    message = "Reaction updated successfully!",
                    data = addedReaction,
                    allReactions = allReactions,
                    reactionCounts = allReactions.GroupBy(r => r.Type)
                                             .Select(g => new { type = g.Key, count = g.Count() })
                                             .ToDictionary(x => x.type, x => x.count)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to add reaction. Error: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetReactions(int publicationId)
        {
            try
            {
                var reactions = await _publicationService.GetReactionsForPublicationAsync(publicationId);
                return Json(new { success = true, data = reactions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to load reactions: {ex.Message}" });
            }
        }

















    }
}
