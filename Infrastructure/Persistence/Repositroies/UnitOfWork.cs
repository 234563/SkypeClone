using Infrastructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositroies
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserRepository Users { get; }

        public IRefreshTokenRepository RefreshTokens { get; }

        public IUserRoleRepository UserRole { get; }

        public IContactRepository Contacts  { get; }

        public IChatRoomRepository ChatRooms { get; }

        public IMessageRepository Messages { get; }

        //public IRoleRepository Roles { get; }
        //public IUserRoleRepository UserRoles { get; }

        #region Constructor
        public UnitOfWork(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokens,
            IUserRoleRepository userRole,
            IContactRepository contactRepository ,
            IChatRoomRepository chatRoom,
            IMessageRepository messages
            //IUserRoleRepository userRoleRepository
            )
        {
            Users = userRepository;
            RefreshTokens = refreshTokens;
            UserRole = userRole;
            Contacts = contactRepository;
            ChatRooms = chatRoom;
            Messages = messages;
            //Roles = roleRepository;
            //UserRoles = userRoleRepository;
        }

        #endregion

        public Task<int> SaveChangesAsync()
        {
            // For ADO.NET, this may not be needed unless you're batching commands
            return Task.FromResult(0);
        }
    }

}
