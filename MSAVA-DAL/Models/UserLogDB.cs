﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class UserLogDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Guid UserId { get; set; }
        public UserDB? User { get; set; }
        public Guid? AdminId { get; set; }
        public UserDB? Admin { get; set; }
        public UserLogAction Action { get; set; } = UserLogAction.Unknown;
    }
}
