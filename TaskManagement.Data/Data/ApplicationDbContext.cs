using Microsoft.EntityFrameworkCore;
using System;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto.UserTask;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Tasks> Tasks { get; set; }
    public DbSet<TaskDetail> TaskDetails { get; set; }
    public DbSet<TaskState> TaskStates { get; set; }
    //public DbSet<UserTask> UserTasks { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }





    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft delete filters
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Tasks>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<TaskDetail>().HasQueryFilter(td => !td.IsDeleted);
        modelBuilder.Entity<TaskState>().HasQueryFilter(ts => !ts.IsDeleted);
        modelBuilder.Entity<TaskDto1>().HasNoKey();
        //modelBuilder.Entity<UserTask>().HasQueryFilter(ut => !ut.IsDeleted);

        // Relationships

        modelBuilder.Entity<Tasks>()
            .HasOne(t => t.User)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Tasks>()
            .HasOne(t => t.TaskStatus)
            .WithMany(ts => ts.Tasks)
            .HasForeignKey(t => t.TaskStatusId);

        modelBuilder.Entity<TaskDetail>()
            .HasOne(td => td.Tasks)
            .WithMany(t => t.TaskDetails)
            .HasForeignKey(td => td.TaskId);

        modelBuilder.Entity<Tasks>()
            .HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);




        //modelBuilder.Entity<UserTask>()
        //    .HasKey(ut => new { ut.UserId, ut.TaskId });

        //modelBuilder.Entity<UserTask>()
        //    .HasOne(ut => ut.User)
        //    .WithMany()
        //    .HasForeignKey(ut => ut.UserId);

        //modelBuilder.Entity<UserTask>()
        //    .HasOne(ut => ut.Task)
        //    .WithMany()
        //    .HasForeignKey(ut => ut.TaskId)
        //    .OnDelete(DeleteBehavior.Restrict);

        //modelBuilder.Entity<UserTask>()
        //    .HasOne(ut => ut.TaskStatus)
        //    .WithMany()
        //    .HasForeignKey(ut => ut.TaskStatusId);

        //modelBuilder.Entity<UserTask>()
        //    .HasOne(ut => ut.TaskDetail)
        //    .WithMany()
        //    .HasForeignKey(ut => ut.TaskId)
        //    .OnDelete(DeleteBehavior.Restrict); // optional if removed



        modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => !rt.IsDeleted);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed TaskStates
        modelBuilder.Entity<TaskState>().HasData(
            new TaskState { Id = 1, Name = "Unassigned", IsDeleted = false },
            new TaskState { Id = 2, Name = "Todo", IsDeleted = false },
            new TaskState { Id = 3, Name = "InProgress", IsDeleted = false },
            new TaskState { Id = 4, Name = "Done", IsDeleted = false }
        );
    }
}
    