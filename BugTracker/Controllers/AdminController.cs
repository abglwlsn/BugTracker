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
    //[RequireHttps]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Users
        [Authorize(Roles = "Project Manager, Administrator")]
        public ActionResult Users()
        {
            var users = db.Users.ToList();
            return View(users);
        }

        //GET: Admin/Users
        [Authorize(Roles = "Project Manager, Administrator")]
        //public PartialViewResult _UserInfo(string id)
        public ActionResult UserInfo(string id)
        {
            var model = new UserInfoViewModel(id);
            var devTickets = db.Tickets.OrderBy(t => t.PriorityId).Where(t => t.AssignedToId == id && t.Status.Name != "Resolved");

            model.AssignedProjects = id.ListUserProjects();
            model.AssignedTickets = id.ListUserTickets();
            model.Roles = id.ListUserRoles();

            //return PartialView(model);
            return View(model);
        }

        //GET: Admin/Users/_AddRemoveRole/5
        [Authorize(Roles = "Administrator")]
        //public PartialViewResult _AddRemoveRole(string id)
        public ActionResult AddRemoveRole(string id)
        {
            //var roles = id.ListUserRoles().ToList();
            //var selectedRoles = new List<SelectListItem>();
            //foreach (var role in roles)
            //    selectedRoles.Add(new SelectListItem() { Text = role });

            var model = new AddRemoveRolesViewModel();
            model.User = db.Users.Find(id);
            model.Roles = new MultiSelectList(db.Roles, "Name", "Name", model.SelectedRoles);
            model.SelectedRoles = id.ListUserRoles().ToArray();
            //model.SelectedRoles = selectedRoles;

            //return PartialView(model);
            return View(model);
        }

        //POST: Admin/Users/AddRemoveRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult AddRemoveRole([Bind(Include="User, SelectedRoles")]AddRemoveRolesViewModel model)
        {
            //model.User = db.Users.Find(model.User.Id);

            //var selectedRoles = new List<string>();
            //foreach (var role in model.SelectedRoles)
            //    selectedRoles.Add(role.Text);

            ModelState.Remove("User.FirstName");
            ModelState.Remove("User.LastName");
            //ModelState.Add("SelectedRoles", model.SelectedRoles);

            if(ModelState.IsValid)
            {
                foreach (var role in db.Roles.Select(r=>r.Name))
                {
                    if (model.SelectedRoles.Contains(role))
                        model.User.Id.AddUserToRole(role);
                    else
                        if (model.User.Id.UserIsInRole(role))
                            model.User.Id.RemoveUserFromRole(role);
                    //UserManager handles validation for us, ex: user is already in role when add is attempted. Fails silently.
                }

                //notify user of role change
                //var es = new EmailService();
                //var user = db.Users.Find(model.User.Id);
                //var msg = user.CreateUserRolesModifiedMessage();
                //es.Send(msg);

                ViewBag.SuccessMessage = "User roles have been successfully updated.";
                return RedirectToAction("Users");
            }

            ViewBag.ErrorMessage = "Something went wrong. Please try again, or contact tech support.";
            //return RedirectToAction("Users");
            return View(model);
        }

        //GET: Admin/Roles/_AddRemoveUsers/roleName
        [Authorize(Roles = "Administrator")]
        public PartialViewResult _AddRemoveUsers(string roleName)
        {
            var users = new List<string>();
            foreach (var user in db.Users)
                users.Add(user.FullName);

            var model = new AddRemoveUsersViewModel();
            model.RoleName = roleName;
            model.Users = new MultiSelectList(db.Users, "Id", "FullName", users);

            return PartialView(model);
        }

        //GET: Admin/Users
        [Authorize(Roles = "Project Manager, Administrator")]
        public PartialViewResult _AssignUserToTicket(string id)
        {
            var user = db.Users.Find(id);
            var projects = db.Projects.Where(p => p.Users.Contains(user)).ToList();
            var tickets = db.Tickets.Where(t => t.Status.Name != "Resolved").ToList();

            var model = new AssignUserViewModel();
            model.User = db.Users.Find(id);
            model.Projects = new SelectList(projects, "Id", "Name");
            model.Tickets = new SelectList(tickets, "Id", "Name");
            model.CurrentTickets = db.Tickets.Where(t => t.AssignedToId == id).ToList();

            return PartialView(model);
        }

        //POST: Admin/Users/AssignUserToTicket/5
        [Authorize(Roles="Project Manager, Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignUserToTicket(AssignUserViewModel model)
        {
            var original = db.Tickets.AsNoTracking().FirstOrDefault(t=>t.Id == model.SelectedTicketId);
            var ticket = db.Tickets.Find(model.SelectedTicketId);
            var userId = User.Identity.GetUserId();

            ticket.AssignedToId = model.User.Id;
            ticket.Status = db.Statuses.FirstOrDefault(s => s.Name == "Assigned");

            //notify developers
            //var developer = db.Users.Find(ticket.AssignedToId);
            //var es = new EmailService();
            //var msg = ticket.CreateAssignedToTicketMessage(developer);
            //es.SendAsync(msg);
        
            //var oldDeveloper = db.Users.Find(original.AssignedToId);
            //var msg2 = original.CreateAssignmentRemovedMessage(oldDeveloper);
            //es.SendAsync(msg2);

            ////add changelog
            //ticket.Id.CreateTicketChangeLog(userId, "Assigned To", ticket.AssignedTo.FullName, original.AssignedTo.FullName);

            ////add notifications
            //ticket.Id.CreateTicketNotification("Ticket Assigned", new List<string>{developer.Id}, msg.Body);
            //ticket.Id.CreateTicketNotification("Ticket Reassigned", new List<string> { oldDeveloper.Id }, msg2.Body);

            db.SaveChanges();
            ViewBag.SuccessMessage = "Ticket: " + ticket.Name + "has been reassigned, and a notification has been sent to the developers.";

            return RedirectToAction("Users");
        }

        //GET: Admin/Roles
        [Authorize(Roles = "Administrator")]
        public ActionResult Roles()
        {
            var developers = db.Roles.FirstOrDefault(r => r.Name == "Developer").Name.UsersInRole().AsEnumerable();
            var administrators = db.Roles.FirstOrDefault(r => r.Name == "Administrator").Name.UsersInRole().AsEnumerable();
            var projectManagers = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Name.UsersInRole().AsEnumerable();
            var submitters = db.Roles.FirstOrDefault(r => r.Name == "Submitter").Name.UsersInRole().AsEnumerable();

            var model = new RolesIndexViewModel()
            {
                Submitters = submitters,
                Developers = developers,
                ProjectManagers = projectManagers,
                Administrators = administrators
            };

            return View(model);
        }
    }
}