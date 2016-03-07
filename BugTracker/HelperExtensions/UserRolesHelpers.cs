using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class UserRolesHelpers
    {
        private static UserManager<ApplicationUser> manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        private static ApplicationDbContext db = new ApplicationDbContext();

        public static bool UserIsInRole(this string userId, string roleName)
        {
            return manager.IsInRole(userId, roleName);
        }

        public static IList<string> ListUserRoles(this string userId)
        {
            return manager.GetRoles(userId).ToList();
        }

        public static IList<ApplicationUser> UsersInRole(this string roleName)
        {
            var role = db.Roles.FirstOrDefault(r => r.Name == roleName);
            var userList = new List<ApplicationUser>();
           
            foreach (var user in db.Users)
                if (UserIsInRole(user.Id, roleName))
                    userList.Add(user);

            return userList;
        }

        public static IList<ApplicationUser> UsersNotInRole(this string roleName)
        {
            //CONVERT TO LINQ
            var role = db.Roles.FirstOrDefault(r => r.Name == roleName);
            var users = manager.Users.ToList();
            var userList = (IList<ApplicationUser>)role.Users;

            foreach (var user in userList)
                users.Remove(user);
            
            return users;
        }

        public static bool AddUserToRole(this string userId, string roleName)
        {
            var result = manager.AddToRole(userId, roleName);
            return result.Succeeded;
        }

        public static bool RemoveUserFromRole(this string userId, string roleName)
        {
            var result = manager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }
    }
}