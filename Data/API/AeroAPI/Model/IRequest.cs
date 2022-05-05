using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LFS.Data.API.AeroAPI.Model
{
    internal interface IRequest
    {
        Uri Uri { get; }
    }
}
