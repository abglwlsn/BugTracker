using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Project
    {
        public Project()
        {
            this.Users = new HashSet<ApplicationUser>();
            this.Tickets = new HashSet<Ticket>();
        }

        public int Id { get; set; }
        public string ProjectManagerId { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTimeOffset Deadline { get; set; }
        [Required]
        public string Description { get; set; }
        public string Version { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ApplicationUser ProjectManager { get; set; }
    }
}