using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Domain.DAL.Enums;

namespace TodoList.Domain.DAL.Entities
{
    public class ToDoListTask : BaseEntity
    {
        public int ToDoListId { get; set; }
        public ToDoList ToDoList { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TodoStatus Status { get; set; } = TodoStatus.NotStarted;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime? Reminder { get; set; }
    }
}
