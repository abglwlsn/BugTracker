using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using BugTracker.HelperExtensions;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
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
            { 9, new NotificationType {Id=8, Name="New Project Manager" } }
        };

        // GET: Tickets
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            IEnumerable<Ticket> tickets = db.Tickets.Include(t => t.AssignedTo).Include(t => t.Project).OrderBy(t => t.Priority.Id).ToList();

            return View(tickets);
        }

        //GET: Tickets/UserTickets
        //try route prefix for Tickets/{user.FirstName}
        [Authorize(Roles = "Project Manager, Developer, Submitter")]
        public ActionResult UserTickets(string returnUrl)
        {
            var userId = User.Identity.GetUserId();
            IEnumerable<Project> projects;
            IEnumerable<Ticket> assignedTickets;
            IEnumerable<Ticket> submittedTickets;

            if (User.IsInRole("Developer"))
                assignedTickets = userId.ListUserTickets().OrderBy(t => t.Priority.Id);
            else
                assignedTickets = null;

            projects = userId.ListUserProjects().OrderByDescending(p => p.Deadline);
            submittedTickets = db.Tickets.Where(t => t.SubmitterId == userId).Where(t => t.Status.Name != "Resolved").OrderBy(t => t.Submitted).ToList();

            var model = new UserTicketsViewModel()
            {
                AssignedProjects = projects,
                AssignedTickets = assignedTickets,
                SubmittedTickets = submittedTickets
            };

            return View(model);
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            Ticket ticket = db.Tickets.Include(t => t.Comments).Include(t => t.Logs).FirstOrDefault(t => t.Id == id);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ticket == null)
                return HttpNotFound();

            ViewBag.FileErrorMessage = TempData["FileErrorMessage"];

            return View(ticket);
        }

        //GET: Tickets/ChooseProject
        public ActionResult ChooseProject(string returnUrl)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var projects = new List<Project>();

            if (User.IsInRole("Administrator"))
                projects = db.Projects.Where(p => p.IsResolved != true).ToList();
            else
                foreach (var project in db.Projects.ToList())
                    if (project.Users.Contains(user))
                        projects.Add(project);

            var model = new ChooseProjectViewModel()
            {
                Projects = new SelectList(projects, "Id", "Name"),
                returnUrl = returnUrl
            };

            return View(model);
        }

        //POST: Tickets/ChooseProject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChooseProject(ChooseProjectViewModel model)
        {
            var project = db.Projects.Find(model.SelectedProjectId);
            return RedirectToAction("Create", new { id = project.Id });
        }

        // GET: Tickets/Create
        public ActionResult Create(int? id)
        {
            var project = db.Projects.Find(id);
            var userId = User.Identity.GetUserId();
            string projectName;
            var projectId = project.Id;
            var developers = "Developer".UsersInRole().ToList();
            var projectDevelopers = new List<ApplicationUser>();

            //select project developers
            if (project != null)
            {
                foreach (var dev in developers)
                    if (project.Users.Any(u => u.Id == dev.Id))
                        projectDevelopers.Add(dev);
                projectName = project.Name;
            }
            else
                projectName = "";

            var model = new CreateEditTicketViewModel()
            {
                Ticket = new Ticket(),
                ProjectName = projectName,
                ProjectId = project.Id,
                Developers = new SelectList(projectDevelopers, "Id", "FullName"),
                Priorities = new SelectList(db.Priorities.OrderBy(p => p.Id), "Id", "Name"),
                Statuses = new SelectList(db.Statuses, "Id", "Name"),
                Phases = new SelectList(db.TicketPhases, "Id", "Name"),
                Actions = new SelectList(db.TicketActions, "Id", "Name")
            };
            return View(model);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ProjectId,SubmitterId,AssignedToId,PriorityId,PhaseId,StatusId,ActionId,Name,Submitted,Description")] Ticket ticket, int ProjectId)
        {
            var userId = User.Identity.GetUserId();
            var project = db.Projects.Find(ProjectId);

            if (ModelState.IsValid)
            {
                ticket.SubmitterId = userId;
                ticket.Submitted = DateTimeOffset.Now;
                ticket.ProjectId = ProjectId;
                var es = new EmailService();

                if (ticket.AssignedToId != null)
                    ticket.Status = db.Statuses.FirstOrDefault(s => s.Name == "Assigned");
                else
                    ticket.Status = db.Statuses.FirstOrDefault(s => s.Name == "Unassigned");

                if (!User.IsInRole("Administrator"))
                {
                    var admins = "Administrator".UsersInRole().ToList();
                    var msgList = ticket.CreateTicketSubmittedMessage(admins);
                    var msg = msgList.First().Body;
                    foreach (var message in msgList)
                        es.Send(message);

                    var notif = ticket.Id.CreateTicketNotification(types[2], admins, msg);
                }

                db.Tickets.Add(ticket);
                db.SaveChanges();

                if(ticket.AssignedToId!= null)
                {
                    var developer = db.Users.Find(ticket.AssignedToId);
                    var msg = ticket.CreateAssignedToTicketMessage(project, developer);
                    es.SendAsync(msg);

                    //log notification
                    var notif = ticket.Id.CreateTicketNotification(types[2], new List<ApplicationUser> { developer }, msg.Body);
                    db.Notifications.Add(notif);
                }

                    return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        public ActionResult Edit(int? id)
        {
            Ticket ticket = db.Tickets.Find(id);
            var userId = User.Identity.GetUserId();
            var project = ticket.Project;
            string projectName;
            //var projects = new List<Project>();
            var developers = "Developer".UsersInRole();
            var projectDevelopers = new List<ApplicationUser>();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ticket == null)
                return HttpNotFound();

            //if (User.IsInRole("Administrator"))
            //    projects = db.Projects.Where(p => p.IsResolved != true).ToList();
            //else
            //    projects = userId.ListUserProjects().Where(p => p.IsResolved != true).ToList();

            //select project developers
            if (project != null)
            {
                foreach (var dev in developers.ToList())
                    if (project.Users.Any(u => u.Id == dev.Id))
                        projectDevelopers.Add(dev);
                projectName = project.Name;
            }
            else
                projectName = "";

            var model = new CreateEditTicketViewModel()
            {
                Ticket = ticket,
                ProjectName = projectName,
                //Projects = new SelectList(projects, "Id", "Name"),
                Developers = new SelectList(projectDevelopers, "Id", "FullName", ticket.AssignedTo),
                Priorities = new SelectList(db.Priorities.OrderBy(p => p.Id), "Id", "Name", ticket.Priority),
                Statuses = new SelectList(db.Statuses, "Id", "Name", ticket.Status),
                Phases = new SelectList(db.TicketPhases, "Id", "Name", ticket.Phase),
                Actions = new SelectList(db.TicketActions, "Id", "Name", ticket.Action)
            };

            return View(model);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        public ActionResult Edit([Bind(Include = "Id,ProjectId,AssignedToId,PriorityId,PhaseId,StatusId,ActionId,Name,LastModified,Submitted, SubmitterId,Closed,Description")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);
                var project = db.Projects.Find(ticket.ProjectId);
                var manager = db.Users.Find(project.ProjectManagerId);
                var userId = User.Identity.GetUserId();
                var es = new EmailService();

                //check statuses
                var unassigned = db.Statuses.FirstOrDefault(s => s.Name == "Unassigned");
                if (ticket.StatusId == unassigned.Id && ticket.AssignedToId != null)
                    ticket.Status = db.Statuses.FirstOrDefault(t => t.Name == "Assigned");

                var resolvedStatus = db.Statuses.FirstOrDefault(s => s.Name == "Resolved");
                if (ticket.StatusId == resolvedStatus.Id)
                {
                    ticket.Closed = DateTimeOffset.Now;

                    //notify submitter, project manager, admins
                    var recipientList = new List<ApplicationUser>();
                    var admins = "Administrator".UsersInRole();
                    var submitter = db.Users.Find(ticket.SubmitterId);

                    recipientList.Add(manager);
                    if (submitter != manager)
                        recipientList.Add(submitter);
                    recipientList.Union(admins);

                    var msgList = ticket.CreateTicketResolvedMessage(recipientList);
                    var msg = msgList.First().Body;
                    foreach (var message in msgList)
                        es.Send(message);

                    //log notifications
                    var notification = ticket.Id.CreateTicketNotification(types[5], recipientList, msg);
                    db.Notifications.Add(notification);
                }

                if (oldTicket.AssignedToId != ticket.AssignedToId)
                {
                    var oldDev = db.Users.Find(oldTicket.AssignedToId);
                    var newDev = db.Users.Find(ticket.AssignedToId);

                    //emails
                    var msg = ticket.CreateTicketReassignedMessage(project, oldDev);
                    var msg2 = ticket.CreateAssignedToTicketMessage(project, newDev);

                    es.Send(msg);
                    es.Send(msg2);

                    //log notifications
                    ICollection<Notification> notifications = new List<Notification>
                    {
                        ticket.Id.CreateTicketNotification(types[4], new List<ApplicationUser> {oldDev }, msg.Body),
                        ticket.Id.CreateTicketNotification(types[2], new List<ApplicationUser> { newDev }, msg2.Body)
                    };

                    //db.Notifications.Add(notification);
                    //db.Notifications.Add(notification2);
                    db.Notifications.AddRange(notifications);
                }

                if (oldTicket != ticket && userId != manager.Id)
                {
                    var msg = ticket.CreateTicketModifiedMessage(manager);
                    es.Send(msg);

                    ticket.Id.CreateTicketNotification(types[3], new List<ApplicationUser> { manager }, msg.Body);
                }

                //create changelogs
                var newLogs = oldTicket.CreateTicketChangeLogs(ticket, userId);
                db.Logs.AddRange(newLogs);
                db.SaveChanges();

                ticket.LastModified = DateTimeOffset.Now;
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            return View(ticket);
        }

        //GET: Tickets/RequestReassignment/5
        [Authorize(Roles = "Developer")]
        public ActionResult RequestReassignment(int id)
        {
            var ticket = db.Tickets.Find(id);

            return View(ticket);
        }

        //POST: Tickets/RequestReassignment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Developer")]
        public ActionResult RequestReassignment(int id, string request)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var ticket = db.Tickets.Find(id);
            var es = new EmailService();
            var msg = ticket.CreateReassignmentRequestedMessage(user, request);
            es.Send(msg);

            return RedirectToAction("Details", "Tickets", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
