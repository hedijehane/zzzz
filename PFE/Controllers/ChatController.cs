using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PFE.API.Hubs;
using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PFE.API.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly IUserConnectionManager _connectionManager;

        public ChatController(
            IChatService chatService,
            IUserRepository userRepository,
            IHubContext<ChatHub> chatHubContext,
            IUserConnectionManager connectionManager)
        {
            _chatService = chatService;
            _userRepository = userRepository;
            _chatHubContext = chatHubContext;
            _connectionManager = connectionManager;
        }

        // GET: /Chat
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var chats = await _chatService.GetUserChatsAsync(userId);
            return View(chats);
        }

        // GET: /Chat/Details/5
        public async Task<IActionResult> Details(int id)
        {
            int userId = GetCurrentUserId();
            var chat = await _chatService.GetChatByIdAsync(id, userId);
            if (chat == null)
            {
                return NotFound();
            }

            var messages = await _chatService.GetChatMessagesAsync(id, userId);

            // Mark messages as read
            await _chatService.MarkMessagesAsReadAsync(id, userId);

            ViewBag.Messages = messages;
            ViewBag.CurrentUserId = userId;

            return View(chat);
        }

        // GET: /Chat/NewPrivateChat
        public async Task<IActionResult> NewPrivateChat()
        {
            int userId = GetCurrentUserId();
            var currentUser = await _userRepository.GetByIdAsync(userId);

            // Get users from the same department
            var users = await _userRepository.GetAllUsersWithDetailsAsync();
            var departmentUsers = users
                .Where(u => u.DepartmentId == currentUser.DepartmentId && u.Id != userId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department.Name
                })
                .ToList();

            return View(departmentUsers);
        }

        // POST: /Chat/CreatePrivateChat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePrivateChat(int otherUserId)
        {
            int userId = GetCurrentUserId();

            try
            {
                var chat = await _chatService.CreatePrivateChatAsync(userId, otherUserId);
                return RedirectToAction(nameof(Details), new { id = chat.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(NewPrivateChat));
            }
        }

        // GET: /Chat/NewGroupChat
        public async Task<IActionResult> NewGroupChat()
        {
            int userId = GetCurrentUserId();
            var currentUser = await _userRepository.GetUserWithDetailsAsync(userId);

            if (currentUser == null)
            {
                return NotFound();
            }

            // Get users from the same department
            var users = await _userRepository.GetAllUsersWithDetailsAsync();
            var departmentUsers = users
                .Where(u => u.DepartmentId == currentUser.DepartmentId && u.Id != userId)
                .ToList();

            ViewBag.DepartmentId = currentUser.DepartmentId;
            ViewBag.DepartmentName = currentUser?.Department?.Name ?? "Unknown Department";
            ViewBag.DepartmentUsers = departmentUsers.Select(u => new
            {
                Id = u.Id,
                FullName = u.Name,
                Email = u.Email,
                Role = u.Role?.Name
            }).ToList();

            return View();
        }

        // POST: /Chat/CreateGroupChat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroupChat(string chatName, int departmentId, List<int> selectedUserIds)
        {
            int userId = GetCurrentUserId();

            if (selectedUserIds == null || !selectedUserIds.Any())
            {
                ModelState.AddModelError("", "Please select at least one user for the group chat");
                return RedirectToAction(nameof(NewGroupChat));
            }

            try
            {
                // Make sure creator is in the list
                if (!selectedUserIds.Contains(userId))
                {
                    selectedUserIds.Add(userId);
                }

                var chat = await _chatService.CreateCustomGroupChatAsync(userId, departmentId, chatName, selectedUserIds);
                return RedirectToAction(nameof(Details), new { id = chat.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(NewGroupChat));
            }
        }

        // POST: /Chat/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(MessageCreateDto messageDto)
        {
            int userId = GetCurrentUserId();

            if (userId != messageDto.SenderId)
            {
                return Unauthorized();
            }

            try
            {
                await _chatService.AddMessageAsync(messageDto);

                // Update the chat's last activity timestamp
                await _chatService.UpdateChatLastActivityAsync(messageDto.ChatId);

                return RedirectToAction(nameof(Details), new { id = messageDto.ChatId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(Details), new { id = messageDto.ChatId });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst("sub") ??
                              User.FindFirst("userId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user authentication");
            }

            return userId;
        }
    }
}