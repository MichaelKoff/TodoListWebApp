using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
        private readonly ILogger<ToDoListController> _logger;
        private readonly IMapper _mapper;
        private readonly IToDoListService _todoListService;
        private readonly IToDoListTaskService _todoListTaskService;
        private readonly UserManager<User> _userManager;

        public ToDoListController(ILogger<ToDoListController> logger, IMapper mapper, IToDoListService todoListService, IToDoListTaskService todoListTaskService, UserManager<User> userManager)
        {
            _logger = logger;
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

                var todoLists = await _todoListService.GetAllAsync(user.Id);
                _logger.LogInformation("Retrieved all Todo lists for user {UserId}", user.Id);

                var model = new CombinedTodoListViewModel
                {
                    TodoLists = _mapper.Map<List<ToDoListViewModel>>(todoLists),
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(Index));
                return RedirectToAction("Error", "Home", new { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [Route("/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ToDoListViewModel toDoListViewModel)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var toDoList = _mapper.Map<ToDoList>(toDoListViewModel);
                toDoList.User = user;
                toDoList.UserId = user.Id;

                await _todoListService.AddAsync(toDoList);
                _logger.LogInformation("User {UserId} created new Todo list with Id {TodoListId}", user.Id, toDoList.Id);

                ModelState.Clear();
                return await GetTodoListContainerAsync(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(Create));
                var errorUrl = Url.Action("Error", "Home", new { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                return BadRequest(errorUrl);
            }
        }

        [HttpPost("/Duplicate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                await _todoListService.DuplicateAsync(id, user.Id);
                _logger.LogInformation("User {UserId} duplicated Todo list with Id {TodoListId}", user.Id, id);

                return await GetTodoListContainerAsync(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(Duplicate));
                var errorUrl = Url.Action("Error", "Home", new { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                return BadRequest(errorUrl);
            }
        }

        [HttpPost("/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                await _todoListService.DeleteAsync(id, user.Id);
                _logger.LogInformation("User {UserId} deleted Todo list with Id {TodoListId}", user.Id, id);

                return await GetTodoListContainerAsync(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(Delete));
                var errorUrl = Url.Action("Error", "Home", new { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                return BadRequest(errorUrl);
            }
        }

        [HttpPost("/Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ToDoListViewModel toDoListViewModel)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var list = _mapper.Map<ToDoList>(toDoListViewModel);
                list.User = user;
                list.UserId = user.Id;

                await _todoListService.UpdateAsync(list);
                _logger.LogInformation("User {UserId} updated Todo list with Id {TodoListId}", user.Id, list.Id);

                ModelState.Clear();
                return await GetTodoListContainerAsync(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(Update));
                var errorUrl = Url.Action("Error", "Home", new { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                return BadRequest(errorUrl);
            }
        }

        [HttpPost("/todolist/{id}")]
        public async Task<IActionResult> TodoListTasks(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var todoList = await _todoListService.GetByIdAsync(id, user.Id);

                if (todoList == null)
                {
                    _logger.LogWarning("User {UserId} attempted to retrieve a non-existing Todo list with Id {TodoListId}", user.Id, id);
                    return PartialView("_TodoListTasksError");
                }

                _logger.LogInformation("User {UserId} retrieved a Todo list with Id {TodoListId}", user.Id, id);

                var viewModel = _mapper.Map<ToDoListViewModel>(todoList);
                viewModel.ToDoListTasks.Sort((t1, t2) => t1.CreationDate.CompareTo(t2.CreationDate));

                return PartialView("_TodoListTasks", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(TodoListTasks));
                var errorUrl = Url.Action("Error", "Home", new { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                return BadRequest(errorUrl);
            }
        }

        [HttpGet("/todolist/{id}")]
        public async Task<IActionResult> GetTasks(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var todoLists = await _todoListService.GetAllAsync(user.Id);
                var selectedTodoList = todoLists.FirstOrDefault(tl => tl.Id == id);

                if (selectedTodoList == null)
                {
                    _logger.LogWarning("User {UserId} attempted to retrieve a non-existing Todo list with Id {TodoListId}", user.Id, id);
                }
                else
                {
                    _logger.LogInformation("User {UserId} retrieved a Todo list with Id {TodoListId}", user.Id, id);
                }

                var model = new CombinedTodoListViewModel
                {
                    TodoLists = _mapper.Map<List<ToDoListViewModel>>(todoLists),
                    SelectedTodoList = _mapper.Map<ToDoListViewModel>(selectedTodoList),
                    IsListNotFound = selectedTodoList == null
                };

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(GetTasks));
                return RedirectToAction("Error", "Home", new { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost("/TasksDueToday")]
        public async Task<IActionResult> TasksDueToday()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var tasksDueToday = await _todoListTaskService.GetTasksDueTodayAsync(user.Id);
                _logger.LogInformation("User {UserId} retrieved his tasks due today", user.Id);

                tasksDueToday.Sort((t1, t2) => t1.CreationDate.CompareTo(t2.CreationDate));
                var todoListViewModel = new ToDoListViewModel()
                {
                    ToDoListTasks = _mapper.Map<List<ToDoListTaskViewModel>>(tasksDueToday),
                };

                return PartialView("_TasksDueToday", todoListViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(TasksDueToday));
                var errorUrl = Url.Action("Error", "Home", new { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                return BadRequest(errorUrl);
            }
        }

        [HttpGet("/TasksDueToday")]
        public async Task<IActionResult> GetTasksDueToday()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var tasksDueToday = await _todoListTaskService.GetTasksDueTodayAsync(user.Id);
                _logger.LogInformation("User {UserId} retrieved his tasks due today", user.Id);

                var todoListViewModel = new ToDoListViewModel()
                {
                    ToDoListTasks = _mapper.Map<List<ToDoListTaskViewModel>>(tasksDueToday),
                };

                var todoLists = await _todoListService.GetAllAsync(user.Id);

                var model = new CombinedTodoListViewModel()
                {
                    TodoLists = _mapper.Map<List<ToDoListViewModel>>(todoLists),
                    SelectedTodoList = todoListViewModel,
                    IsDueToday = true
                };

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(GetTasksDueToday));
                return RedirectToAction("Error", "Home", new { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost("/CreateTask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(ToDoListTaskViewModel task)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var taskToAdd = _mapper.Map<ToDoListTask>(task);
                await _todoListTaskService.AddAsync(taskToAdd);
                _logger.LogInformation("User {UserId} attached new Task with Id {TodoListTaskId} to Todo List {TodoListId}", user.Id, taskToAdd.Id, taskToAdd.ToDoListId);

                ModelState.Clear();
                return await TodoListTasks(taskToAdd.ToDoListId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(CreateTask));
                var errorUrl = Url.Action("Error", "Home", new { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                return BadRequest(errorUrl);
            }   
        }

        [HttpPost("/DeleteTask/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int id, int todoListId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                await _todoListTaskService.DeleteAsync(id, user.Id);
                _logger.LogInformation("User {UserId} deleted Task with Id {TodoListTaskId}", user.Id, id);

                if (Request.Headers["Referer"].ToString().Contains("TasksDueToday"))
                {
                    return await TasksDueToday();
                }
                else
                {
                    return await TodoListTasks(todoListId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(DeleteTask));
                var errorUrl = Url.Action("Error", "Home", new { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                return BadRequest(errorUrl);
            }
        }

        [HttpPost("/UpdateTask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTask(ToDoListTaskViewModel task)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var taskToUpdate = _mapper.Map<ToDoListTask>(task);

                await _todoListTaskService.UpdateAsync(taskToUpdate, user.Id);
                _logger.LogInformation("User {UserId} updated Task with Id {TodoListTaskId}", user.Id, taskToUpdate.Id);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(UpdateTask));
                var errorUrl = Url.Action("Error", "Home", new { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                return BadRequest(errorUrl);
            }
        }

        [HttpPost("/UpdateTaskStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTaskStatus(int id, int todoListId, TodoStatus newStatus)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                await _todoListTaskService.UpdateStatusAsync(id, user.Id, newStatus);
                _logger.LogInformation("User {UserId} updated status of Task with Id {TodoListTaskId}", user.Id, id);

                ModelState.Clear();

                if (Request.Headers["Referer"].ToString().Contains("TasksDueToday"))
                {
                    return await TasksDueToday();
                }
                else
                {
                    return await TodoListTasks(todoListId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the {ActionName} action", nameof(UpdateTaskStatus));
                var errorUrl = Url.Action("Error", "Home", new { requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                return BadRequest(errorUrl);
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
