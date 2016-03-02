using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.HelperExtensions;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    [Authorize(Roles ="Administrator")]
    [RequireHttps]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Users
        public ActionResult Users()
        {
            return View();
        }

        //GET: Admin/Users
        [Authorize(Roles="Project Manager")]
        public PartialViewResult _UserInfo(string userId)
        {
            var model = new UserInfoViewModel();
            model.User = db.Users.Find(userId);
            model.Roles = userId.ListUserRoles();
            if (userId.UserIsInRole("Developer"))
                model.AssignedTickets = db.Tickets.OrderBy(t=>t.PriorityId).Where(t => t.AssignedToId == userId);
            if (userId.UserIsInRole("Project Manager"))
                model.AssignedProjects = db.Projects.OrderByDescending(p=>p.Deadline).Where(p => p.ProjectManagerId == userId);

            return PartialView(model);
        }

        //GET: Admin/Users
        public PartialViewResult _EditUserRoles(string userId)
        {
            var model = new AddRemoveRolesViewModel();
            model.UserId = userId;
            model.SelectedRoles = userId.ListUserRoles().ToArray();
            model.Roles = new MultiSelectList(db.Roles, "Name", "Name", model.SelectedRoles);

            return PartialView(model);
        }

        //POST: Admin/Users/EditUserRoles/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUserRoles(AddRemoveRolesViewModel model)
        {
            if(ModelState.IsValid)
            {
                foreach (var role in db.Roles.Select(r=>r.Name))
                {
                    if (model.SelectedRoles.Contains(role))
                        model.UserId.AddUserToRole(role);
                    else
                        model.UserId.RemoveUserFromRole(role);
                    //UserManager handles validation for us, ex: user is already in role when add is attempted. Fails silently.

                    ViewBag.SuccessMessage = "User roles have been successfully updated.";

                    return RedirectToAction("Users");
                }
            }

            ViewBag.ErrorMessage = "Something went wrong. Please try again, or contact tech support.";
            return RedirectToAction("Users");
        }

        //GET: Admin/Users
        [Authorize(Roles="Project Manager")]
        public PartialViewResult _AssignUserToTicket(string userId)
        {
            var model = new AssignUserViewModel();
            model.UserId = userId;
            model.Projects = db.Projects.ToList();
            model.Tickets = db.Tickets.ToList();
            model.CurrentTickets = db.Tickets.Where(t => t.AssignedToId == userId).ToList();

            return PartialView(model);
        }

        //POST: Admin/Users/AssignUserToTicket/5
        [Authorize(Roles="Project Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignUserToTicket(AssignUserViewModel model)
        {
            var originalTicket = db.Tickets.AsNoTracking().FirstOrDefault(t=>t.Id == model.SelectedTicket.Id);
            var ticket = db.Tickets.Find(model.SelectedTicket.Id);

            ticket.AssignedToId = model.UserId;
            ticket.Status = db.Statuses.FirstOrDefault(s => s.Name == "Assigned");

            //notify developers
            var developer = db.Users.Find(ticket.AssignedToId);
            var es = new EmailService();
            var msg = developer.CreateAssignedToMessage(ticket);
            es.SendAsync(msg);
        
            var oldDeveloper = db.Users.Find(originalTicket.AssignedToId);
            var msg2 = oldDeveloper.CreateAssignmentRemovedMessage(originalTicket);
            es.SendAsync(msg2);

            //add changelog
            Log newLog = new Log()
            {
                TicketId = ticket.Id,
                ModifiedById = User.Identity.GetUserId(),
                Modified = DateTimeOffset.Now,
                Property = "Assigned To",
                NewValue = ticket.AssignedTo.FullName,
                OldValue = originalTicket.AssignedTo.FullName
            };
            db.Logs.Add(newLog);

            //add notifications
            Notification assigned = new Notification()
            {
                TicketId = ticket.Id,
                TypeId = db.NotificationTypes.FirstOrDefault(n => n.Name == "Ticket Assigned").Id,
                RecipientId = developer.Id,
                SendDate = DateTimeOffset.Now,
                Message = msg.Body
            };

            Notification reassigned = new Notification()
            {
                TicketId = ticket.Id,
                TypeId = db.NotificationTypes.FirstOrDefault(n => n.Name == "Ticket Reassigned").Id,
                RecipientId = oldDeveloper.Id,
                SendDate = DateTimeOffset.Now,
                Message = msg2.Body
            };

            db.SaveChanges();

            ViewBag.SuccessMessage = "Ticket: " + ticket.Name + "has been reassigned, and a notification has been sent to the developers.";

            return RedirectToAction("Users");
        }

        //GET: Admin/Roles
        public ActionResult Roles()
        {
            return View();
        }
    }
}