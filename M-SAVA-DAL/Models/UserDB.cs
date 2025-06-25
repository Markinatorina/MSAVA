using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class UserDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public required string Username { get; set; }
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBanned { get; set; }
        public bool IsWhitelisted { get; set; }
        public Guid InviteCode { get; set; }
    }
}
