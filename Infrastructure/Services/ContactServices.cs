using Application.DTOs.Contact;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{

    public class ContactService(IUnitOfWork unitOfWork) : IContactService
    {
        
        public Task<int> AddContactAsync(int userId, int contactUserId)
        {
            return unitOfWork.Contacts.AddContactAsync(userId, contactUserId);
        }

        public Task<List<ContactResponseDTO>> GetContactsAsync(int userId)
        {
            return unitOfWork.Contacts.GetContactsByUserIdAsync(userId);
        }

        public Task<List<ContactResponseDTO>> SearchUsersContact(string searchTerm, int currentUserId)
        {
            return unitOfWork.Contacts.SearchUsersContact(searchTerm, currentUserId);
        }
    }
}
