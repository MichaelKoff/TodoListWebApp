using Microsoft.EntityFrameworkCore;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Interfaces;

namespace TodoList.Domain.DAL.Repositories
{
    public class ToDoListRepository : IRepository<ToDoList>
    {
        private readonly ApplicationDbContext _context;

        public ToDoListRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ToDoList todoList)
        {
            await _context.ToDoLists.AddAsync(todoList);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ToDoList todoList)
        {
            _context.ToDoLists.Remove(todoList);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ToDoList>> GetAllAsync()
        {
            return await _context.ToDoLists
                .Include(tl => tl.ToDoListTasks)
                .ToListAsync();
        }

        public async Task<ToDoList> GetByIdAsync(int id)
        {
            return await _context.ToDoLists
                .Include(tl => tl.ToDoListTasks)
                .FirstOrDefaultAsync(tl => tl.Id == id);
        }

        public async Task UpdateAsync(ToDoList todoList)
        {
            _context.Entry(todoList).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
