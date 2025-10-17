using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSAVA_Shared.Models
{
    public class LoginRequestDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
