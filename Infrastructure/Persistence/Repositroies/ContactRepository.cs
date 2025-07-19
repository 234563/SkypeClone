using Infrastructure.Common.Interfaces;
using System.Data.SqlClient;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Data;
using Infrastructure.Persistence.Interfaces;
using Skype.Infrastructure.Common;
using Infrastructure.Common;
using Application.DTOs.Contact;
using Application.DTOs;

public class ContactRepository : DbHelper, IContactRepository
{
    private readonly ILogger<ContactRepository> logger;

    public ContactRepository(ISqlConnectionFactory sqlConnectionFactory, ILogger<ContactRepository> _logger)
        : base(sqlConnectionFactory)
    {
        logger = _logger;
    }

    public async Task<int> AddContactAsync(int userId, int contactUserId)
    {
        try
        {
            var parameters = new[]
            {
                new SqlParameter("@UserIdA", userId),
                new SqlParameter("@UserIdB", contactUserId)
            };

            var table = await ExecuteStoredProcedureAsync("AddContact", parameters);
            return table.Rows.Count > 0 ? Convert.ToInt32(table.Rows[0]["AffectedRows"]) : 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding contact.");
            throw;
        }
    }

    public async Task<List<ContactResponseDTO>> GetContactsByUserIdAsync(int userId)
    {
        var contacts = new List<ContactResponseDTO>();

        try
        {
            var parameters = new[] { new SqlParameter("@UserId", userId) };
            var dt = await ExecuteStoredProcedureAsync("GetContactsByUserId", parameters);

            foreach (DataRow row in dt.Rows)
            {
                contacts.Add(new ContactResponseDTO
                {
                    Id = row.SafeGet<int>("Id"),
                    FullName = row.SafeGet<string>("FullName"),
                    Email = row.SafeGet<string>("Email"),
                    IsOnline = row.SafeGet<bool>("IsOnline"),
                    IsContact = row.SafeGet<bool>("IsContact"),
                    ChatAvatar = row.SafeGet<string>("ChatAvatar")
                });
            }

            return contacts;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching contacts.");
            throw;
        }
    }


    public async Task<List<ContactResponseDTO>> SearchUsersContact(string searchTerm, int currentUserId)
    {
        var parameters = new[]
        {
          new SqlParameter("@SearchTerm", searchTerm),
          new SqlParameter("@CurrentUserId", currentUserId)
        };

        var table = await ExecuteStoredProcedureAsync("SearchUsersContact", parameters);

        var results = new List<ContactResponseDTO>();

        foreach (DataRow row in table.Rows)
        {
            results.Add(new ContactResponseDTO
            {
                Id = row.SafeGet<int>("Id"),
                FullName = row.SafeGet<string>("FullName"),
                Email = row.SafeGet<string>("Email"),
                IsContact = row.SafeGet<bool>("IsContact")
            });
        }

        return results;
    }

}
