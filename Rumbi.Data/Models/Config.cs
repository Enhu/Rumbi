using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rumbi.Data.Models
{
    public class Config
    {
        public int Id { get; set; }
        public int TwitchAccessToken { get; set; }
        public DateTime TwitchTokenExpirationDate { get; set; }
    }
}
