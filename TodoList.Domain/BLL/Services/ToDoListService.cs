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
    public class ToDoListService : IToDoListService
    {
        private readonly IRepository<ToDoList> _repository;

        public ToDoListService(IRepository<ToDoList> repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(ToDoList todoList)
        {
            await _repository.AddAsync(todoList);
        }

        public async Task DeleteAsync(ToDoList todoList)
        {
            await _repository.DeleteAsync(todoList);
        }

        public async Task<List<ToDoList>> GetAllAsync(string userId)
        {
            return await _repository.GetAllAsync(userId);
        }

        public async Task<ToDoList> GetByIdAsync(int id, string userId)
        {
            return await _repository.GetByIdAsync(id, userId);
        }

        public async Task UpdateAsync(ToDoList todoList)
        {
            await _repository.UpdateAsync(todoList);
        }
    }
}
