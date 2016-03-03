using BugTracker.HelperExtensions;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects/Index
        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var projects = new List<Project>();
            if (User.IsInRole("Administrator"))
                projects = db.Projects.Include("Tickets").Include("ProjectManager").Include("Users").Where(p => p.IsSoftDeleted != true).OrderByDescending(p => p.Deadline).ToList();
            else
                projects = (List<Project>)userId.ListUserProjects();
           
            return View(projects);
        }

        // GET: Projects/Details/5
        [Authorize(Roles = "Administrator, Project Manager, Developer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        //GET: Projects/Create
        public PartialViewResult _Create()
        {
            var managers = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Name.UsersInRole();
            var model = new CreateEditProjectViewModel()
            {
                Project = new Project(),
                ProjectManagers = new SelectList(managers, "Id", "FullName")
            };

            return PartialView(model);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Create([Bind(Include="Id,ProjectManagerId,Name,Deadline,Description,Version")]Project project)
        {
            var userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                if (project.ProjectManagerId == null && userId.UserIsInRole("Project Manager"))
                    project.ProjectManagerId = userId;

                //VERIFY THAT THE ABOVE CODE APPLIES BEFORE THIS RUNS
                //notify project manager
                if (project.ProjectManagerId != null)
                {
                    var es = new EmailService();
                    var msg = project.CreateAssignedToProjectMessage(project.ProjectManager);
                    es.Send(msg);
                }

                project.Created = DateTimeOffset.Now;
                db.SaveChanges();

                ViewBag.SuccessMessage = "Project " + project.Name + " created.";
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Something went wrong. Please try again or submit a ticket.";
            return RedirectToAction("Index");
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Administrator, Project Manager")]
        public PartialViewResult _Edit(int id)
        {
            Project project = db.Projects.Find(id);

            return PartialView(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Project Manager")]
        public ActionResult Edit([Bind(Include="Id,ProjectManagerId,Name,Deadline,Description,Version")]int id)
        {
            var project = db.Projects.Find(id);
            var original = db.Projects.AsNoTracking().FirstOrDefault(p=>p.Id == id);
            var user = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                //notify involved users
                if (original.ProjectManagerId != project.ProjectManagerId)
                {
                    var developers = db.Roles.FirstOrDefault(r => r.Name == "Developer").Name.UsersInRole();
                    var es = new EmailService();
                    var msg = project.CreateAssignedToProjectMessage(project.ProjectManager);
                    es.Send(msg);

                    var msg2 = original.CreateRemovedFromProjectMessage(original.ProjectManager);
                    es.Send(msg2);

                    var msgList = project.CreateNewProjectManagerMessage(developers);
                    foreach (var message in msgList)
                        es.Send(message);

                    //add notifications
                    project.Id.CreateProjectNotification("Project Assigned", new List<string> { project.ProjectManagerId }, msg.Body);
                    project.Id.CreateTicketNotification("Project Reassigned", new List<string> { original.ProjectManagerId }, msg2.Body);
                    //developers notification
                }

                //add changelog
                //project.Id.CreateProjectChangeLog(userId, );
                //How to determin which fields where changed? foreach through all, need new and old value

                project.LastModified = DateTimeOffset.Now;

                db.SaveChanges();

                return RedirectToAction("Index");

            }

            ViewBag.ErrorMessage = "Something went wrong. Please try again or submit a ticket.";
            return RedirectToAction("Index");
        }

        // GET: Projects/Delete/5
        [Authorize(Roles ="Administrator")]
        public PartialViewResult _Delete(int id)
        {
            var project = db.Projects.Find(id);

            return PartialView(project);
        }

        // POST: Projects/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [Authorize(Roles = "Administrator")]
        public ActionResult SoftDelete(int id)
        {
            var project = db.Projects.Find(id);
            project.IsSoftDeleted = true;
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
