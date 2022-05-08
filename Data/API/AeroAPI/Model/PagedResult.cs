using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LFS.Data.API.AeroAPI.Model
{
    public class PagedResult
    {
        [JsonPropertyName("links")]
        public IDictionary<string, string> Links { get; set; }

        [JsonPropertyName("num_pages")]
        public int? NumberOfPages { get; set; }
    }
}
