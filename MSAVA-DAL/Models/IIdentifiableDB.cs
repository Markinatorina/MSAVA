﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public interface IIdentifiableDB
    {
        public Guid Id { get; set; }
    }
}
