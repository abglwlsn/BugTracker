using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Ticket
    {
        public Ticket()
        {
            this.Logs = new HashSet<Log>();
            this.Comments = new HashSet<Comment>();
            this.Attachments = new HashSet<Attachment>();
        }

        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public string SubmitterId { get; set; }
        public string AssignedToId { get; set; }
        public int? PriorityId { get; set; }
        public int? StatusId { get; set; }
        public int? PhaseId { get; set; }
        public int ActionId { get; set; }
        [Required]
        [StringLength(40, ErrorMessage = "Ticket name must be between {2} and {1} characters.", MinimumLength = 5)]
        public string Name { get; set; }
        public DateTimeOffset Submitted { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public DateTimeOffset Closed { get; set; }
        [Required]
        public string Description { get; set; }

        public virtual Project Project { get; set; }
        public virtual ApplicationUser Submitter { get; set; }
        public virtual ApplicationUser AssignedTo { get; set; }
        public virtual Priority Priority { get; set; }
        public virtual Status Status { get; set; }
        public virtual TicketPhase Phase { get; set; }
        public virtual TicketAction Action { get; set; }
        public virtual ICollection<Log> Logs { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }
    }
}