using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Domain.BLL.Interfaces;
using TodoList.Domain.Constants;
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
            ValidateToDoList(todoList);

            var listWithSameTitle = await GetByTitleAsync(todoList.ApplicationUserId, todoList.Title);

            if (listWithSameTitle == null)
            {
                await _repository.AddAsync(todoList);
                return;
            }

            todoList.Title = await GetUniqueTitleAsync(todoList);

            await _repository.AddAsync(todoList);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty");
            }

            var todoList = await _repository.GetByIdAsync(id, userId);

            if (todoList == null)
            {
                throw new ArgumentException($"Todo list with the specified ID \"{id}\" and user ID \"{userId}\" does not exist.", nameof(id));
            }

            await _repository.DeleteAsync(todoList);
        }

        public async Task DuplicateAsync(int id, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty");
            }

            var existingTodoList = await _repository.GetByIdAsync(id, userId);

            if (existingTodoList == null)
            {
                throw new ArgumentException($"Todo list with the specified ID \"{id}\" and user ID \"{userId}\" does not exist.", nameof(id));
            }

            var duplicatedTodoList = new ToDoList()
            {
                Title = existingTodoList.Title,
                ApplicationUser = existingTodoList.ApplicationUser,
                ApplicationUserId = existingTodoList.ApplicationUserId,
                ToDoListTasks = new List<ToDoListTask>()
            };

            foreach (var task in existingTodoList.ToDoListTasks)
            {
                var duplicatedTask = new ToDoListTask
                {
                    Title = task.Title,
                    Description = task.Description,
                    DueDate = task.DueDate,
                    Status = task.Status,
                    Reminder = task.Reminder,
                    ToDoList = duplicatedTodoList,
                };

                duplicatedTodoList.ToDoListTasks.Add(duplicatedTask);
            }

            await AddAsync(duplicatedTodoList);
        }

        public async Task<List<ToDoList>> GetAllAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty");
            }

            return await _repository.GetAllAsync(userId);
        }

        public Task<ToDoList> GetByIdAsync(int id, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty");
            }

            return _repository.GetByIdAsync(id, userId);
        }

        public async Task UpdateAsync(ToDoList todoList)
        {
            ValidateToDoList(todoList);

            var listToUpdate = await _repository.GetByIdAsync(todoList.Id, todoList.ApplicationUserId);

            if (listToUpdate == null)
            {
                throw new ArgumentException($"Todo list with the specified ID \"{todoList.Id}\" and user ID \"{todoList.ApplicationUserId}\" does not exist.");
            }

            if (listToUpdate.Title.Equals(todoList.Title))
            {
                return;
            }

            var listWithSameTitle = await GetByTitleAsync(todoList.ApplicationUserId, todoList.Title);

            if (listWithSameTitle == null)
            {
                listToUpdate.Title = todoList.Title;
                await _repository.UpdateAsync(listToUpdate);
                return;
            }

            listToUpdate.Title = await GetUniqueTitleAsync(todoList);

            await _repository.UpdateAsync(listToUpdate);
        }

        private async Task<ToDoList> GetByTitleAsync(string userId, string title)
        {
            var todoLists = await _repository.GetAllAsync(userId);
            var todoList = todoLists.FirstOrDefault(tl => tl.Title.Equals(title));

            return todoList;
        }

        private async Task<string> GetUniqueTitleAsync(ToDoList todoList)
        {
            var i = 1;
            var title = todoList.Title;
            var listWithSameTitle = await GetByTitleAsync(todoList.ApplicationUserId, title);

            while (listWithSameTitle != null)
            {
                title = $"{todoList.Title} ({i})";
                listWithSameTitle = await GetByTitleAsync(todoList.ApplicationUserId, title);
                i++;
            }

            return title;
        }

        private static void ValidateToDoList(ToDoList todoList)
        {
            if (todoList.ApplicationUser == null)
            {
                throw new ArgumentException("User cannot be null.");
            }

            if (string.IsNullOrEmpty(todoList.ApplicationUserId))
            {
                throw new ArgumentException("User Id cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(todoList.Title))
            {
                throw new ArgumentException("Title cannot be empty or whitespace.");
            }

            if (todoList.Title.Length > AppConstants.MaxToDoListTitleLength)
            {
                throw new ArgumentException($"Title cannot be longer than {AppConstants.MaxToDoListTitleLength} characters");
            }
        }
    }
}
