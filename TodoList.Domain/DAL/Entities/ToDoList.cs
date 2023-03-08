﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoList.Domain.DAL.Entities
{
    public class ToDoList : BaseEntity
    {
        public string Title { get; set; }

        public ICollection<ToDoListTask> ToDoListTasks { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
