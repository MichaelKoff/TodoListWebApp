using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using TodoList.Domain.Constants;
using TodoList.Domain.DAL.Entities;

namespace TodoList.MVC.ViewModels
{
    public class ToDoListViewModel : BaseEntity
    {
        private string _title;

        [Required]
        [MaxLength(AppConstants.MaxToDoListTitleLength)]
        [RegularExpression(@"^\s*\S+(?:\s+\S+)*\s*$", ErrorMessage = "Title cannot be empty or whitespace.")]
        public string Title 
        {
            get => _title;
            set => _title = Regex.Replace(value.Trim(), @"\s+", " ");
        }

        public List<ToDoListTaskViewModel> ToDoListTasks { get; set; }

    }
}
