using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Rumbi.Data.Models
{
    public class Announcement
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public ulong MessageId { get; set; }
        public AnnouncementAttachment? Attachment { get; set; }
    }
}
