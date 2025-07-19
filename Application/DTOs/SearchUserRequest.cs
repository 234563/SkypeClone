using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SearchUserRequest
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int CurrentUserId { get; set; }
    }

    public class SearchUserResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

}
