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
    public class ProjectsController : Controller
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

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
        public ActionResult Create([Bind(Include="Id,ProjectManagerId,Name,Deadline,Description,Version")]Project project, List<string> SelectedDevelopers, List<string> SelectedSubmitters)
        {
            var userId = User.Identity.GetUserId();
            var manager = project.ProjectManagerId.GetProjectManager();
            if (ModelState.IsValid)
            {
                if (manager == null && userId.UserIsInRole("Project Manager"))
                        project.ProjectManagerId = userId;
                else if (manager != null)
                    project.Users.Add(manager);

                //VERIFY THAT THE ABOVE CODE APPLIES BEFORE THIS RUNS
                //notify project manager
                //if (manager != null)
                //{
                //    var es = new EmailService();
                //    var msg = project.CreateAssignedToProjectMessage(project.ProjectManager);
                //    es.Send(msg);
                //}
                foreach (var user in db.Users)
                {
                    if (SelectedDevelopers.Contains(user.FullName))
                        project.Users.Add(user);
                    if (SelectedSubmitters.Contains(user.FullName) && !project.Users.Any(u=>u.FullName == user.FullName))
                        project.Users.Add(user);
                }

                project.Users.Add(manager);
                project.Created = DateTimeOffset.Now;
                db.Projects.Add(project);
                db.SaveChanges();

                ViewBag.SuccessMessage = "Project " + project.Name + " created.";
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Something went wrong. Please try again or submit a ticket.";
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
            var selectedManager = project.ProjectManagerId.GetProjectManager();
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
        public ActionResult Edit([Bind(Include="Id,ProjectManagerId,Name,Deadline,Description,Version")]Project project, List<string> SelectedDevelopers, List<string> SelectedSubmitters)
        {
            var original = db.Projects.AsNoTracking().FirstOrDefault(p=>p.Id == project.Id);
            var origManager = original.ProjectManagerId.GetProjectManager();
            var manager = project.ProjectManagerId.GetProjectManager();

            var proj = db.Projects.Find(project.Id);
            proj.Name = project.Name;
            proj.ProjectManagerId = project.ProjectManagerId;
            proj.Version = project.Version;
            proj.Deadline = project.Deadline;
            proj.Description = project.Description;

            if (ModelState.IsValid)
            {
                proj.Users.Clear();
                foreach (var user in db.Users)
                {
                    if (SelectedDevelopers.Contains(user.FullName))
                        proj.Users.Add(user);
                    if (SelectedSubmitters.Contains(user.FullName) && !proj.Users.Any(u => u.FullName == user.FullName))
                        proj.Users.Add(user);
                }

                if (proj.ProjectManagerId != original.ProjectManagerId)
                {
                    proj.Users.Remove(origManager);
                    proj.Users.Add(manager);
                }
                else
                    proj.Users.Add(manager);

                proj.LastModified = DateTimeOffset.Now;
                //db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();

                //notify involved users
                //if (original.ProjectManagerId != project.ProjectManagerId)
                //{
                //    var developers = db.Roles.FirstOrDefault(r => r.Name == "Developer").Name.UsersInRole();
                //    var es = new EmailService();
                //    var msg = project.CreateAssignedToProjectMessage(project.ProjectManager);
                //    es.Send(msg);

                //    var msg2 = original.CreateRemovedFromProjectMessage(original.ProjectManager);
                //    es.Send(msg2);

                //    var msgList = project.CreateNewProjectManagerMessage(developers);
                //    foreach (var message in msgList)
                //        es.Send(message);

                //    //add notifications
                //    project.Id.CreateProjectNotification("Project Assigned", new List<string> { project.ProjectManagerId }, msg.Body);
                //    project.Id.CreateTicketNotification("Project Reassigned", new List<string> { original.ProjectManagerId }, msg2.Body);
                //    //developers notification
                //}

                //add changelog
                //project.Id.CreateProjectChangeLog(userId, );
                //How to determin which fields where changed? foreach through all, need new and old value

                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Something went wrong. Please try again or submit a ticket.";
            return RedirectToAction("Index");
        }

        // GET: Projects/Delete/5
        //[Authorize(Roles ="Administrator")]
        //public PartialViewResult _Delete(int id)
        //{
        //    var project = db.Projects.Find(id);

        //    return PartialView(project);
        //}

        // POST: Projects/Delete/5
        //[HttpPost]
        //[ActionName("Delete")]
        //[Authorize(Roles = "Administrator")]
        //public ActionResult SoftDelete(int id)
        //{
        //    var project = db.Projects.Find(id);
        //    project.IsResolved = true;
        //    db.SaveChanges();

        //    return RedirectToAction("Index");
        //}
    }
}
