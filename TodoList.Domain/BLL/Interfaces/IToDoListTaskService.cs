using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Enums;

namespace TodoList.Domain.BLL.Interfaces
{
    public interface IToDoListTaskService
    {
        Task AddAsync(ToDoListTask todoListTask);
        Task<ToDoListTask> GetByIdAsync(int id, string userId);
        Task<List<ToDoListTask>> GetAllAsync(string userId);
        Task<List<ToDoListTask>> GetTasksDueTodayAsync(string userId);
        Task UpdateAsync(ToDoListTask todoListTask, string userId);
        Task UpdateStatusAsync(int id, string userId, TodoStatus newStatus);
        Task DeleteAsync(int id, string userId);
    }
}
