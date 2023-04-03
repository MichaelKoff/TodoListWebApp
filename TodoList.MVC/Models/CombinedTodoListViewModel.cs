using TodoList.Domain.DAL.Entities;
using TodoList.MVC.ViewModels;

namespace TodoList.MVC.Models
{
    public class CombinedTodoListViewModel
    {
        public IEnumerable<ToDoListViewModel> TodoLists { get; set; }
        public ToDoListViewModel SelectedTodoList { get; set; }
    }
}
