using BugTracker.HelperExtensions;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{

    public class UserInfoViewModel
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public UserInfoViewModel(string userId)
        {
            this.User = db.Users.Find(userId);
            this.Roles = userId.ListUserRoles();
        }

        public ApplicationUser User { get; set; }
        public IEnumerable<Ticket> AssignedTickets { get; set; }
        public IEnumerable<Project> AssignedProjects { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }

    public class AddRemoveRolesViewModel
    {
        public string UserId { get; set; }
        public MultiSelectList Roles { get; set; }
        public string[] SelectedRoles { get; set; }
    }

    public class AssignUserViewModel
    {
        public string UserId { get; set; }
        public IList<Project> Projects { get; set; }
        public IList<Ticket> Tickets { get; set; }
        public Ticket SelectedTicket { get; set; }
        public List<Ticket> CurrentTickets { get; set; }
    }

    public class AddUsersViewModel
    {
        public string RoleName { get; set; }
        public MultiSelectList UsersNotInRole { get; set; }
        public string[] SelectedUsersToAdd { get; set; }
    }

    public class RemoveUsersViewModel
    {
        public string RoleName { get; set; }
        public MultiSelectList UsersInRole { get; set; }
        public string[] SelectedUsersToRemove { get; set; }
    }
}