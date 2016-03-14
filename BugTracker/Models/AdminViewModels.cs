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
        public ApplicationUser User { get; set; }
        public MultiSelectList AllRoles { get; set; }
        public List<string> SelectedRoles { get; set; }
        public IEnumerable<Ticket> AssignedTickets { get; set; }
        public IEnumerable<Project> AssignedProjects { get; set; }
    }

    //public class AddRemoveRolesViewModel
    //{
    //    public ApplicationUser User { get; set; }
    //    public MultiSelectList Roles { get; set; }
    //    public string[] SelectedRoles { get; set; }
        //public List<string> SelectedRoles { get; set; }
        //public IEnumerable<SelectListItem> SelectedRoles { get; set; }
    //}

    public class AssignUserViewModel
    {
        public ApplicationUser User { get; set; }
        public SelectList Projects { get; set; }
        public int? SelectedProjectId { get; set; }
        public SelectList Tickets { get; set; }
        public int SelectedTicketId { get; set; }
        public IEnumerable<Ticket> CurrentTickets { get; set; }
    }

    public class AddRemoveUsersViewModel
    {
        public string RoleName { get; set; }
        public MultiSelectList Users { get; set; }
        public IEnumerable<string> SelectedUsers { get; set; }
        //public MultiSelectList UsersInRole { get; set; }
        //public string[] SelectedUsersToRemove { get; set; }
    }

    public class RolesIndexViewModel
    {
        public IEnumerable<ApplicationUser> Submitters { get; set; }
        public IEnumerable<ApplicationUser> Developers { get; set; }
        public IEnumerable<ApplicationUser> ProjectManagers { get; set; }
        public IEnumerable<ApplicationUser> Administrators { get; set; }

    }
}