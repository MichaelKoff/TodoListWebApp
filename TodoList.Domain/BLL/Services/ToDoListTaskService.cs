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

        public async Task AddAsync(ToDoListTask task)
        {
            if (string.IsNullOrWhiteSpace(task.Title))
            {
                throw new ArgumentException("Title cannot be empty or whitespace.", nameof(task.Title));
            }

            if (task.Title.Length > 50)
            {
                throw new ArgumentException("Title cannot be longer than 50 characters", nameof(task.Title));
            }

            await _repository.AddAsync(task);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var taskToDelte = await _repository.GetByIdAsync(id, userId);

            if (taskToDelte == null)
            {
                throw new ArgumentException("Task with the specified ID does not exist.", nameof(id));
            }

            await _repository.DeleteAsync(taskToDelte);
        }

        public async Task<List<ToDoListTask>> GetAllAsync(string userId)
        {
            return await _repository.GetAllAsync(userId);
        }

        public async Task<ToDoListTask> GetByIdAsync(int id, string userId)
        {
            return await _repository.GetByIdAsync(id, userId);
        }

        public async Task UpdateAsync(ToDoListTask task)
        {
            if (string.IsNullOrWhiteSpace(task.Title))
            {
                throw new ArgumentException("Title cannot be empty or whitespace.", nameof(task.Title));
            }

            if (task.Title.Length > 50)
            {
                throw new ArgumentException("Title cannot be longer than 50 characters", nameof(task.Title));
            }

            if (task.Description != null && task.Description.Length > 255)
            {
                throw new ArgumentException("Description cannot be longer than 255 characters", nameof(task.Description));
            }

            var taskToUpdate = await _repository.GetByIdAsync(task.Id, task.ToDoList.ApplicationUserId);
            var taskProperties = typeof(ToDoListTask).GetProperties();

            foreach (var property in taskProperties)
            {
                if (property.Name == nameof(ToDoListTask.ToDoListId) || property.Name == nameof(ToDoListTask.CreationDate) || property.PropertyType == typeof(ToDoList))
                {
                    continue;
                }

                var newValue = property.GetValue(task);

                // Get the value of the property from the existing task
                var existingValue = property.GetValue(taskToUpdate);

                // Compare the values
                if (!Equals(newValue, existingValue))
                {
                    // Update the value of the property in the existing task
                    property.SetValue(taskToUpdate, newValue);
                }
            }

            await _repository.UpdateAsync(taskToUpdate);
        }
    }
}
