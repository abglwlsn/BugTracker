using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class TicketHelpers
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static IEnumerable<Ticket> ListUserTickets(this string userId)
        {
            var tickets = db.Tickets.OrderBy(o=>o.PriorityId).Where(t => t.AssignedToId == userId && t.Status.Name != "Resolved").ToList();

            return tickets;
        }

        public static void CreateTicketChangeLog(this int ticketId, string userId, string property, string newValue, string oldValue)
        {
            Log newLog = new Log()
            {
                TicketId = ticketId,
                ModifiedById = userId,
                Modified = DateTimeOffset.Now,
                Property = property,
                NewValue = newValue,
                OldValue = oldValue
            };
            db.Logs.Add(newLog);
            db.SaveChanges();
        }
    }
}