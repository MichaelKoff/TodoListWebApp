using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TodoList.Domain.DAL.Entities;

namespace TodoList.Domain
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<ToDoList> ToDoLists { get; set; }

        public DbSet<ToDoListTask> ToDoListTasks { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
            .HasMany(u => u.ToDoLists)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId);

            builder.Entity<ToDoList>()
                .HasMany(l => l.ToDoListTasks)
                .WithOne(t => t.ToDoList)
                .HasForeignKey(t => t.ToDoListId);
        }
    }
}