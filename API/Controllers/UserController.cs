using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
using Application.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private Application.Interfaces.Services.IAuthApplicationService _userAppService;
        private IUserService _userService;
        private ILogger<UserController> _logger;
        private readonly IOnlineUserCacheService _onlineUserCacheService;

        public UserController(Application.Interfaces.Services.IAuthApplicationService userAppService , IUserService userService, IOnlineUserCacheService onlineUserCacheService)
        {
            _userAppService = userAppService;
            this._userService = userService;
            _onlineUserCacheService = onlineUserCacheService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto user)
        {
            var result = await _userAppService.RegisterUserAsync(user);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            try
            {
                var result = await _userAppService.LoginUserAsync(login);
                if (result == null)
                    return NoContent(); // HTTP 204
                return StatusCode(200, result);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the login request.");
                return StatusCode(500, new GeneralResponse
                {
                    Status = false,
                    Message = "An error occurred while processing your request."
                });
            }
            
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromQuery] UserRole role)
        {
            var result = await _userAppService.AssignRoleToUserAsync(role.UserId, role.RoleId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            var result = await _userAppService.RefreshTokenAsync(request.RefreshToken);
            if (result == null)
                return StatusCode(401, result);

            return Ok(result);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> Search(string SearchTerm , int CurrentUserId)
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
                return BadRequest("Search term is required.");

            var users = await _userService.SearchUsersAsync(SearchTerm, CurrentUserId);
            return Ok(users);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UserInfoDto userInfo)
        {
            if (userInfo == null || userInfo.Id <= 0)
            {
                return BadRequest(new GeneralResponse
                {
                    Status = false,
                    Message = "Invalid user data provided."
                });
            }

            try
            {
                var result = await _userService.UpdateUserInfo(userInfo);

                return Ok(new GeneralResponse
                {
                    Status = result > 0,
                    Message = result > 0 ? "User updated successfully." : "No changes were made."
                });
            }
            catch (Exception ex)
            {
                
                return StatusCode(StatusCodes.Status500InternalServerError, new GeneralResponse
                {
                    Status = false,
                    Message = "An error occurred while updating user information."
                });
            }
        }


        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserInfo(int userId)
        {
            try
            {
                var userInfo = await _userService.GetUserInfoAsync(userId);

                if (userInfo == null)
                {
                    return NotFound(new { Status = false, Message = "User not found" });
                }

                return Ok(new { Status = true, Data = userInfo });
            }
            catch (Exception ex)
            {
                // Optional: log the exception
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Status = false,
                    Message = "Internal server error"
                });
            }
        }


        [HttpGet("IsAuthenticated")]
        [Authorize]
        public async Task<IActionResult> IsAuthenticated()
        {
            return StatusCode(200, "Authenticated");
        }

        [HttpGet("online-users")]
        [Authorize]
        public async Task<IActionResult> GetOnlineUsers()
        {
            var onlineUsers = await _onlineUserCacheService.GetOnlineUsers();
            return Ok(onlineUsers);
        }
    }

}
