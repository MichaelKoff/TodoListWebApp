using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Domain.BLL.Interfaces;
using TodoList.Domain.Constants;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Enums;
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
            ValidateTaskOnCreate(todoListTask);

            todoListTask.CreationDate = DateTime.Now;
            todoListTask.Status = TodoStatus.NotStarted;

            await _repository.AddAsync(todoListTask);
        }

        public async Task<ToDoListTask> GetByIdAsync(int id, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty");
            }

            return await _repository.GetByIdAsync(id, userId);
        }

        public async Task<List<ToDoListTask>> GetAllAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty");
            }

            return await _repository.GetAllAsync(userId);
        }

        public async Task<List<ToDoListTask>> GetTasksDueTodayAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty");
            }

            var tasks = await _repository.GetAllAsync(userId);
            var tasksDueToday = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today.Date).ToList();
            return tasksDueToday;
        }

        public async Task UpdateAsync(ToDoListTask todoListTask, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty");
            }

            ValidateTaskOnUpdate(todoListTask);

            var taskToUpdate = await _repository.GetByIdAsync(todoListTask.Id, userId);

            if (taskToUpdate == null)
            {
                throw new ArgumentException($"Task with the specified ID \"{todoListTask.Id}\" does not exist.");
            }

            var taskProperties = typeof(ToDoListTask).GetProperties();

            foreach (var property in taskProperties)
            {
                if (property.Name == nameof(ToDoListTask.ToDoListId) || property.Name == nameof(ToDoListTask.CreationDate) || property.PropertyType == typeof(ToDoList))
                {
                    continue;
                }

                var newValue = property.GetValue(todoListTask);

                var existingValue = property.GetValue(taskToUpdate);

                if (!Equals(newValue, existingValue))
                {
                    property.SetValue(taskToUpdate, newValue);
                }
            }

            await _repository.UpdateAsync(taskToUpdate);
        }

        public async Task UpdateStatusAsync(int id, string userId, TodoStatus newStatus)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty");
            }

            var taskToUpdate = await _repository.GetByIdAsync(id, userId);

            if (taskToUpdate == null)
            {
                throw new ArgumentException($"Task with the specified ID \"{id}\" does not exist.", nameof(id));
            }

            taskToUpdate.Status = newStatus;

            await _repository.UpdateAsync(taskToUpdate);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty");
            }

            var taskToDelete = await _repository.GetByIdAsync(id, userId);

            if (taskToDelete == null)
            {
                throw new ArgumentException($"Task with the specified ID \"{id}\" does not exist.", nameof(id));
            }

            await _repository.DeleteAsync(taskToDelete);
        }

        private static void ValidateTaskOnCreate(ToDoListTask todoListTask)
        {
            if (todoListTask.ToDoListId <= 0)
            {
                throw new ArgumentException("Unset or invalid Todo list id");
            }

            if (string.IsNullOrWhiteSpace(todoListTask.Title))
            {
                throw new ArgumentException("Title cannot be empty or whitespace.");
            }

            if (todoListTask.Title.Length > AppConstants.MaxToDoListTaskTitleLength)
            {
                throw new ArgumentException($"Title cannot be longer than {AppConstants.MaxToDoListTaskTitleLength} characters");
            }
        }

        private static void ValidateTaskOnUpdate(ToDoListTask todoListTask)
        {
            if (string.IsNullOrWhiteSpace(todoListTask.Title))
            {
                throw new ArgumentException("Title cannot be empty or whitespace.");
            }

            if (todoListTask.Title.Length > AppConstants.MaxToDoListTaskTitleLength)
            {
                throw new ArgumentException($"Title cannot be longer than {AppConstants.MaxToDoListTaskTitleLength} characters");
            }

            if (todoListTask.Description != null && todoListTask.Description.Length > AppConstants.MaxToDoListTaskDescriptionLength)
            {
                throw new ArgumentException($"Description cannot be longer than {AppConstants.MaxToDoListTaskDescriptionLength} characters");
            }
        }
    }
}
