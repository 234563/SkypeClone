using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Contact
{
    public class ContactResponseDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ChatAvatar { get; set; } //= "https://randomuser.me/api/portraits/women/44.jpg";
        public bool IsContact { get; set; }
        public bool IsOnline { get; set; } = false;
    }
}
