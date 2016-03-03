using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public int? TicketId { get; set; }
        public int TypeId { get; set; }
        public IEnumerable<string> RecipientId { get; set; }
        public DateTimeOffset SendDate { get; set; }
        public string Message { get; set; }

        public virtual Project Project { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual NotificationType Type { get; set; }
        public virtual ApplicationUser Recipient { get; set; }
    }
}