using System.Collections.Generic;
using TodoList.Domain.DAL.Entities;

namespace TodoList.MVC.ViewModels
{
    public class CombinedTodoListViewModel
    {
        public IEnumerable<ToDoListViewModel> TodoLists { get; set; }
        public ToDoListViewModel SelectedTodoList { get; set; }
        public bool IsDueToday { get; set; } = false;
        public bool IsListNotFound { get; set; } = false;
    }
}
