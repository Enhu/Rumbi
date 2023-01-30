using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rumbi.Data.Models
{
    public class TemporalFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public ulong FileId { get; set; }
        public ulong MediumId { get; set; }
    }
}
