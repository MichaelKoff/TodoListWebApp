using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Domain.DAL.Entities;

namespace TodoList.Domain.BLL.Interfaces
{
    public interface IToDoListTaskService
    {
        Task<ToDoListTask> GetByIdAsync(int id, string userId);
        Task<List<ToDoListTask>> GetAllAsync(string userId);
        Task AddAsync(ToDoListTask todoListTask);
        Task UpdateAsync(ToDoListTask todoListTask);
        Task DeleteAsync(ToDoListTask todoListTask);
    }
}
