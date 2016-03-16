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
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static IEnumerable<IdentityMessage> CreateTicketSubmittedMessage(this Ticket ticket, IEnumerable<ApplicationUser> recipients)
        {
            var _ticket = db.Tickets.Find(ticket.Id);
            var msgList = new List<IdentityMessage>();

            foreach (var user in recipients)
            {
                var msg = new IdentityMessage();
                msg.Destination = user.Email;
                msg.Body = "A new ticket has been submitted by " + _ticket.Submitter.FullName + ". Please review the ticket details to determine validity, and to assign a Project Manager and Developer. <br/><br/>Ticket: " + _ticket.Name + "<br/> <br/>Submitted: " + _ticket.Submitted.FormatDateTimeOffset() + "<br/><br/>Description: " + _ticket.Description;

                msgList.Add(msg);
            }


            return msgList;
        }

        public static IdentityMessage CreateAssignedToTicketMessage(this Ticket ticket, Project project, ApplicationUser user)
        {
            var _ticket = db.Tickets.Find(ticket.Id);
            var manager = db.Users.Find(project.ProjectManagerId);
            var msg = new IdentityMessage();
            msg.Destination = user.Email;
            msg.Body = "A new ticket has been assigned to you by " + manager.FullName + ". Ticket details are below. <br/><br/> Project: " + project.Name + "<br/> Project Due Date: " + project.Deadline + "<br/> Ticket Name: " + _ticket.Name + "<br/> Description: " + _ticket.Description + "<br/> Submitter: " + _ticket.Submitter.FullName + "<br/> Priority: " + _ticket.Priority.Name + "<br/> Action: " + _ticket.Action.Name + "<br/> Phase: " + _ticket.Phase.Name + "<br/><br/>If you have questions or cannot complete this ticket, please contact " + manager.FirstName + "at" + manager.Email + ".";
            msg.Subject = "New ticket assignment on project " + project.Name;

            return msg;
        }

        public static IdentityMessage CreateAssignedToProjectMessage(this Project project, ApplicationUser user)
        {
            var manager = db.Users.Find(project.ProjectManagerId);
            var msg = new IdentityMessage();
           msg.Destination = user.Email;
           msg.Body = "You have recently been assigned to a new project. Project details are below. <br/><br/> Project: " + project.Name + "<br/> Project Due Date: " + project.Deadline.FormatDateTimeOffsetCondensed() + "<br/> Open Tickets: " + project.Tickets.Count() + "<br/><br/>If you have questions,, please contact " + manager.FirstName + "at" + manager.Email + ".";
           msg.Subject = "New project assignment: " + project.Name;

            return msg;
        }

        public static IdentityMessage CreateRemovedFromProjectMessage(this Project project, ApplicationUser user)
        {
            var manager = db.Users.Find(project.ProjectManagerId);
            var msg = new IdentityMessage();
           msg.Destination = user.Email;
           msg.Body = "You have recently been removed from a project. Project details are below. <br/><br/> Project: " + project.Name + "<br/> Project Due Date: " + project.Deadline.FormatDateTimeOffsetCondensed() + "<br/> Open Tickets: " + project.Tickets.Count() + "<br/><br/>If you have questions, please contact " + manager.FirstName + "at" + manager.Email + ".";
           msg.Subject = "Removed from Project: " + project.Name;

            return msg;
        }

        public static IEnumerable<IdentityMessage> CreateNewProjectManagerMessage(this Project project, IEnumerable<ApplicationUser> developers)
        {
            var manager = db.Users.Find(project.ProjectManagerId);
            var msgList = new List<IdentityMessage>();
            foreach(var developer in developers)
            {
                var msg = new IdentityMessage();
                msg.Destination = developer.Email;
                msg.Body = "A new project manager has recently been assigned to one of your projects. Details are below. <br/><br/> Project: " + project.Name + "<br/> New Project Manager: " + manager.FullName + "<br/> Project Due Date: " + project.Deadline.FormatDateTimeOffsetCondensed() + "<br/> Open Tickets: " + project.Tickets.Count() + "<br/><br/>If you have questions, please contact " + manager.FirstName + "at" + manager.Email + ".";
                msg.Subject = "New Project Manager on project " + project.Name;

                msgList.Add(msg);
            }
            return msgList;
        }

        public static IdentityMessage CreateTicketReassignedMessage(this Ticket ticket, Project project, ApplicationUser user)
        {
            var manager = db.Users.Find(project.ProjectManagerId);
            var msg = new IdentityMessage();
            msg.Destination = user.Email;
            msg.Body = "One of your tickets has been reassigned to a new developer.  Details are below. <br/><br/> Name: " + ticket.Name + "<br/> Project: " + project.Name + "<br/><br/> If you have questions about this reassignment, please contact the Project Manager, " + manager.FullName + " at " + manager.Email + ".";
            msg.Subject = "Ticket: " + ticket.Name + " has been reassigned.";

            return msg;
        }

        public static IdentityMessage CreateTicketModifiedMessage(this Ticket ticket, ApplicationUser user)
        {
            var manager = db.Users.Find(ticket.Project.ProjectManagerId);
            var msg = new IdentityMessage();
            msg.Destination = user.Email;
            msg.Body = "The ticket " + ticket.Name + " in project " + ticket.Project.Name + " has been modified. Please review the changelog to see if any action is necessary. View the ChangeLog <a href=\"https://awest-bugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id + "\">here.</a>";
            msg.Subject = "Ticket: " + ticket.Name + " has been modified.";

            return msg;
        }

        public static IdentityMessage CreateReassignmentRequestedMessage(this Ticket ticket, ApplicationUser user, string request)
        {
            var manager = db.Users.Find(ticket.Project.ProjectManagerId);
            var msg = new IdentityMessage();
            msg.Destination = manager.Email;
            msg.Body = "The developer " + user.FullName + " for ticket " + ticket.Name + " has requested that the ticket be reassigned. Please review the request below and either fulfill the request or contact the developer. If the developer indicates that the ticket is ready to advance to the next stage of development, review current work for errors before reassigning.<br/><br/> Request: " + request + "<br/><br/>View ticket details <a href=\"https://awest-bugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id + "\">here</a>.";
            msg.Subject = "Reassignment requested for ticket " + ticket.Name;

            return msg;
        }

        public static IEnumerable<IdentityMessage> CreateTicketResolvedMessage(this Ticket ticket, IEnumerable<ApplicationUser> recipients)
        {
            var manager = db.Users.Find(ticket.Project.ProjectManagerId);
            var msgList = new List<IdentityMessage>();
            foreach (var recipient in recipients)
            {
                var msg = new IdentityMessage();
                msg.Destination = recipient.Email;
                msg.Body = "One of your tickets has been reassigned to a new developer.  Details are below. <br/><br/> Name: " + ticket.Name + "<br/> Project: " + ticket.Project.Name + "<br/><br/> If you have questions about this reassignment, please contact the Project Manager, " + manager.FullName + " at " + manager.Email + ".";
                msg.Subject = "Ticket: " + ticket.Name + " has been reassigned.";

                msgList.Add(msg);
            }
            return msgList;
        }

        //public static IdentityMessage CreateTicketIsExplosiveMessage(this Ticket ticket, ApplicationUser developer, ApplicationUser projectManager)
        //{
        //    var msg = new IdentityMessage();
        //    return msg;
        //}

        //public static IdentityMessage CreateUserRolesModifiedMessage(this ApplicationUser user)
        //{
        //    var msg = new IdentityMessage();
        //    return msg;
        //}

    }
}