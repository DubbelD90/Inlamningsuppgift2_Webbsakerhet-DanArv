using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inlamningsuppgift2_Webbsakerhet_DanArv.Models
{
    public class ApplicationFile
    {
        public Guid Id { get; set; }
        public string UntrustedName { get; set; }
        public DateTime TimeStamp { get; set; }
        public long Size { get; set; }
        public byte[] Content { get; set; }
    }
}
