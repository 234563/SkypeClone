using Application.DTOs.Contact;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Interfaces
{
    public interface IContactRepository
    {
        Task<int> AddContactAsync(int userId, int contactUserId);
        Task<List<ContactResponseDTO>> GetContactsByUserIdAsync(int userId);
        Task<List<ContactResponseDTO>> SearchUsersContact(string searchTerm, int currentUserId);
    }
}
