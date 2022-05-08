using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LFS.Data.API.AeroAPI.Model
{
    public partial class ResultList<_T> : PagedResult
    {
        public virtual IList<_T> Results { get; set; }
    }
}
