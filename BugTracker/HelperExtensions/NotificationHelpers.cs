using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class NotificationHelpers
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static IEnumerable<string> ConvertUsersToNames(this ICollection<ApplicationUser> users)
        {
            var names = new List<string>();
            foreach (var user in users)
                names.Add(user.FullName);

            return names;
        }

        public static Notification CreateTicketNotification(this int ticketId, NotificationType type, List<ApplicationUser> recipients, string msgBody)
        {
            Notification notification = new Notification()
            {
                TicketId = ticketId,
                TypeId = type.Id,
                Recipients = recipients,
                SendDate = DateTimeOffset.Now,
                Message = msgBody
            };

            return notification;
        }

        //public static Notification CreateTicketNotification(this int ticketId, string type, List<ApplicationUser> recipients, string msgBody)
        //{
        //    var typeId = db.NotificationTypes.FirstOrDefault(n => n.Name == type).Id;
        //    var users = new List<string>();
        //    foreach (var user in recipients)
        //        users.Add(user.FullName);

        //    Notification notification = new Notification()
        //    {
        //        TicketId = ticketId,
        //        TypeId = typeId,
        //        Recipients = users,
        //        SendDate = DateTimeOffset.Now,
        //        Message = msgBody
        //    };

        //    return notification;          
        //}

        //public static Notification CreateProjectNotification(this int projectId, string type, IEnumerable<ApplicationUser> recipients, string msgBody)
        //{
        //    var typeId = db.NotificationTypes.FirstOrDefault(n => n.Name == type).Id;
        //    var users = new List<string>();
        //    foreach (var user in recipients)
        //        users.Add(user.FullName);

        //    Notification notification = new Notification()
        //    {
        //        ProjectId = projectId,
        //        TypeId = typeId,
        //        Recipients = users,
        //        SendDate = DateTimeOffset.Now,
        //        Message = msgBody
        //    };

        //    return notification;
        //}

        public static Notification CreateProjectNotification(this int projectId, NotificationType type, List<ApplicationUser> recipients, string msgBody)
        {
            Notification notification = new Notification()
            {
                ProjectId = projectId,
                TypeId = type.Id,
                Recipients = recipients,
                SendDate = DateTimeOffset.Now,
                Message = msgBody
            };

            return notification;
        }
    }
}