using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoList.Domain.BLL.Interfaces;
using TodoList.Domain.DAL.Entities;

namespace TodoList.MVC.Controllers
{
    [Authorize]
    public class ToDoListController : Controller
    {
        private readonly IToDoListService _todoListService;
        private readonly IToDoListTaskService _todoListTaskService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ToDoListController(IToDoListService todoListService, IToDoListTaskService todoListTaskService, UserManager<ApplicationUser> userManager)
        {
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

                var tasks = await _todoListService.GetAllAsync(userId);

                return View(tasks);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
    }
}
