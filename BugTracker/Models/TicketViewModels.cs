using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class CreateTicketViewModel
    {
        public IEnumerable<SelectListItem> Projects { get; set; }
        public int SelectedProjectId { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }
        public int SelectedTypeId { get; set; }
        public IEnumerable<SelectListItem> Actions { get; set; }
        public int SelectedActionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class EditTicketViewModel
    {
        //bring in whole ticket object to access those properties directly. assign to ticket.Id in get action. in post, a little more complex to access embedded ticket object  - will have to figure this out.
        Ticket Ticket
        public IEnumerable<SelectListItem> Projects { get; set; }
        public int SelectedProjectId { get; set; }
        public IEnumerable<SelectListItem> Developers { get; set; }
        public string SelectedDeveloperId { get; set; }
        public IEnumerable<SelectListItem> Priorities { get; set; }
        public int SelectedPriorityId { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public int SelectedStatusId
        {
            get; set;
        }
           
        public IEnumerable<SelectListItem> Types { get; set; }
        public int SelectedTypeId { get; set; }
        public IEnumerable<SelectListItem> Actions { get; set; }
        public int SelectedActionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        // The items:
        public IEnumerable<SelectListItem> Items
        {
            get
            {
                var allItems = _items.Select(i => new SelectListItem
                {
                    Value = i.Value,
                    Text = i.Text
                });
                return DefaultItem.Concat(allItems);
            }
        }
    }
}