﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using TodoList.Domain.BLL.Interfaces;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Enums;
using TodoList.MVC.Models;
using TodoList.MVC.ViewModels;

namespace TodoList.MVC.Controllers
{
    [Authorize]
    public class ToDoListController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToDoListService _todoListService;
        private readonly IToDoListTaskService _todoListTaskService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ToDoListController(IMapper mapper, IToDoListService todoListService, IToDoListTaskService todoListTaskService, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _todoListService = todoListService;
            _todoListTaskService = todoListTaskService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;

                var todoLists = await _todoListService.GetAllAsync(userId);
                var model = new CombinedTodoListViewModel
                {
                    TodoLists = _mapper.Map<List<ToDoListViewModel>>(todoLists),
                    SelectedTodoList = null
                };

                return View(model);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ToDoListViewModel toDoListViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var toDoList = _mapper.Map<ToDoList>(toDoListViewModel);
            toDoList.ApplicationUser = user;
            toDoList.ApplicationUserId = userId;

            await _todoListService.AddAsync(toDoList);

            return await GetTodoListContainerAsync(userId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            await _todoListService.DeleteAsync(id, userId);

            return await GetTodoListContainerAsync(userId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ToDoListViewModel toDoListViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var list = _mapper.Map<ToDoList>(toDoListViewModel);
            list.ApplicationUser = user;
            list.ApplicationUserId = userId;

            await _todoListService.UpdateAsync(list);

            return await GetTodoListContainerAsync(userId);
        }

        [HttpPost("[controller]/todolist/{id}")]
        public async Task<IActionResult> TodoListTasks(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;

                var todoList = await _todoListService.GetByIdAsync(id, userId);

                if (todoList == null)
                {
                    return PartialView("_TodoListTasksError");
                }

                return PartialView("_TodoListTasks", _mapper.Map<ToDoListViewModel>(todoList));
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpGet("[controller]/todolist/{id}")]
        public async Task<IActionResult> GetTasks(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var selectedTodoList = await _todoListService.GetByIdAsync(id, user.Id);
                var todoLists = await _todoListService.GetAllAsync(user.Id);

                var model = new CombinedTodoListViewModel
                {
                    TodoLists = _mapper.Map<List<ToDoListViewModel>>(todoLists),
                    SelectedTodoList = _mapper.Map<ToDoListViewModel>(selectedTodoList),
                    IsListNotFound = selectedTodoList == null
                };

                return View("Index", model);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost("[controller]/TasksDueToday")]
        public async Task<IActionResult> TasksDueToday()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var todoListViewModel = new ToDoListViewModel();
                var tasks = await _todoListTaskService.GetAllAsync(user.Id);
                var tasksDueToday = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today.Date).ToList();

                todoListViewModel.ToDoListTasks = _mapper.Map<List<ToDoListTaskViewModel>>(tasksDueToday);

                return PartialView("_TasksDueToday", todoListViewModel);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpGet("[controller]/TasksDueToday")]
        public async Task<IActionResult> GetTasksDueToday()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var tasks = await _todoListTaskService.GetAllAsync(user.Id);
                var tasksDueToday = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today.Date).ToList();

                var todoListViewModel = new ToDoListViewModel();
                todoListViewModel.ToDoListTasks = _mapper.Map<List<ToDoListTaskViewModel>>(tasksDueToday);

                var todoLists = await _todoListService.GetAllAsync(user.Id);

                var model = new CombinedTodoListViewModel()
                {
                    TodoLists = _mapper.Map<List<ToDoListViewModel>>(todoLists),
                    SelectedTodoList = todoListViewModel,
                    IsDueToday = true
                };

                return View("Index", model);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(ToDoListTaskViewModel task)
        {
            var taskToAdd = _mapper.Map<ToDoListTask>(task);
            await _todoListTaskService.AddAsync(taskToAdd);

            return await TodoListTasks(taskToAdd.ToDoListId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var task = await _todoListTaskService.GetByIdAsync(id, user.Id);
            int todoListId = task.ToDoListId;

            await _todoListTaskService.DeleteAsync(id, user.Id);

            if (Request.Headers["Referer"].ToString().Contains("TasksDueToday"))
            {
                return await TasksDueToday();
            }
            else
            {
                return await TodoListTasks(todoListId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTask(ToDoListTaskViewModel task)
        {
            var user = await _userManager.GetUserAsync(User);

            var taskToUpdate = _mapper.Map<ToDoListTask>(task);
            var todoList = await _todoListService.GetByIdAsync(taskToUpdate.ToDoListId, user.Id);
            taskToUpdate.ToDoList = todoList;

            await _todoListTaskService.UpdateAsync(taskToUpdate);

            ModelState.Clear();

            if (Request.Headers["Referer"].ToString().Contains("TasksDueToday"))
            {
                return await TasksDueToday();
            }
            else
            {
                return await TodoListTasks(taskToUpdate.ToDoListId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTaskStatus(int id, TodoStatus newStatus)
        {
            var user = await _userManager.GetUserAsync(User);

            var taskToUpdate = await _todoListTaskService.GetByIdAsync(id, user.Id);
            taskToUpdate.Status = newStatus;

            await _todoListTaskService.UpdateAsync(taskToUpdate);

            ModelState.Clear();

            if (Request.Headers["Referer"].ToString().Contains("TasksDueToday"))
            {
                return await TasksDueToday();
            }
            else
            {
                return await TodoListTasks(taskToUpdate.ToDoListId);
            }
        }

        private async Task<IActionResult> GetTodoListContainerAsync(string userId)
        {
            var todoLists = await _todoListService.GetAllAsync(userId);
            var model = _mapper.Map<List<ToDoListViewModel>>(todoLists);
            return PartialView("_TodoListContainer", model);
        }
    }
}
