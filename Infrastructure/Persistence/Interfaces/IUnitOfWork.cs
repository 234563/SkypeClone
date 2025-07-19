using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IUserRoleRepository UserRole { get; }
        IContactRepository Contacts { get; }
        IChatRoomRepository ChatRooms { get; }  
        IMessageRepository Messages { get; }
        //IRoleRepository Roles { get; }
        //IUserRoleRepository UserRoles { get; }

        Task<int> SaveChangesAsync(); 
    }
}
