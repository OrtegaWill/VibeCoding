using Microsoft.EntityFrameworkCore;
using TicketManagement.Core.Models;

namespace TicketManagement.Infrastructure.Data
{
    public class TicketManagementDbContext : DbContext
    {
        public TicketManagementDbContext(DbContextOptions<TicketManagementDbContext> options) : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistory> TicketHistory { get; set; }
        public DbSet<EmailFilter> EmailFilters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ticket configuration
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TicketNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.Property(e => e.RequestedBy).HasMaxLength(100);
                entity.Property(e => e.CustomerEmail).HasMaxLength(100);
                entity.Property(e => e.EmailMessageId).HasMaxLength(255);
                entity.Property(e => e.EmailThreadId).HasMaxLength(255);
                
                entity.HasIndex(e => e.TicketNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.AssignedTo);
                entity.HasIndex(e => e.CustomerEmail);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.EmailMessageId);
            });

            // TicketComment configuration
            modelBuilder.Entity<TicketComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Author).HasMaxLength(100);
                entity.Property(e => e.AuthorEmail).HasMaxLength(100);
                entity.Property(e => e.EmailMessageId).HasMaxLength(255);
                
                entity.HasOne(e => e.Ticket)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TicketHistory configuration
            modelBuilder.Entity<TicketHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Field).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OldValue).HasMaxLength(500);
                entity.Property(e => e.NewValue).HasMaxLength(500);
                entity.Property(e => e.ChangedBy).HasMaxLength(100);
                entity.Property(e => e.ChangeReason).HasMaxLength(500);
                
                entity.HasOne(e => e.Ticket)
                    .WithMany(e => e.History)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // EmailFilter configuration
            modelBuilder.Entity<EmailFilter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.FromEmail).HasMaxLength(100);
                entity.Property(e => e.FromDomain).HasMaxLength(100);
                entity.Property(e => e.SubjectContains).HasMaxLength(200);
                entity.Property(e => e.BodyContains).HasMaxLength(200);
                entity.Property(e => e.DefaultAssignee).HasMaxLength(100);
                
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Order);
            });

            // Seed data for EmailFilters
            modelBuilder.Entity<EmailFilter>().HasData(
                new EmailFilter
                {
                    Id = 1,
                    Name = "Support Requests",
                    Description = "Filter for general support requests",
                    IsActive = true,
                    SubjectContains = "support",
                    DefaultPriority = TicketPriority.Medium,
                    DefaultCategory = TicketCategory.Support,
                    Order = 1
                },
                new EmailFilter
                {
                    Id = 2,
                    Name = "Bug Reports",
                    Description = "Filter for bug reports and issues",
                    IsActive = true,
                    SubjectContains = "bug",
                    DefaultPriority = TicketPriority.High,
                    DefaultCategory = TicketCategory.Bug,
                    Order = 2
                },
                new EmailFilter
                {
                    Id = 3,
                    Name = "Feature Requests",
                    Description = "Filter for feature requests",
                    IsActive = true,
                    SubjectContains = "feature",
                    DefaultPriority = TicketPriority.Medium,
                    DefaultCategory = TicketCategory.Feature,
                    Order = 3
                }
            );
        }
    }
} 