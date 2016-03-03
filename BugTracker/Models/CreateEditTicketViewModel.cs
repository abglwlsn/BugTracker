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

        public SelectListItem Projects { get; set; }
        public SelectListItem Developers { get; set; }
        public SelectListItem Priorities { get; set; }
        public SelectListItem Statuses { get; set; }
        public SelectListItem Types { get; set; }
        public SelectListItem Actions { get; set; }
    }
}