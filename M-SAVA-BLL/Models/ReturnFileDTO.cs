using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Models
{
    public class ReturnFileDTO
    {
        public required Guid Id { get; set; }
        public required string FileName { get; set; }
        public required string FileExtension { get; set; }
        public required FileStreamResult FileStream { get; set; }
    }
}
