using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class ProjectHelpers
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static bool IsUserInProject(this string userId, int projectId)
        {
            var project = db.Projects.Find(projectId);
            var user = project.Users.FirstOrDefault(u=>u.Id == userId);
            if (user != null)
                return true;
            else
                return false;
        }

        public static IEnumerable<Project> ListUserProjects(this string userId)
        {
            var devProjects = db.Projects.Include("Tickets").Include("ProjectManager").Include("Users").OrderByDescending(p => p.Deadline).Where(p => p.Tickets.Any(t => t.AssignedToId == userId) && p.IsResolved != true);
            var progManProjects = db.Projects.Include("Tickets").Include("ProjectManager").Include("Users").OrderByDescending(p => p.Deadline).Where(p => p.ProjectManagerId == userId && p.IsResolved != true);
           var allProjects = devProjects.Union(progManProjects).ToList();

            return allProjects;
        }

        public static IEnumerable<ApplicationUser> ListUsersNotOnProject(this int projectId)
        {
            var project = db.Projects.Find(projectId);
            var users = db.Users.ToList();
            var projectUserList = (IList<ApplicationUser>)project.Users;

            foreach (var user in projectUserList)
                users.Remove(user);

            return users;
        }

        //public static void AssignProjectManager(this int projectId, string userId)
        //{
        //    var project = db.Projects.Find(projectId);
        //    project.ProjectManagerId = userId;
        //    db.SaveChanges();
        //}

        public static void ReassignProjectManager(this int projectId, string oldPMId, string newPMId)
        {
            var project = db.Projects.Find(projectId);
            project.ProjectManagerId = newPMId;
            project.Users.Remove(db.Users.Find(oldPMId));
            project.Users.Add(db.Users.Find(newPMId));
            db.SaveChanges();
        }

        //public static void AddDeveloperToProject(this int ticketId, string userId)
        //{
        //    var project = db.Tickets.Find(ticketId).Project;
        //    if (!project.Users.Any(u=>u.Id == userId))
        //        project.Users.Add(db.Users.Find(userId));
        //    db.SaveChanges();
        //}

        //public static void RemoveDeveloperFromProject(this int ticketId, string userId)
        //{
        //    var project = db.Tickets.Find(ticketId).Project;
        //    if (project.Users.Any(u => u.Id == userId))
        //        project.Users.Remove(db.Users.Find(userId));
        //    db.SaveChanges();
        //}

        public static void CreateProjectChangeLog(this int projectId, string userId, string property, string newValue, string oldValue)
        {
            Log newLog = new Log()
            {
                ProjectId = projectId,
                ModifiedById = userId,
                Modified = DateTimeOffset.Now,
                Property = property,
                NewValue = newValue,
                OldValue = oldValue
            };
            db.Logs.Add(newLog);
            db.SaveChanges();
        }
    }
}