using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Refresh_Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

}
