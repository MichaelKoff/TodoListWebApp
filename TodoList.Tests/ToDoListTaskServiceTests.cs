using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Domain.BLL.Services;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Repositories;
using TodoList.Domain;
using TodoList.Domain.DAL.Enums;
using TodoList.Domain.DAL.Interfaces;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using TodoList.Domain.Constants;
using Microsoft.VisualBasic;

namespace TodoList.Tests
{
    [TestFixture]
    public class ToDoListTaskServiceTests
    {
        private ToDoListTaskService _sut;
        private ApplicationDbContext _context;
        private User _user;

        [SetUp]
        public void Setup()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var options = optionsBuilder
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

            this._context = new ApplicationDbContext(options);
            this._user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "TestUser",
                Email = "test.user@example.com"
            };

            _context.Users.Add(this._user);
            _context.SaveChanges();

            ToDoListTaskRepository taskRepo = new ToDoListTaskRepository(this._context);

            this._sut = new ToDoListTaskService(taskRepo);
        }

        [TearDown]
        public void DeleteDatabase()
        {
            this._context.Database.EnsureDeleted();
            this._context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddNewTaskWithSetCreationDateAndStatus_WhenInputIsValid()
        {
            // Arrange

            var task = new ToDoListTask
            {
                Title = "Test Task",
                ToDoListId = 1
            };

            // Act
            await _sut.AddAsync(task);

            // Assert
            var addedTask = await _context.ToDoListTasks.FindAsync(task.Id);
            addedTask.Should().NotBeNull();
            addedTask.Should().BeEquivalentTo(task, options => options
                .Excluding(t => t.CreationDate)
                .Excluding(t => t.Status));

            addedTask.Status.Should().Be(TodoStatus.NotStarted);
            addedTask.CreationDate.Should().BeCloseTo(DateTime.Now, precision: TimeSpan.FromSeconds(1));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task AddAsync_ShouldThrowArgumentException_WhenToDoListIdIsUnsetOrInvalid(int todoListId)
        {
            // Arrange
            var task = new ToDoListTask
            {
                Title = "Test Task",
                ToDoListId = todoListId
            };

            // Act & Assert
            Func<Task> act = async () => await _sut.AddAsync(task);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Unset or invalid Todo list id");
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task AddAsync_ShouldThrowArgumentException_WhenTitleIsNullOrEmpty(string title)
        {
            // Arrange
            var task = new ToDoListTask
            {
                Title = title,
                ToDoListId = 1
            };

            // Act & Assert
            Func<Task> act = async () => await _sut.AddAsync(task);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Title cannot be empty or whitespace.");
        }

        [Test]
        public async Task AddAsync_ShouldThrowArgumentException_WhenTitleIsTooLong()
        {
            // Arrange
            var tooLongTitle = new string('A', AppConstants.MaxToDoListTaskTitleLength + 1);
            var task = new ToDoListTask
            {
                Title = tooLongTitle,
                ToDoListId = 1
            };

            // Act & Assert
            Func<Task> act = async () => await _sut.AddAsync(task);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage($"Title cannot be longer than {AppConstants.MaxToDoListTaskTitleLength} characters");
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnToDoListTask_WhenIdAndUserIdAreValid()
        {
            // Arrange
            var todoList = new ToDoList
            {
                Title = "Test List",
                User = _user,
                UserId = _user.Id
            };
            var task = new ToDoListTask
            {
                ToDoListId = 1,
                Title = "Test Task",
                Description = "Test Description",
                Status = TodoStatus.NotStarted,
                CreationDate = DateTime.Today,
                DueDate = DateTime.Today,
            };

            _context.ToDoLists.Add(todoList);
            _context.ToDoListTasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            var fetchedTask = await _sut.GetByIdAsync(task.Id, _user.Id);

            // Assert
            fetchedTask.Should().NotBeNull();
            fetchedTask.Should().BeEquivalentTo(task);
            fetchedTask.ToDoList.Should().BeSameAs(todoList);
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task GetByIdAsync_ShouldThrowArgumentNullException_WhenUserIdIsNullOrEmpty(string userId)
        {
            // Arrange
            int id = 1;

            // Act
            Func<Task> act = async () => await _sut.GetByIdAsync(id, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("User Id cannot be null or empty (Parameter 'userId')");
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllToDoListTasksForUser()
        {
            // Arrange
            var list1 = new ToDoList
            {
                Title = "Test List 1",
                User = _user,
                UserId = _user.Id,
                ToDoListTasks = new List<ToDoListTask>
                {
                    new ToDoListTask
                    {
                        Title = "Test Task 1",
                        Description = "Test Description",
                        Status = TodoStatus.NotStarted,
                        CreationDate = DateTime.Today,
                        DueDate = DateTime.Today,
                    },

                    new ToDoListTask
                    {
                        Title = "Test Task 2",
                        Description = "Test Description",
                        Status = TodoStatus.InProgress,
                        CreationDate = DateTime.Today,
                        DueDate = DateTime.Today,
                    },

                    new ToDoListTask
                    {
                        Title = "Test Task 3",
                        Description = "Test Description",
                        Status = TodoStatus.Completed,
                        CreationDate = DateTime.Today,
                        DueDate = DateTime.Today,
                    },
                }
            };
            var list2 = new ToDoList
            {
                Title = "Test List 2",
                User = _user,
                UserId = _user.Id,
                ToDoListTasks = new List<ToDoListTask>
                {
                    new ToDoListTask
                    {
                        Title = "Test Task 4",
                        Description = "Test Description",
                        Status = TodoStatus.NotStarted,
                        CreationDate = DateTime.Today,
                        DueDate = DateTime.Today,
                    },

                    new ToDoListTask
                    {
                        Title = "Test Task 5",
                        Description = "Test Description",
                        Status = TodoStatus.InProgress,
                        CreationDate = DateTime.Today,
                        DueDate = DateTime.Today,
                    },

                    new ToDoListTask
                    {
                        Title = "Test Task 6",
                        Description = "Test Description",
                        Status = TodoStatus.Completed,
                        CreationDate = DateTime.Today,
                        DueDate = DateTime.Today,
                    },
                }
            };
            _context.ToDoLists.AddRange(list1, list2);
            await _context.SaveChangesAsync();

            // Act
            var allUserTasks = await _sut.GetAllAsync(_user.Id);

            // Assert
            allUserTasks.Should().HaveCount(6);
            allUserTasks.Should().BeEquivalentTo(list1.ToDoListTasks.Concat(list2.ToDoListTasks));
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task GetAllAsync_ShouldThrowArgumentNullException_WhenUserIdIsNullOrEmpty(string userId)
        {
            // Arrange

            // Act
            Func<Task> act = async () => await _sut.GetAllAsync(userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("User Id cannot be null or empty (Parameter 'userId')");
        }

        [Test]
        public async Task GetTasksDueTodayAsync_ShouldReturnAllToDoListTasksDueTodayForUser()
        {
            // Arrange
            var list1 = new ToDoList
            {
                Title = "Test List 1",
                User = _user,
                UserId = _user.Id,
                ToDoListTasks = new List<ToDoListTask>
                {
                    new ToDoListTask
                    {
                        Title = "Test Task 1",
                        Description = "Test Description",
                        Status = TodoStatus.NotStarted,
                        CreationDate = DateTime.Today,
                        DueDate = DateTime.Today,
                    },

                    new ToDoListTask
                    {
                        Title = "Test Task 2",
                        Description = "Test Description",
                        Status = TodoStatus.InProgress,
                        CreationDate = DateTime.Today,
                        DueDate = null,
                    },

                    new ToDoListTask
                    {
                        Title = "Test Task 3",
                        Description = "Test Description",
                        Status = TodoStatus.Completed,
                        CreationDate = DateTime.Today,
                        DueDate = null,
                    },
                }
            };
            var list2 = new ToDoList
            {
                Title = "Test List 2",
                User = _user,
                UserId = _user.Id,
                ToDoListTasks = new List<ToDoListTask>
                {
                    new ToDoListTask
                    {
                        Title = "Test Task 4",
                        Description = "Test Description",
                        Status = TodoStatus.NotStarted,
                        CreationDate = DateTime.Today,
                        DueDate = null,
                    },

                    new ToDoListTask
                    {
                        Title = "Test Task 5",
                        Description = "Test Description",
                        Status = TodoStatus.InProgress,
                        CreationDate = DateTime.Today,
                        DueDate = DateTime.Today,
                    },

                    new ToDoListTask
                    {
                        Title = "Test Task 6",
                        Description = "Test Description",
                        Status = TodoStatus.Completed,
                        CreationDate = DateTime.Today,
                        DueDate = DateTime.Today,
                    },
                }
            };
            _context.ToDoLists.AddRange(list1, list2);
            await _context.SaveChangesAsync();

            // Act
            var tasksDueToday = await _sut.GetTasksDueTodayAsync(_user.Id);

            // Assert
            tasksDueToday.Should().HaveCount(3);
            tasksDueToday.Should().OnlyContain(x => x.DueDate == DateTime.Today);
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task GetTasksDueTodayAsync_ShouldThrowArgumentNullException_WhenUserIdIsNullOrEmpty(string userId)
        {
            // Arrange

            // Act
            Func<Task> act = async () => await _sut.GetTasksDueTodayAsync(userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("User Id cannot be null or empty (Parameter 'userId')");
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateToDoListTask_WhenModelAndUserIdAreValid()
        {
            // Arrange
            var taskToUpdate = new ToDoListTask
            {
                Title = "Test Task 1",
                Status = TodoStatus.NotStarted,
                CreationDate = DateTime.Today,
                DueDate = DateTime.Today,
            };
            var list = new ToDoList
            {
                Title = "Test List 1",
                User = _user,
                UserId = _user.Id,
                ToDoListTasks = new List<ToDoListTask>(),
            };

            list.ToDoListTasks.Add(taskToUpdate);

            _context.ToDoLists.Add(list);
            await _context.SaveChangesAsync();

            var updateTaskModel = new ToDoListTask()
            {
                Id = taskToUpdate.Id,
                Title = "Test task 2",
                Description = "Test description",
                Status = TodoStatus.Completed,
                DueDate = DateTime.Today.AddDays(1),
                Reminder = DateTime.Today,
                ToDoListId = 111,
                ToDoList = new ToDoList()
                {
                    Id = 111,
                    Title = "Some list",
                }
            };

            // Act
            await _sut.UpdateAsync(updateTaskModel, _user.Id);

            // Assert
            var updatedTask = await _context.ToDoListTasks.FindAsync(taskToUpdate.Id);
            updatedTask.Should().BeEquivalentTo(updateTaskModel, options => options
                .Excluding(t => t.ToDoList)
                .Excluding(t => t.ToDoListId)
                .Excluding(t => t.CreationDate));

            updatedTask.ToDoList.Should().BeSameAs(taskToUpdate.ToDoList);
            updatedTask.ToDoListId.Should().Be(taskToUpdate.ToDoListId);
            updatedTask.CreationDate.Should().Be(taskToUpdate.CreationDate);
        }


        [TestCase("")]
        [TestCase(null)]
        public async Task UpdateAsync_ShouldThrowArgumentNullException_WhenUserIdIsNullOrEmpty(string userId)
        {
            // Arrange
            var task = new ToDoListTask();

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(task, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("User Id cannot be null or empty (Parameter 'userId')");
        }

        [TestCase(null)]
        [TestCase("  ")]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenTaskTitleIsNullOrWhitespace(string title)
        {
            // Arrange
            var task = new ToDoListTask()
            {
                Title = title,
            };

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(task, _user.Id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Title cannot be empty or whitespace.");
        }

        [Test]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenTaskTitleIsTooLong()
        {
            // Arrange
            var tooLongTitle = new string('A', AppConstants.MaxToDoListTaskTitleLength + 1);
            var task = new ToDoListTask()
            {
                Title = tooLongTitle,
            };

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(task, _user.Id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage($"Title cannot be longer than {AppConstants.MaxToDoListTaskTitleLength} characters");
        }

        [Test]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenTaskDescriptionIsTooLong()
        {
            // Arrange
            var tooLongDescription = new string('A', AppConstants.MaxToDoListTaskDescriptionLength + 1);
            var task = new ToDoListTask()
            {
                Title = "Test title",
                Description = tooLongDescription
            };

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(task, _user.Id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage($"Description cannot be longer than {AppConstants.MaxToDoListTaskDescriptionLength} characters");
        }

        [Test]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenTaskDoesNotExist()
        {
            // Arrange
            var task = new ToDoListTask()
            {
                Id = 999,
                Title = "Test title",
            };

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(task, _user.Id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage($"Task with the specified ID \"{task.Id}\" does not exist.");
        }

        [Test]
        public async Task UpdateStatusAsync_ShouldOnlyUpdateTaskStatus()
        {
            // Arrange
            var newStatus = TodoStatus.Completed;
            var taskToUpdate = new ToDoListTask
            {
                Title = "Test Task 1",
                Status = TodoStatus.NotStarted,
                CreationDate = DateTime.Today,
                DueDate = DateTime.Today,
            };
            var list = new ToDoList
            {
                Title = "Test List 1",
                User = _user,
                UserId = _user.Id,
                ToDoListTasks = new List<ToDoListTask>(),
            };

            list.ToDoListTasks.Add(taskToUpdate);

            _context.ToDoLists.Add(list);
            await _context.SaveChangesAsync();

            // Act
            await _sut.UpdateStatusAsync(taskToUpdate.Id, _user.Id, newStatus);

            // Assert
            var updatedTask = await _context.ToDoListTasks.FindAsync(taskToUpdate.Id);
            updatedTask.Should().BeEquivalentTo(taskToUpdate, options => options.Excluding(t => t.Status));
            updatedTask.Status.Should().Be(newStatus);
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task UpdateStatusAsync_ShouldThrowArgumentNullException_WhenUserIdIsNullOrEmpty(string userId)
        {
            // Arrange
            int id = 1;
            var status = TodoStatus.Completed;

            // Act
            Func<Task> act = async () => await _sut.UpdateStatusAsync(id, userId, status);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("User Id cannot be null or empty (Parameter 'userId')");
        }

        [Test]
        public async Task UpdateStatusAsync_ShouldThrowArgumentException_WhenTaskDoesNotExist()
        {
            // Arrange
            int id = 1;
            var status = TodoStatus.Completed;

            // Act
            Func<Task> act = async () => await _sut.UpdateStatusAsync(id, _user.Id, status);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage($"Task with the specified ID \"{id}\" does not exist. (Parameter 'id')");
        }

        [Test]
        public async Task DeleteAsync_ShouldDeleteTask_WhenIdAndUserIdAreValid()
        {
            // Arrange
            var list = new ToDoList
            {
                Title = "Test List 1",
                User = _user,
                UserId = _user.Id,
                ToDoListTasks = new List<ToDoListTask>(),
            };

            var task = new ToDoListTask
            {
                Title = "Test Task 1",
            };

            list.ToDoListTasks.Add(task);

            _context.ToDoLists.Add(list);
            await _context.SaveChangesAsync();

            // Act
            await _sut.DeleteAsync(task.Id, _user.Id);

            // Assert
            var fetchedTask = await _context.ToDoListTasks.FindAsync(task.Id);
            fetchedTask.Should().BeNull();
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task DeleteAsync_ShouldThrowArgumentNullException_WhenUserIdIsNullEmpty(string userId)
        {
            // Arrange
            var id = 1;

            // Act
            Func<Task> act = async () => await _sut.DeleteAsync(id, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("User Id cannot be null or empty (Parameter 'userId')");
        }

        [Test]
        public async Task DeleteAsync_ShouldThrowArgumentException_WhenToDoListDoesNotExist()
        {
            // Arrange
            var id = 1;

            // Act
            Func<Task> act = async () => await _sut.DeleteAsync(id, _user.Id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Task with the specified ID \"{id}\" does not exist. (Parameter 'id')");
        }
    }
}
