using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.HelperExtensions
{
    public static class NotificatonHelpers
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static void CreateTicketNotification(this int ticketId, string type, List<string> recipientIds, string msgBody)
        {
            var typeId = db.NotificationTypes.FirstOrDefault(n => n.Name == type).Id;

            Notification notification = new Notification()
            {
                TicketId = ticketId,
                TypeId = typeId,
                RecipientId = recipientIds,
                SendDate = DateTimeOffset.Now,
                Message = msgBody
            };

            db.Notifications.Add(notification);
            db.SaveChanges();          
        }

        public static void CreateProjectNotification(this int projectId, string type, List<string> recipientIds, string msgBody)
        {
            var typeId = db.NotificationTypes.FirstOrDefault(n => n.Name == type).Id;

            Notification notification = new Notification()
            {
                ProjectId = projectId,
                TypeId = typeId,
                RecipientId = recipientIds,
                SendDate = DateTimeOffset.Now,
                Message = msgBody
            };

            db.Notifications.Add(notification);
            db.SaveChanges();
        }
    }
}