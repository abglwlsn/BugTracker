using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string SubmitterId { get; set; }
        [Required]
        [StringLength(500, ErrorMessage = "Comment must be between {2} and {1} characters.", MinimumLength = 5)]
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }
        public DateTimeOffset Submitted { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser Submitter { get; set; }
    }
}