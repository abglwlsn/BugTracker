using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using System.IO;
using Microsoft.AspNet.Identity;
using BugTracker.HelperExtensions;

namespace BugTracker.Controllers
{
    public class AttachmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Attachments
        //public ActionResult Index()
        //{
        //    var attachments = db.Attachments.Include(a => a.Ticket);
        //    return View(attachments.ToList());
        //}

        // GET: Attachments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attachment attachment = db.Attachments.Find(id);
            if (attachment == null)
            {
                return HttpNotFound();
            }
            return View(attachment);
        }

        // GET: Attachments/Create
        public ActionResult Create(int id)
        {
            var ticket = db.Tickets.Find(id);
            var attachment = new Attachment();
            attachment.Ticket = ticket;
            attachment.TicketId = ticket.Id;

            return View(attachment);
        }

        // POST: Attachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,SubmitterId,Title,Description,FilePath,Submitted")] Attachment attachment, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                attachment.Submitted = DateTimeOffset.Now;
                attachment.SubmitterId = User.Identity.GetUserId();

                if (attachment.Title == null)
                {
                    if (attachment.Description != null)
                        attachment.Title = new string(attachment.Description.Take(15).ToArray()) + "...";
                    else
                        attachment.Title = attachment.Submitted.FormatDateTimeOffset();
                }

                if (FileUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Images"), fileName));
                    attachment.FilePath = "~/Images/" + fileName;
                }

                db.Attachments.Add(attachment);
                db.SaveChanges();

                if (User.IsInRole("Administrator"))
                    return RedirectToAction("Index", "Tickets");
                else
                    return RedirectToAction("Details", "Tickets", new { id = attachment.TicketId });
            }
            TempData["FileErrorMessage"] = "File not uploaded. Please try again.";
                return RedirectToAction("Details", "Tickets", new { id = attachment.TicketId });
        }

        // GET: Attachments/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Attachment attachment = db.Attachments.Find(id);
        //    if (attachment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "SubmitterId", attachment.TicketId);
        //    return View(attachment);
        //}

        // POST: Attachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,TicketId,SubmitterId,Title,Description,FilePath,Submitted")] Attachment attachment)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(attachment).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "SubmitterId", attachment.TicketId);
        //    return View(attachment);
        //}

        // GET: Attachments/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Attachment attachment = db.Attachments.Find(id);
        //    if (attachment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(attachment);
        //}

        // POST: Attachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Attachment attachment = db.Attachments.Find(id);
            db.Attachments.Remove(attachment);
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
