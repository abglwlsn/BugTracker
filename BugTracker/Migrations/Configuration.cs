namespace BugTracker.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            //-----------------create roles
            if (!context.Roles.Any(r => r.Name == "Developer"))
                roleManager.Create(new IdentityRole { Name = "Developer" });

            if (!context.Roles.Any(r => r.Name == "ProjectManager"))
                roleManager.Create(new IdentityRole { Name = "Project Manager" });

            if (!context.Roles.Any(r => r.Name == "Administrator"))
                roleManager.Create(new IdentityRole { Name = "Administrator" });

            if (!context.Roles.Any(r => r.Name == "Submitter"))
                roleManager.Create(new IdentityRole { Name = "Submitter" });

            //--------------add as administrator
            if (!context.Users.Any(u => u.Email == "abigailwwest@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "abigailwwest@gmail.com",
                    Email = "abigailwwest@gmail.com",
                    FirstName = "Abigail",
                    LastName = "West"
                }, "Budget1!");
            }
            var userId = userManager.FindByEmail("abigailwwest@gmail.com").Id;
            userManager.AddToRole(userId, "Administrator");

            //---------------seed look up tables
            context.Priorities.AddOrUpdate(p => p.Name,
                new Priority() { Name = "Explosive" },
                new Priority() { Name = "Urgent" },
                new Priority() { Name = "Important" },
                new Priority() { Name = "Enhancement" },
                new Priority() { Name = "Wish List" }
                );

            context.Statuses.AddOrUpdate(s => s.Name,
                new Status() { Name = "Unassigned" },
                new Status() { Name = "Assigned" },
                new Status() { Name = "In Progress" },
                new Status() { Name = "In Review" },
                new Status() { Name = "Resolved" }
                );

            context.TicketTypes.AddOrUpdate(t => t.Name,
                new TicketPhase() { Name = "Design" },
                new TicketPhase() { Name = "Testing" },
                new TicketPhase() { Name = "Database" },
                new TicketPhase() { Name = "Routing" },
                new TicketPhase() { Name = "User Interface" },
                new TicketPhase() { Name = "Security" },
                new TicketPhase() { Name = "Integrated" }
                );

            context.TicketActions.AddOrUpdate(a => a.Name,
                new TicketAction() { Name = "Build New" },
                new TicketAction() { Name = "Add On" },
                new TicketAction() { Name = "Refactor" },
                new TicketAction() { Name = "Repair" },
                new TicketAction() { Name = "Replace" }
                );

            context.NotificationTypes.AddOrUpdate(n => n.Name,
                new NotificationType() { Name = "Ticket Assigned" },
                new NotificationType() { Name = "Ticket Resolved" },
                new NotificationType() { Name = "Reminder: Update Tickets" },
                new NotificationType() { Name = "Ticket Modified" },
                new NotificationType() { Name = "Ticket Reassigned" }
                );
        }
    }
}
