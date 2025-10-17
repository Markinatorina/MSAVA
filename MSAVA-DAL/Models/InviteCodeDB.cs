﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class InviteCodeDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public required Guid OwnerId { get; set; }
        public UserDB? Owner { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime ExpiresAt { get; set; }
        public required int MaxUses { get; set; }
    }
}
