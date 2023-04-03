using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Enums;

namespace TodoList.MVC.ViewModels
{
    public class ToDoListTaskViewModel : BaseEntity
    {
        private string _title;
        public int ToDoListId { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^\s*\S+(?:\s+\S+)*\s*$", ErrorMessage = "Title cannot be empty or whitespace.")]
        public string Title
        {
            get => _title;
            set => _title = Regex.Replace(value.Trim(), @"\s+", " ");
        }

        [MaxLength(255)]
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TodoStatus Status { get; set; } = TodoStatus.NotStarted;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime? Reminder { get; set; }
    }
}
