using Application.DTOs.Contact;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IContactService
    {
        Task<int> AddContactAsync(int userId, int contactUserId);
        Task<List<ContactResponseDTO>> GetContactsAsync(int userId);
        Task<List<ContactResponseDTO>> SearchUsersContact(string searchTerm, int currentUserId);

    }

}
