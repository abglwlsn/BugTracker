using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string SubmitterId { get; set; }
        public string Title { get; set; }
        [StringLength(100, ErrorMessage ="Description must be fewer than 100 characters. For a longer message, please leave a comment.")]
        public string Description { get; set; }
        public string FilePath { get; set; }
        public DateTimeOffset Submitted { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser Submitter { get; set; }
    }
}