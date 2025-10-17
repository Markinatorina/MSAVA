using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSAVA_Shared.Models
{
    public class FetchFileTwitterDTO
    {
        public string TweetUrl { get; set; } = string.Empty;
        public string BearerToken { get; set; } = string.Empty; 
        public Guid AccessGroupId { get; set; }
        public List<string>? Tags { get; set; } = null!;
        public List<string>? Categories { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool PublicViewing { get; set; } = false;
        public bool PublicDownload { get; set; } = false;
    }
}
