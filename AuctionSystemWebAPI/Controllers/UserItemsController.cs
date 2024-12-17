using AuctionSystemWebAPI.DTOs;
using AuctionSystemWebAPI.Models;
using AuctionSystemWebAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AuctionSystemWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserItemsController : ControllerBase
    {
        private readonly IUserItemRepository _userRepository;

        public UserItemsController(IUserItemRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateUser([FromBody] CreateUserItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest("Full name and email are required.");
            }

            var user = new UserItem
            {
                id = Guid.NewGuid().ToString(), // Replace this with Azure AD B2C ID if available
                FullName = dto.FullName,
                Email = dto.Email,
                AuctionIds = dto.AuctionIds ?? new List<string>(),
                WonAuctionIds = dto.WonAuctionIds ?? new List<string>()
            };

            await _userRepository.AddOrUpdateAsync(user);
            return Ok(user);
        }

    }
}
