using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rumbi.Data.Models
{
    public class FAQ
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string Identifier { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
    }
}