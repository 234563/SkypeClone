using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public class UserInfoDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public bool IsOnline { get; set; }
        public string? UserRole { get; set; }
        public bool EnableNotification { get; set; }
        public bool AppearInSearchResult { get; set; }
        public bool ShowOnlineStatus { get; set; }
        public int DefaultTheme { get; set; }
        public string? ChatAvatar { get; set; }
        public int ChatRoomID { get; set; }
    }

}
