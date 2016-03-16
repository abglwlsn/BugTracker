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

        public static ICollection<Log> CreateTicketChangeLogs(this Ticket oldTicket, Ticket newTicket, string userId)
        {
            var newLogs = new List<Log>();
            var modified = DateTimeOffset.Now;

            if (oldTicket?.AssignedToId != newTicket.AssignedToId)
            {
                var oldDev = db.Users.Find(oldTicket?.AssignedToId);
                var newDev = db.Users.Find(newTicket.AssignedToId);

                var log = new Log
                {
                    TicketId = newTicket.Id,
                    ProjectId = newTicket.ProjectId,
                    ModifiedById = userId,
                    Modified = modified,
                    Property = "Assigned To",
                    OldValue = oldDev.FullName,
                    NewValue = newDev.FullName
                };

                newLogs.Add(log);
            }

            if(oldTicket?.PriorityId != newTicket.PriorityId)
            {
                var oldPri = db.Priorities.Find(oldTicket.PriorityId);
                var newPri = db.Priorities.Find(newTicket.PriorityId);

                Log log = new Log
                {
                    TicketId = newTicket.Id,
                    ProjectId = newTicket.ProjectId,
                    ModifiedById = userId,
                    Modified = modified,
                    Property = "Priority",
                    OldValue = oldPri.Name,
                    NewValue = newPri.Name
                };

                newLogs.Add(log);
            }

            if(oldTicket?.StatusId != newTicket.StatusId)
            {
                var oldStat = db.Statuses.Find(oldTicket.PriorityId);
                var newStat = db.Statuses.Find(newTicket.PriorityId);

                Log log = new Log
                {
                    TicketId = newTicket.Id,
                    ProjectId = newTicket.ProjectId,
                    ModifiedById = userId,
                    Modified = modified,
                    Property = "Status",
                    OldValue = oldStat.Name,
                    NewValue = newStat.Name
                };

                newLogs.Add(log);
            }

            if (oldTicket?.Description != newTicket.Description)
            {
                Log log = new Log
                {
                    TicketId = newTicket.Id,
                    ProjectId = newTicket.ProjectId,
                    ModifiedById = userId,
                    Modified = modified,
                    Property = "Descriptin",
                    OldValue = oldTicket?.Description,
                    NewValue = newTicket.Description
                };

                newLogs.Add(log);
            }

            return newLogs;
        }
    }
}