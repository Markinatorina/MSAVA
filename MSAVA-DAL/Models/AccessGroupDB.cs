﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class AccessGroupDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public required Guid OwnerId { get; set; }
        public UserDB? Owner { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required string Name { get; set; }
        public ICollection<UserDB> Users { get; set; } = new List<UserDB>();
        public IEnumerable<AccessGroupDB>? SubGroups { get; set; }
    }
}