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
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            var tickets = db.Tickets.Include(t => t.AssignedTo).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.Submitter).Include(t => t.Phase).Include(t => t.Action);
            return View(tickets.OrderBy(p=>p.PriorityId).ToList());
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
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ProjectId,SubmitterId,AssignedToId,PriorityId,StatusId,TypeId,Name,Submitted,Description")] CreateEditTicketViewModel model)
        {
            var userId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                Ticket ticket = model.Ticket;
                ticket.SubmitterId = userId;
                ticket.Submitted = DateTimeOffset.Now;


                if (ticket.AssignedToId != null)
                {
                    ticket.Status = db.Statuses.Find(2);
                    var developer = db.Users.Find(ticket.AssignedToId);
                    var es = new EmailService();
                    var msg = ticket.CreateAssignedToTicketMessage(developer);
                    es.SendAsync(msg);
                }
                else
                    ticket.Status = db.Statuses.Find(1);

                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
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

                if (ticket.Status.Name == "Resolved")
                {
                    ticket.Closed = DateTimeOffset.Now;

                    //notify submitter, project manager, admins
                    var es = new EmailService();
                    var recipientList = new List<ApplicationUser>();
                    var admins = db.Roles.FirstOrDefault(r => r.Name == "Administrator").Name.UsersInRole();
                    recipientList.Add(ticket.Project.ProjectManager);
                    recipientList.Add(ticket.Submitter);
                    recipientList.Union(admins);

                    var msgList = ticket.CreateTicketResolvedMessage(recipientList);
                    foreach (var message in msgList)
                        es.Send(message);
                }

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
