using BugTracker.HelperExtensions;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [Authorize]
    public class ProjectsController : Controller
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private Dictionary<int, NotificationType> types = new Dictionary<int, NotificationType>()
        {
            { 1, new NotificationType {Id=9, Name="Ticket Submitted" } },
            { 2, new NotificationType {Id=1, Name="Ticket Assigned" } },
            { 3, new NotificationType {Id=4, Name="Ticket Modified" } },
            { 4, new NotificationType {Id=5, Name="Ticket Reassigned" } },
            { 5, new NotificationType {Id=2, Name="Ticket Resolved" } },
            { 6, new NotificationType {Id=3, Name="Reminder:Update Tickets" } },
            { 7, new NotificationType {Id=6, Name="Project Assigned" } },
            { 8, new NotificationType {Id=7, Name="Project Reassigned" } },
            { 9, new NotificationType {Id=8, Name="New Project Manager" } },
            {10, new NotificationType {Id=9, Name="Project Deadline Changed" } }
        };

        // GET: Projects/Index
        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            List<Project> projects;

            if (User.IsInRole("Administrator"))
                projects = db.Projects.Where(p => p.IsResolved != true).OrderByDescending(p => p.Deadline).ToList();
            else
                projects = userId.ListUserProjects().ToList();

            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(projects);
        }

        // GET: Projects/Details/5
        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        public ActionResult Details(int? id)
        {
            Project project = db.Projects.Find(id);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (project == null)
                return HttpNotFound();

            return View(project);
        }

        //GET: Projects/Create
        public ActionResult Create()
        {
            var managers = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Name.UsersInRole().AsEnumerable();
            var developers = db.Roles.FirstOrDefault(r => r.Name == "Developer").Name.UsersInRole().AsEnumerable();
            var submitters = db.Roles.FirstOrDefault(r => r.Name == "Submitter").Name.UsersInRole().AsEnumerable();

            var model = new CreateEditProjectViewModel()
            {
                Project = new Project(),
                ProjectManagers = new SelectList(managers, "Id", "FullName"),
                Developers = new MultiSelectList(developers, "FullName", "FullName"),
                Submitters = new MultiSelectList(submitters, "FullName", "FullName")
            };

            return View(model);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Create(Project project, List<string> SelectedDevelopers, List<string> SelectedSubmitters)
        {
            var userId = User.Identity.GetUserId();
            var manager = db.Users.Find(project.ProjectManagerId);
            var defaultManager = db.Users.Find(userId);

            if (ModelState.IsValid)
            {
                db.Projects.Add(project);

                if (manager == null && userId.UserIsInRole("Project Manager"))
                {
                    project.ProjectManagerId = userId;
                    project.Users.Add(defaultManager);
                }
                else if (manager != null)
                    project.Users.Add(manager);

                //notify project manager
                var es = new EmailService();
                var msg = project.CreateAssignedToProjectMessage(manager != null ? manager : defaultManager);
                es.Send(msg);

                //log notification
                var notif = project.Id.CreateProjectNotification(types[7], new List<ApplicationUser> { manager != null ? manager : defaultManager }, msg.Body);
                db.Notifications.Add(notif);

                //add users
                foreach (var user in db.Users)
                {
                    if (SelectedDevelopers.Contains(user.FullName))
                        project.Users.Add(user);
                    if (SelectedSubmitters.Contains(user.FullName) && !project.Users.Any(u => u.FullName == user.FullName))
                        project.Users.Add(user);
                }

                project.Created = DateTimeOffset.Now;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Project " + project.Name + " created.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Something went wrong. Please try again or submit a ticket.";
            return RedirectToAction("Index");
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Edit(int id)
        {
            Project project = db.Projects.Find(id);

            var projectUsers = project.Users.ToList();
            var managers = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Name.UsersInRole();
            var developers = db.Roles.FirstOrDefault(r => r.Name == "Developer").Name.UsersInRole();
            var submitters = db.Roles.FirstOrDefault(r => r.Name == "Submitter").Name.UsersInRole();
            var selectedManager = db.Users.Find(project.ProjectManagerId);
            var selectedDevelopers = new List<string>();
            var selectedSubmitters = new List<string>();

            foreach (var user in projectUsers)
            {
                if (user.Id.UserIsInRole("Developer"))
                    selectedDevelopers.Add(user.FullName);
                if (user.Id.UserIsInRole("Submitter"))
                    selectedSubmitters.Add(user.FullName);
            }

            var model = new CreateEditProjectViewModel()
            {
                Project = project,
                ProjectManagers = new SelectList(managers, "Id", "FullName", selectedManager),
                Developers = new MultiSelectList(developers, "FullName", "FullName", selectedDevelopers),
                Submitters = new MultiSelectList(submitters, "FullName", "FullName", selectedSubmitters)
            };

            return View(model);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Edit([Bind(Include = "Id,ProjectManagerId,Name,Deadline,Description,Version")]Project project, List<string> SelectedDevelopers, List<string> SelectedSubmitters)
        {
            var original = db.Projects.AsNoTracking().FirstOrDefault(p => p.Id == project.Id);
            var origManager = db.Users.Find(original.ProjectManagerId);
            var manager = db.Users.Find(project.ProjectManagerId);
            var userId = User.Identity.GetUserId();
            var es = new EmailService();

            var proj = db.Projects.Find(project.Id);
            proj.Name = project.Name;
            proj.ProjectManagerId = project.ProjectManagerId;
            proj.Version = project.Version;
            proj.Deadline = project.Deadline;
            proj.Description = project.Description;
            proj.LastModified = DateTimeOffset.Now;

            if (ModelState.IsValid)
            {
                //reassign current users
                proj.Users.Clear();
                foreach (var user in db.Users)
                {
                    if (SelectedDevelopers.Contains(user.FullName))
                        proj.Users.Add(user);
                    if (SelectedSubmitters.Contains(user.FullName) && !proj.Users.Any(u => u.FullName == user.FullName))
                        proj.Users.Add(user);
                }

                //add changelogs
                var newLogs = original.CreateProjectChangelogs(proj, userId);
                db.Logs.AddRange(newLogs);

                var developers = "Developer".UsersInRole();
                var projectDevs = new List<ApplicationUser>();
                foreach (var dev in developers)
                    if (proj.Users.Any(u => u.Id == dev.Id))
                        projectDevs.Add(dev);

                //check if deadline change
                if (original.Deadline != proj.Deadline)
                {
                    if (manager.Id != userId)
                        projectDevs.Add(manager);
                    var msgList = proj.CreateProjectDeadlineChangedMessage(projectDevs);
                    var msgListBody = msgList.First().Body;
                    foreach (var message in msgList)
                        es.Send(message);

                    var notif = proj.Id.CreateProjectNotification(types[10], new List<ApplicationUser>(projectDevs), msgListBody);
                    db.Notifications.Add(notif);
                }

                //check if new Project Manager
                if (original.ProjectManagerId != proj.ProjectManagerId)
                {
                    proj.Users.Remove(origManager);
                    proj.Users.Add(manager);

                    //create emails
                    var msgList = proj.CreateNewProjectManagerMessage(projectDevs);
                    var msg = proj.CreateAssignedToProjectMessage(manager);
                    var msg2 = proj.CreateRemovedFromProjectMessage(origManager);
                    var msgListBody = msgList.First().Body;

                    es.Send(msg);
                    es.Send(msg2);
                    foreach (var message in msgList)
                        es.Send(message);

                    //add notifications
                    ICollection<Notification> notifs = new List<Notification>
                    {
                    proj.Id.CreateProjectNotification(types[7], new List<ApplicationUser> { manager }, msg.Body),
                    proj.Id.CreateProjectNotification(types[8], new List<ApplicationUser> { origManager }, msg2.Body),
                    proj.Id.CreateProjectNotification(types[9], new List<ApplicationUser>(projectDevs), msgListBody)
                    };
                    db.Notifications.AddRange(notifs);
                }
                else
                    proj.Users.Add(manager);

                db.SaveChanges();

                return RedirectToAction("Details", "Projects", new { id = proj.Id });
            }

            ViewBag.ErrorMessage = "Something went wrong. Please try again or submit a ticket.";
            return RedirectToAction("Index");
        }
    }
}
