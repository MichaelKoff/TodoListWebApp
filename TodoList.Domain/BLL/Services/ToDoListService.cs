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
            if (todoList.ApplicationUser == null)
            {
                throw new ArgumentNullException(nameof(todoList.ApplicationUser), "User cannot be null.");
            }

            if (string.IsNullOrEmpty(todoList.ApplicationUserId))
            {
                throw new ArgumentNullException(nameof(todoList.ApplicationUser), "User Id cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(todoList.Title))
            {
                throw new ArgumentException("Title cannot be empty or whitespace.", nameof(todoList.Title));
            }

            if (todoList.Title.Length > 50)
            {
                throw new ArgumentException("Title cannot be longer than 50 characters", nameof(todoList.Title));
            }

            var existingList = await GetByTitleAsync(todoList.ApplicationUserId, todoList.Title);

            if (existingList == null)
            {
                await _repository.AddAsync(todoList);
                return;
            }

            var i = 1;
            var originalTitle = todoList.Title;

            while (existingList != null)
            {
                todoList.Title = $"{originalTitle} ({i})";
                existingList = await GetByTitleAsync(todoList.ApplicationUserId, todoList.Title);
                i++;
            }

            await _repository.AddAsync(todoList);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var todoList = await GetByIdAsync(id, userId);

            if (todoList == null)
            {
                throw new ArgumentException("Todo list with the specified ID and user ID does not exist.", nameof(id));
            }

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
            if (string.IsNullOrWhiteSpace(todoList.Title))
            {
                throw new ArgumentException("Title cannot be empty or whitespace.", nameof(todoList.Title));
            }

            if (todoList.Title.Length > 50)
            {
                throw new ArgumentException("Title cannot be longer than 50 characters", nameof(todoList.Title));
            }

            var listToUpdate = await _repository.GetByIdAsync(todoList.Id, todoList.ApplicationUserId);

            if (listToUpdate.Title.Equals(todoList.Title))
            {
                return;
            }

            var existingList = await GetByTitleAsync(todoList.ApplicationUserId, todoList.Title);

            if (existingList == null)
            {
                listToUpdate.Title = todoList.Title;
                await _repository.UpdateAsync(listToUpdate);
                return;
            }

            var i = 1;
            var originalTitle = todoList.Title;

            while (existingList != null)
            {
                todoList.Title = $"{originalTitle} ({i})";
                existingList = await GetByTitleAsync(todoList.ApplicationUserId, todoList.Title);
                i++;
            }

            listToUpdate.Title = todoList.Title;

            await _repository.UpdateAsync(listToUpdate);
        }

        private async Task<ToDoList> GetByTitleAsync(string userId, string title)
        {
            var todoLists = await GetAllAsync(userId);
            var todoList = todoLists.FirstOrDefault(tl => tl.Title.Equals(title));

            return todoList;
        }
    }
}
