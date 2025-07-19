using Application.DTOs.Contact;
using Application.Interfaces.Services;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContactsController(IUserService _userService , IContactService contactService) : Controller
    {
       

        //[HttpGet("search")]
        //public async Task<IActionResult> Search(string SearchTerm, int CurrentUserId)
        //{
        //    if (string.IsNullOrWhiteSpace(SearchTerm))
        //        return BadRequest("Search term is required.");

        //    var users = await _userService.SearchUsersAsync(SearchTerm, CurrentUserId);
        //    return Ok(users);
        //}


        [HttpPost]
        public async Task<IActionResult> AddContact(AddContactRequestDto req)
        {
            var id = await contactService.AddContactAsync(req.UserId, req.ContactUserId);
            return id > 0 ? Ok(new { message = "Contact added", ContactId = id , status = true}) : BadRequest("Failed to add contact");
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetContacts(int userId)
        {
            var contacts = await contactService.GetContactsAsync(userId);
            return Ok(contacts);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUserContact(string searchTerm, int currentUserId)
        {
            var contacts = await contactService.SearchUsersContact(searchTerm, currentUserId);
            
            return Ok(contacts);
        }


    }
}
