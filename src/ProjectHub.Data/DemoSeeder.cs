using Microsoft.EntityFrameworkCore;
using ProjectHub.Data.Models;

namespace ProjectHub.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        if (await db.Projects.AnyAsync(cancellationToken))
        {
            return;
        }

        var today = DateTime.UtcNow.Date;

        var projects = new[]
        {
            new Project
            {
                Name = "ProjectHub Workshop",
                Description = "Example project used during the workshop.",
                Status = ProjectStatus.Active,
                WorkItems =
                [
                    new WorkItem
                    {
                        Title = "Implement authentication",
                        Description = "Add authentication to ProjectHub.",
                        Priority = WorkItemPriority.High,
                        Status = WorkItemStatus.InProgress,
                        DueDateUtc = today.AddDays(-2)
                    },
                    new WorkItem
                    {
                        Title = "Configure project dashboard",
                        Priority = WorkItemPriority.Normal,
                        Status = WorkItemStatus.New,
                        DueDateUtc = today.AddDays(5)
                    },
                    new WorkItem
                    {
                        Title = "Add Redis caching",
                        Priority = WorkItemPriority.Normal,
                        Status = WorkItemStatus.New
                    }
                ]
            },
            new Project
            {
                Name = "Customer Portal",
                Description = "Build a self-service portal for customers.",
                Status = ProjectStatus.Active,
                WorkItems =
                [
                    new WorkItem
                    {
                        Title = "Design customer profile page",
                        Description = "Create the layout for customer profile information.",
                        Priority = WorkItemPriority.High,
                        Status = WorkItemStatus.Completed,
                        DueDateUtc = today.AddDays(-10)
                    },
                    new WorkItem
                    {
                        Title = "Add support ticket submission",
                        Description = "Allow customers to submit and track support tickets.",
                        Priority = WorkItemPriority.High,
                        Status = WorkItemStatus.InProgress,
                        DueDateUtc = today.AddDays(7)
                    },
                    new WorkItem
                    {
                        Title = "Add notification preferences",
                        Priority = WorkItemPriority.Normal,
                        Status = WorkItemStatus.New,
                        DueDateUtc = today.AddDays(14)
                    }
                ]
            },
            new Project
            {
                Name = "Reporting Modernization",
                Description = "Modernize the organization's reporting experience.",
                Status = ProjectStatus.OnHold,
                WorkItems =
                [
                    new WorkItem
                    {
                        Title = "Document existing reports",
                        Description = "Catalog the reports currently used by the business.",
                        Priority = WorkItemPriority.Normal,
                        Status = WorkItemStatus.Completed,
                        DueDateUtc = today.AddDays(-20)
                    },
                    new WorkItem
                    {
                        Title = "Create reporting data model",
                        Priority = WorkItemPriority.High,
                        Status = WorkItemStatus.Blocked,
                        DueDateUtc = today.AddDays(3)
                    },
                    new WorkItem
                    {
                        Title = "Build executive summary dashboard",
                        Priority = WorkItemPriority.Normal,
                        Status = WorkItemStatus.New,
                        DueDateUtc = today.AddDays(21)
                    }
                ]
            },
            new Project
            {
                Name = "Mobile Application",
                Description = "Deliver a mobile companion application for ProjectHub.",
                Status = ProjectStatus.Active,
                WorkItems =
                [
                    new WorkItem
                    {
                        Title = "Create mobile wireframes",
                        Priority = WorkItemPriority.Normal,
                        Status = WorkItemStatus.Completed,
                        DueDateUtc = today.AddDays(-5)
                    },
                    new WorkItem
                    {
                        Title = "Implement offline synchronization",
                        Description = "Support synchronizing changes when connectivity is restored.",
                        Priority = WorkItemPriority.High,
                        Status = WorkItemStatus.InProgress,
                        DueDateUtc = today.AddDays(12)
                    },
                    new WorkItem
                    {
                        Title = "Configure mobile release pipeline",
                        Priority = WorkItemPriority.Low,
                        Status = WorkItemStatus.New,
                        DueDateUtc = today.AddDays(30)
                    }
                ]
            }
        };

        db.Projects.AddRange(projects);

        await db.SaveChangesAsync(cancellationToken);
    }
}