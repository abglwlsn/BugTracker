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
        public ActionResult Index()
        { 
            IEnumerable<Ticket> tickets = db.Tickets.Include(t => t.AssignedTo).Include(t => t.Project).Where(t=>t.Status.Name != "Resolved").OrderBy(t=>t.Priority).AsEnumerable();

            return View(tickets);
        }

        //GET: Tickets/UserTickets
        //try route prefix for Tickets/{user.FirstName}
        public ActionResult UserTickets()
        {
            var userId = User.Identity.GetUserId();
            IEnumerable<Project> projects;
            IEnumerable<Ticket> assignedTickets;
            IEnumerable<Ticket> submittedTickets;

            if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
                projects = userId.ListUserProjects().OrderByDescending(p=>p.Deadline);
            else
                projects = null;

            if (User.IsInRole("Developer"))
                assignedTickets = userId.ListUserTickets().OrderBy(t=>t.Priority);
            else
                assignedTickets = null;

            submittedTickets = db.Tickets.Where(t => t.SubmitterId == userId).OrderBy(t => t.Submitted);

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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        public ActionResult Create(int? id)
        {
            var project = db.Projects.Find(id);
            IEnumerable<ApplicationUser> developers;

            if (project != null)
                developers = project.Users.Where(u => u.Id.UserIsInRole("Developer")).AsEnumerable();
            else
                developers = db.Users.Where(u => u.Id.UserIsInRole("Developer")).AsEnumerable();

            var model = new CreateEditTicketViewModel()
            {
                Ticket = new Ticket(),
                Project = project,
                Projects = new SelectList(db.Projects.Where(p => p.IsResolved != true), "Id", "Name"),
                Developers = new SelectList(developers, "Id", "Name"),
                Priorities = new SelectList(db.Priorities.OrderByDescending(p => p.Name), "Id", "Name"),
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
        public ActionResult Create([Bind(Include = "Id,ProjectId,SubmitterId,AssignedToId,PriorityId,StatusId,TypeId,Name,Submitted,Description")] Ticket ticket)
        {
            var userId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                ticket.SubmitterId = userId;
                ticket.Submitted = DateTimeOffset.Now;

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
                return RedirectToAction("Index");
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            var developers = ticket.Project.Users.Where(u => u.Id.UserIsInRole("Developer"));
            var model = new CreateEditTicketViewModel()
            {
                Ticket = ticket,
                Project = ticket.Project,
                Developers = new SelectList(developers),
                Priorities = new SelectList(db.Priorities.OrderByDescending(p => p.Name)),
                Statuses = new SelectList(db.Statuses),
                Phases = new SelectList(db.TicketPhases),
                Actions = new SelectList(db.TicketActions)
            };

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ProjectId,SubmitterId,AssignedToId,PriorityId,StatusId,TypeId,Name,Submitted,LastModified,Closed,Description")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.LastModified = DateTimeOffset.Now;

                //if (ticket.Status.Name == "Resolved")
                //{
                //    ticket.Closed = DateTimeOffset.Now;

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
                //}

                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
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
