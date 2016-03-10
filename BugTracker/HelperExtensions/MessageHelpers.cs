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
        public static IdentityMessage CreateAssignedToTicketMessage(this Ticket ticket, ApplicationUser user)
        {
            var manager = ticket.Project.ProjectManagerId.GetProjectManager();
            var msg = new IdentityMessage();
            msg.Destination = user.Email;
            msg.Body = "A new ticket has been assigned to you by " + manager.FullName + ". Ticket details are below. <br/><br/> Project: " + ticket.Project.Name + "<br/> Project Due Date: " + ticket.Project.Deadline + "<br/> Ticket Name: " + ticket.Name + "<br/> Description: " + ticket.Description + "<br/> Submitter: " + ticket.Submitter.FullName + "<br/> Priority: " + ticket.Priority.Name + "<br/> Action: " + ticket.Action.Name + "<br/> Phase: " + ticket.Phase.Name + "<br/><br/>If you have questions or cannot complete this ticket, please contact " + manager.FirstName + "at" + manager.Email + ".";
            msg.Subject = "New ticket assignment on project " + ticket.Project.Name;

            return msg;
        }

        public static IdentityMessage CreateAssignedToProjectMessage(this Project project, ApplicationUser user)
        {
            var manager = project.ProjectManagerId.GetProjectManager();
            var msg = new IdentityMessage();
           msg.Destination = user.Email;
           msg.Body = "You have recently been assigned to a new project. Project details are below. <br/><br/> Project: " + project.Name + "<br/> Project Due Date: " + project.Deadline.FormatDateTimeOffsetCondensed() + "<br/> Open Tickets: " + project.Tickets.Count() + "<br/><br/>If you have questions,, please contact " + manager.FirstName + "at" + manager.Email + ".";
           msg.Subject = "New project assignment: " + project.Name;

            return msg;
        }

        public static IdentityMessage CreateRemovedFromProjectMessage(this Project project, ApplicationUser user)
        {
            var manager = project.ProjectManagerId.GetProjectManager();
            var msg = new IdentityMessage();
           msg.Destination = user.Email;
           msg.Body = "You have recently been removed from a project. Project details are below. <br/><br/> Project: " + project.Name + "<br/> Project Due Date: " + project.Deadline.FormatDateTimeOffsetCondensed() + "<br/> Open Tickets: " + project.Tickets.Count() + "<br/><br/>If you have questions, please contact " + manager.FirstName + "at" + manager.Email + ".";
           msg.Subject = "Removed from Project: " + project.Name;

            return msg;
        }

        public static IEnumerable<IdentityMessage> CreateNewProjectManagerMessage(this Project project, IEnumerable<ApplicationUser> developers)
        {
            var manager = project.ProjectManagerId.GetProjectManager();
            var msgList = new List<IdentityMessage>();
            foreach(var developer in developers)
            {
                var msg = new IdentityMessage();
                msg.Destination = developer.Email;
                msg.Body = "A new project manager has recently been assigned to one of your projects. Details are below. <br/><br/> Project: " + project.Name + "<br/> New Project Manager: " + manager.FullName + "<br/> Project Due Date: " + project.Deadline.FormatDateTimeOffsetCondensed() + "<br/> Open Tickets: " + project.Tickets.Count() + "<br/><br/>If you have questions, please contact " + manager.FirstName + "at" + manager.Email + ".";
                msg.Subject = "New Project Manager on project " + project.Name;
            }
            return msgList;
        }

        public static IdentityMessage CreateAssignmentRemovedMessage(this Ticket ticket, ApplicationUser user)
        {
            var manager = ticket.Project.ProjectManagerId.GetProjectManager();
            var msg = new IdentityMessage();
            msg.Destination = user.Email;
            msg.Body = "One of your tickets has been reassigned to a new developer.  Details are below. <br/><br/> Name: " + ticket.Name + "<br/> Project: " + ticket.Project.Name + "<br/><br/> If you have questions about this reassignment, please contact the Project Manager, " + manager.FullName + " at " + manager.Email + ".";
            msg.Subject = "Ticket: " + ticket.Name + " has been reassigned.";

            return msg;
        }

        public static IEnumerable<IdentityMessage> CreateTicketResolvedMessage(this Ticket ticket, IEnumerable<ApplicationUser> recipients)
        {
            var manager = ticket.Project.ProjectManagerId.GetProjectManager();
            var msgList = new List<IdentityMessage>();
            foreach (var recipient in recipients)
            {
                var msg = new IdentityMessage();
                msg.Destination = recipient.Email;
                msg.Body = "One of your tickets has been reassigned to a new developer.  Details are below. <br/><br/> Name: " + ticket.Name + "<br/> Project: " + ticket.Project.Name + "<br/><br/> If you have questions about this reassignment, please contact the Project Manager, " + manager.FullName + " at " + manager.Email + ".";
                msg.Subject = "Ticket: " + ticket.Name + " has been reassigned.";
            }
            return msgList;
        }

        public static IdentityMessage CreateTicketIsExplosiveMessage(this Ticket ticket, ApplicationUser developer, ApplicationUser projectManager)
        {
            var msg = new IdentityMessage();
            return msg;
        }

        public static IdentityMessage CreateUserRolesModifiedMessage(this ApplicationUser user)
        {
            var msg = new IdentityMessage();
            return msg;
        }

    }
}