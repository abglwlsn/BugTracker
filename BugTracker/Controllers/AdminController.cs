using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.HelperExtensions;
using Microsoft.AspNet.Identity;
using System.Collections;

namespace BugTracker.Controllers
{
    [RequireHttps]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Users
        [Authorize(Roles = "Project Manager, Administrator")]
        public ActionResult Users()
        {
            return View();
        }

        //GET: Admin/Users
        [Authorize(Roles = "Project Manager, Administrator")]
        public PartialViewResult _UserInfo(string userId)
        {
            var model = new UserInfoViewModel(userId);
            var devTickets = db.Tickets.OrderBy(t => t.PriorityId).Where(t => t.AssignedToId == userId && t.Status.Name != "Resolved");

            model.AssignedProjects = userId.ListUserProjects();
            model.AssignedTickets = userId.ListUserTickets();

            return PartialView(model);
        }

        //GET: Admin/Users
        [Authorize(Roles = "Administrator")]
        public PartialViewResult _EditUserRoles(string userId)
        {
            var model = new AddRemoveRolesViewModel();
            model.UserId = userId;
            model.SelectedRoles = userId.ListUserRoles().ToArray();
            model.Roles = new MultiSelectList(db.Roles, "Name", "FullName", model.SelectedRoles);

            return PartialView(model);
        }

        //POST: Admin/Users/EditUserRoles/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
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
                }

                //notify user of role change
                var es = new EmailService();
                var user = db.Users.Find(model.UserId);
                var msg = user.CreateUserRolesModifiedMessage();
                es.Send(msg);

                ViewBag.SuccessMessage = "User roles have been successfully updated.";
                return RedirectToAction("Users");
            }

            ViewBag.ErrorMessage = "Something went wrong. Please try again, or contact tech support.";
            return RedirectToAction("Users");
        }

        //GET: Admin/Users
        [Authorize(Roles = "Project Manager, Administrator")]
        public PartialViewResult _AssignUserToTicket(string userId)
        {
            var model = new AssignUserViewModel();
            model.UserId = userId;
            model.Projects = db.Projects.Where(p=>p.IsSoftDeleted != true).ToList();
            model.Tickets = db.Tickets.Where(t=>t.Status.Name != "Resolved").ToList();
            model.CurrentTickets = db.Tickets.Where(t => t.AssignedToId == userId).ToList();

            return PartialView(model);
        }

        //POST: Admin/Users/AssignUserToTicket/5
        [Authorize(Roles="Project Manager, Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignUserToTicket(AssignUserViewModel model)
        {
            var original = db.Tickets.AsNoTracking().FirstOrDefault(t=>t.Id == model.SelectedTicket.Id);
            var ticket = db.Tickets.Find(model.SelectedTicket.Id);
            var userId = User.Identity.GetUserId();

            ticket.AssignedToId = model.UserId;
            ticket.Status = db.Statuses.FirstOrDefault(s => s.Name == "Assigned");

            //notify developers
            var developer = db.Users.Find(ticket.AssignedToId);
            var es = new EmailService();
            var msg = ticket.CreateAssignedToTicketMessage(developer);
            es.SendAsync(msg);
        
            var oldDeveloper = db.Users.Find(original.AssignedToId);
            var msg2 = original.CreateAssignmentRemovedMessage(oldDeveloper);
            es.SendAsync(msg2);

            //add changelog
            ticket.Id.CreateTicketChangeLog(userId, "Assigned To", ticket.AssignedTo.FullName, original.AssignedTo.FullName);

            //add notifications
            ticket.Id.CreateTicketNotification("Ticket Assigned", new List<string>{developer.Id}, msg.Body);
            ticket.Id.CreateTicketNotification("Ticket Reassigned", new List<string> { oldDeveloper.Id }, msg2.Body);

            db.SaveChanges();
            ViewBag.SuccessMessage = "Ticket: " + ticket.Name + "has been reassigned, and a notification has been sent to the developers.";

            return RedirectToAction("Users");
        }

        //GET: Admin/Roles
        [Authorize(Roles = "Administrator")]
        public ActionResult Roles()
        {
            return View();
        }
    }
}