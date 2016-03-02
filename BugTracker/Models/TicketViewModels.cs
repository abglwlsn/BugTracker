using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class TicketViewModel
    {
        //bring in whole ticket object to access those properties directly. assign to ticket.Id in get action. in post, a little more complex to access embedded ticket object  - will have to figure this out.
        public Ticket Ticket { get; set; }

        public IEnumerable<SelectListItem> Projects { get; set; }
        public IEnumerable<SelectListItem> Developers { get; set; }
        public IEnumerable<SelectListItem> Priorities { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }
        public IEnumerable<SelectListItem> Actions { get; set; }
    }
}