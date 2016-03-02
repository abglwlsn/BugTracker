using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class MessageHelpers
    {
        public static IdentityMessage CreateAssignedToMessage(this ApplicationUser user, Ticket ticket)
        {
            var msg = new IdentityMessage();
            msg.Destination = user.Email;
            msg.Body = "A new ticket has been assigned to you by " + ticket.Project.ProjectManager.FullName + ". Ticket details are below. <br/><br/> Project: " + ticket.Project.Name + "<br/> Project Due Date: " + ticket.Project.Deadline + "<br/> Ticket Name: " + ticket.Name + "<br/> Description: " + ticket.Description + "<br/> Submitter: " + ticket.Submitter.FullName + "<br/> Priority: " + ticket.Priority.Name + "<br/> Action: " + ticket.Action.Name + "<br/> Phase: " + ticket.Phase.Name + "<br/><br/>If you have questions or cannot complete this ticket, please contact " + ticket.Project.ProjectManager.FirstName + "at" + ticket.Project.ProjectManager.Email + ".";
            msg.Subject = "New ticket assignment";

            return msg;
        }

        public static IdentityMessage CreateAssignmentRemovedMessage(this ApplicationUser user, Ticket ticket)
        {
            var msg = new IdentityMessage();
            msg.Destination = user.Email;
            msg.Body = "One of your assigned tickets has been reassigned to a new developer.  Details are below. <br/><br/> Name: " + ticket.Name + "<br/> Project: " + ticket.Project.Name + "<br/><br/> If you have questions about this reassignment, please contect the Project Manager, " + ticket.Project.ProjectManager.FullName + " at " + ticket.Project.ProjectManager.Email + ".";
            msg.Subject = "Ticket: " + ticket.Name + " has been reassigned.";

            return msg;
        }
    }
}