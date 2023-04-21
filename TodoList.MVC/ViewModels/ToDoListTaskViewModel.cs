using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using TodoList.Domain.Constants;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Enums;

namespace TodoList.MVC.ViewModels
{
    public class ToDoListTaskViewModel : BaseEntity
    {
        private string _title;
        public int ToDoListId { get; set; }

        [Required]
        [MaxLength(AppConstants.MaxToDoListTaskTitleLength)]
        [RegularExpression(@"^\s*\S+(?:\s+\S+)*\s*$", ErrorMessage = "Title cannot be empty or whitespace.")]
        public string Title
        {
            get => _title;
            set => _title = Regex.Replace(value.Trim(), @"\s+", " ");
        }

        [MaxLength(AppConstants.MaxToDoListTaskDescriptionLength)]
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TodoStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? Reminder { get; set; }
    }
}
