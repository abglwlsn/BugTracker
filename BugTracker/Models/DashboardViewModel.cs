using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Ticket> Tickets { get; set; }
        public IEnumerable<Attachment> Attachments { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public int ProjectsAmt { get; set; }
    }
}