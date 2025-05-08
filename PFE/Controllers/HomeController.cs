using Microsoft.AspNetCore.Mvc;
using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PFE.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPublicationService _publicationService;

        public HomeController(IPublicationService publicationService)
        {
            _publicationService = publicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch the approved publications along with their comments and reactions
                var publications = await _publicationService.GetApprovedPublicationsAsync();

                // Pass the publications along with their comments and reactions to the view
                return View(publications);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while loading publications.");
                return View(new List<PublicationDto>());
            }
        }

        // Other actions, if needed...
    }
}
