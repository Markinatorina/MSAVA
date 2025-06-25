using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class JWTDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsBanned { get; set; }
        public bool IsWhitelisted { get; set; }
        public Guid InviteCode { get; set; }
        public string TokenString { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
