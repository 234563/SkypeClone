using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Contact
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ContactUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
        public User? ContactUser { get; set; }
    }
}
