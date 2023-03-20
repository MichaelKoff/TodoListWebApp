using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Interfaces;

namespace TodoList.Domain.DAL.Repositories
{
    public class ToDoListTaskRepository : IRepository<ToDoListTask>
    {
        private readonly ApplicationDbContext _context;

        public ToDoListTaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ToDoListTask todoListTask)
        {
            await _context.AddAsync(todoListTask);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ToDoListTask todoListTask)
        {
            _context.ToDoListTasks.Remove(todoListTask);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ToDoListTask>> GetAllAsync()
        {
            return await _context.ToDoListTasks
                .ToListAsync();
        }

        public async Task<ToDoListTask> GetByIdAsync(int id)
        {
            return await _context.ToDoListTasks.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync(ToDoListTask todoListTask)
        {
            _context.Entry(todoListTask).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
