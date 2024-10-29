using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Task_Management_System.Models
{
    public class TaskManagementDbContext : IdentityDbContext<IdentityUser>
    {
        public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Duty> Duties { get; set; }  // Keep the DbSet name aligned with the new class

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure Identity tables are configured

            modelBuilder.Entity<Duty>(entity =>
            {
                entity.HasKey(e => e.TaskId);

                entity.Property(e => e.Title)
                    .HasMaxLength(255);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.DueDate)
                    .HasColumnType("datetime");

                entity.Property(e => e.Priority)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Category)
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .IsRequired(false);

                // Configure the UserId property
                entity.Property(e => e.UserId)
                    .IsRequired(); // Assuming UserId cannot be null
            });
        }
    }
}
