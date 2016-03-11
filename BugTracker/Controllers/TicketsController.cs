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
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        [Authorize(Roles ="Administrator")]
        public ActionResult Index()
        { 
            IEnumerable<Ticket> tickets = db.Tickets.Include(t => t.AssignedTo).Include(t => t.Project).OrderBy(t=>t.Priority.Id).ToList();

            return View(tickets);
        }

        //GET: Tickets/UserTickets
        //try route prefix for Tickets/{user.FirstName}
        [Authorize(Roles = "Project Manager, Developer")]
        public ActionResult UserTickets(string returnUrl)
        {
            var userId = User.Identity.GetUserId();
            IEnumerable<Project> projects;
            IEnumerable<Ticket> assignedTickets;
            IEnumerable<Ticket> submittedTickets;

            if (User.IsInRole("Developer"))
                assignedTickets = userId.ListUserTickets().OrderBy(t=>t.Priority.Id);
            else
                assignedTickets = null;

            projects = userId.ListUserProjects().OrderByDescending(p => p.Deadline);
            submittedTickets = db.Tickets.Where(t => t.SubmitterId == userId).Where(t=>t.Status.Name != "Resolved").OrderBy(t => t.Submitted).ToList();

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
            Ticket ticket = db.Tickets.FirstOrDefault(t=>t.Id == id);
            //.Include(t=>t.Logs)
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            if (ticket == null)
                return HttpNotFound();
            
            return View(ticket);
        }

        //GET: Tickets/ChooseProject
        public ActionResult ChooseProject()
        {
            var projects = db.Projects.Where(p => p.IsResolved != true).ToList();
            var model = new ChooseProjectViewModel()
            {
                Projects = new SelectList(projects, "Id", "Name")
            };

            return View(model);
        }

        //POST: Tickets/ChooseProject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="Administrator, Project Manager, Developer")]
        public ActionResult ChooseProject(int id)
        {
            return RedirectToAction("Create", new { id = id });
        }

        // GET: Tickets/Create
        public ActionResult Create(int? id)
        {
            var project = db.Projects.Find(id);
            var userId = User.Identity.GetUserId();
            string projectName;
            var projectId = project.Id;
            var projects = new List<Project>();
            var devRole = "Developer";
            var developers = devRole.UsersInRole().ToList();
            var projectDevelopers = new List<ApplicationUser>();

            if (User.IsInRole("Administrator"))
                projects = db.Projects.Where(p => p.IsResolved != true).ToList();
            else
                projects = userId.ListUserProjects().Where(p => p.IsResolved != true).ToList();

            if (project != null)
            {
                foreach (var dev in developers)
                    if (project.Users.Any(u=>u.Id == dev.Id))
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
                Projects = new SelectList(projects, "Id", "Name"),
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

            if (ModelState.IsValid)
            {
                ticket.SubmitterId = userId;
                ticket.Submitted = DateTimeOffset.Now;
                ticket.ProjectId = ProjectId;

                if (ticket.AssignedToId != null)
                {
                    ticket.Status = db.Statuses.FirstOrDefault(s=>s.Name == "Assigned");
                    //var developer = db.Users.Find(ticket.AssignedToId);
                    //var es = new EmailService();
                    //var msg = ticket.CreateAssignedToTicketMessage(developer);
                    //es.SendAsync(msg);
                }
                else
                    ticket.Status = db.Statuses.FirstOrDefault(s=>s.Name == "Unassigned");

                if (!User.IsInRole("Administrator"))
                {
                    // send notification of submitted ticket to admins
                }

                db.Tickets.Add(ticket);
                db.SaveChanges();

                if (User.IsInRole("Administrator"))
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Index", "Projects");
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        public ActionResult Edit(int? id)
        {
            Ticket ticket = db.Tickets.Find(id);
            var userId = User.Identity.GetUserId();
            var project = db.Projects.Find(id);
            string projectName;
            var projects = new List<Project>();
            var devRole = "Developer";
            var developers = devRole.UsersInRole();
            var projectDevelopers = new List<ApplicationUser>();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            if (ticket == null)
                return HttpNotFound();

            if (User.IsInRole("Administrator"))
                projects = db.Projects.Where(p => p.IsResolved != true).ToList();
            else
                projects = userId.ListUserProjects().Where(p => p.IsResolved != true).ToList();

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
                Projects = new SelectList(projects, "Id", "Name"),
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
                var resolvedStatus = db.Statuses.FirstOrDefault(s => s.Name == "Resolved");
                if (ticket.StatusId == resolvedStatus.Id)
                {
                    ticket.Closed = DateTimeOffset.Now;

                //    //notify submitter, project manager, admins
                //    var es = new EmailService();
                //    var recipientList = new List<ApplicationUser>();
                //    var admins = db.Roles.FirstOrDefault(r => r.Name == "Administrator").Name.UsersInRole();
                //    recipientList.Add(ticket.Project.ProjectManager);
                //    recipientList.Add(ticket.Submitter);
                //    recipientList.Union(admins);

                //    var msgList = ticket.CreateTicketResolvedMessage(recipientList);
                //    foreach (var message in msgList)
                //        es.Send(message);
                }

                ticket.LastModified = DateTimeOffset.Now;
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                if (User.IsInRole("Administrator"))
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("UserTickets");
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ticket ticket = db.Tickets.Find(id);
        //    if (ticket == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ticket);
        //}

        // POST: Tickets/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Ticket ticket = db.Tickets.Find(id);
        //    db.Tickets.Remove(ticket);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
