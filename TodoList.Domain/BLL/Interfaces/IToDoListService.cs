﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Domain.DAL.Entities;

namespace TodoList.Domain.BLL.Interfaces
{
    public interface IToDoListService
    {
        Task AddAsync(ToDoList todoList);
        Task<ToDoList> GetByIdAsync(int id, string userId);
        Task<List<ToDoList>> GetAllAsync(string userId);
        Task UpdateAsync(ToDoList todoList);
        Task DeleteAsync(int id, string userId);
        Task DuplicateAsync(int id, string userId);
    }
}
