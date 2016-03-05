using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class CreateEditTicketViewModel
    {
        //bring in whole ticket object to access those properties directly. assign to ticket.Id in get action. in post, a little more complex to access embedded ticket object  - will have to figure this out.
        public Ticket Ticket { get; set; }
        public Project Project { get; set; }

        public SelectList Projects { get; set; }
        public SelectList Developers { get; set; }
        public SelectList Priorities { get; set; }
        public SelectList Statuses { get; set; }
        public SelectList Phases { get; set; }
        public SelectList Actions { get; set; }
    }
}