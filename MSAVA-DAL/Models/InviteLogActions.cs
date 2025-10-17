﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public enum InviteLogActions : short
    {
        Unknown = 0,

        // Invite code actions
        InviteCodeCreated = 1,
        InviteCodeDeleted = 2,
        InviteCodeExpired = 3,
    }
}
