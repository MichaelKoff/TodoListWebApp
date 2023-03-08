using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Domain.BLL.Interfaces;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Interfaces;

namespace TodoList.Domain.BLL.Services
{
    public class ToDoListTaskService : IToDoListTaskService
    {
        private readonly IRepository<ToDoListTask> _repository;

        public ToDoListTaskService(IRepository<ToDoListTask> repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(ToDoListTask todoListTask)
        {
            await _repository.AddAsync(todoListTask);
        }

        public async Task DeleteAsync(ToDoListTask todoListTask)
        {
            await _repository.DeleteAsync(todoListTask);
        }

        public async Task<List<ToDoListTask>> GetAllAsync(string userId)
        {
            return await _repository.GetAllAsync(userId);
        }

        public async Task<ToDoListTask> GetByIdAsync(int id, string userId)
        {
            return await _repository.GetByIdAsync(id, userId);
        }

        public async Task UpdateAsync(ToDoListTask todoListTask)
        {
            await _repository.UpdateAsync(todoListTask);
        }
    }
}
