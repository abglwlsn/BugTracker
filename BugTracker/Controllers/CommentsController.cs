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

namespace BugTracker.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Comments
        //public ActionResult Index()
        //{
        //    var comments = db.Comments.Include(c => c.Submitter).Include(c => c.Ticket);
        //    return View(comments.ToList());
        //}

        // GET: Comments/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Comment comment = db.Comments.Find(id);
        //    if (comment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(comment);
        //}

        // GET: Comments/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,SubmitterId,Title,Body,Submitted")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.Submitted = DateTimeOffset.Now;
                comment.SubmitterId = User.Identity.GetUserId();

                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
            }
            return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
        }

        // GET: Comments/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Comment comment = db.Comments.Find(id);
        //    if (comment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.SubmitterId = new SelectList(db.ApplicationUsers, "Id", "FirstName", comment.SubmitterId);
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "SubmitterId", comment.TicketId);
        //    return View(comment);
        //}

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,TicketId,SubmitterId,Title,Body,Submitted")] Comment comment)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(comment).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.SubmitterId = new SelectList(db.ApplicationUsers, "Id", "FirstName", comment.SubmitterId);
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "SubmitterId", comment.TicketId);
        //    return View(comment);
        //}

        // GET: Comments/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Comment comment = db.Comments.Find(id);
        //    if (comment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(comment);
        //}

        // POST: Comments/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
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
