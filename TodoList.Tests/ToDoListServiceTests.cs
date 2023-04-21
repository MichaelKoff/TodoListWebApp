using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoList.Domain;
using TodoList.Domain.BLL.Interfaces;
using TodoList.Domain.BLL.Services;
using TodoList.Domain.Constants;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Enums;
using TodoList.Domain.DAL.Interfaces;
using TodoList.Domain.DAL.Repositories;

namespace TodoList.Tests
{
    [TestFixture]
    public class ToDoListServiceTests
    {
        private ToDoListService _sut;
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

            ToDoListRepository todoListRepo = new ToDoListRepository(this._context);

            this._sut = new ToDoListService(todoListRepo);
        }

        [TearDown]
        public void DeleteDatabase()
        {
            this._context.Database.EnsureDeleted();
            this._context.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddTodoList_WhenToDoListIsValid()
        {
            // Arrange
            var listToAdd = new ToDoList
            {
                Title = "Test List",
                UserId = _user.Id,
                User = _user
            };

            // Act
            await _sut.AddAsync(listToAdd);

            // Assert
            var addedList = await _context.ToDoLists.FindAsync(listToAdd.Id);
            addedList.Should().NotBeNull();
            addedList.Should().BeEquivalentTo(listToAdd);

        }

        [TestCase("Test List")]
        public async Task AddAsync_ShouldAddTodoListWithUniqueTitle_WhenTitleIsDuplicate(string title)
        {
            // Arrange
            var todoList1 = new ToDoList
            {
                Title = title,
                UserId = _user.Id,
                User = _user
            };
            var todoList2 = new ToDoList
            {
                Title = title,
                UserId = _user.Id,
                User = _user
            };

            // Act
            await _sut.AddAsync(todoList1);
            await _sut.AddAsync(todoList2);

            // Assert
            var addedList = await _context.ToDoLists.FindAsync(todoList2.Id);
            addedList.Should().NotBeNull();
            addedList.Should().BeEquivalentTo(todoList2, options => options.Excluding(x => x.Title));
            addedList.Title.Should().Be($"{title} (1)");
        }

        [Test]
        public async Task AddAsync_ShouldAddTodoListWithUniqueTitle_WhenTitleIsDuplicateAndMaxLength()
        {
            // Arrange
            var maxLengthTitle = new string('A', AppConstants.MaxToDoListTitleLength);
            var cutLength = AppConstants.MaxToDoListTitleLength - " (1)".Length;
            var expectedTitle = maxLengthTitle[..cutLength] + " (1)";

            var todoList1 = new ToDoList
            {
                Title = maxLengthTitle,
                UserId = _user.Id,
                User = _user
            };
            var todoList2 = new ToDoList
            {
                Title = maxLengthTitle,
                UserId = _user.Id,
                User = _user
            };

            // Act
            await _sut.AddAsync(todoList1);
            await _sut.AddAsync(todoList2);

            // Assert
            var addedList = await _context.ToDoLists.FindAsync(todoList2.Id);
            addedList.Should().BeEquivalentTo(todoList2, options => options.Excluding(x => x.Title).Excluding(x => x.ToDoListTasks));
            addedList.Title.Should().Be(expectedTitle);
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task AddAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty(string userId)
        {
            // Arrange
            var todoList = new ToDoList
            {
                Title = "Test List",
                UserId = userId,
                User = _user
            };

            // Act and Assert
            Func<Task> act = async () => await _sut.AddAsync(todoList);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("User Id cannot be null or empty.");
        }

        [Test]
        public async Task AddAsync_ShouldThrowArgumentException_WhenUserIsNull()
        {
            // Arrange
            var todoList = new ToDoList
            {
                Title = "Test List",
                UserId = _user.Id,
                User = null
            };

            // Act and Assert
            Func<Task> act = async () => await _sut.AddAsync(todoList);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("User cannot be null.");
        }

        [TestCase(null)]
        [TestCase("        ")]
        public async Task AddAsync_ShouldThrowArgumentException_WhenTitleIsNullOrWhitespace(string title)
        {
            // Arrange
            var todoList = new ToDoList
            {
                Title = title,
                UserId = _user.Id,
                User = _user
            };

            // Act and Assert
            Func<Task> act = async () => await _sut.AddAsync(todoList);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Title cannot be empty or whitespace.");
        }

        [Test]
        public async Task AddAsync_ShouldThrowArgumentException_WhenTitleIsTooLong()
        {
            // Arrange
            var tooLongTitle = new string('A', AppConstants.MaxToDoListTitleLength + 1);
            var todoList = new ToDoList
            {
                Title = tooLongTitle,
                UserId = _user.Id,
                User = _user
            };

            // Act and Assert
            Func<Task> act = async () => await _sut.AddAsync(todoList);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage($"Title cannot be longer than {AppConstants.MaxToDoListTitleLength} characters");
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnToDoListWithToDoListTasks_WhenIdAndUserIdAreValid()
        {
            // Arrange
            var todoList = new ToDoList
            {
                Title = "Test List",
                UserId = _user.Id,
                User = _user,
                ToDoListTasks = new List<ToDoListTask>()
            };

            var task = new ToDoListTask
            {
                Title = "Task 1",
                CreationDate = DateTime.Now,
                Status = TodoStatus.NotStarted,
            };

            todoList.ToDoListTasks.Add(task);
            await _sut.AddAsync(todoList);

            // Act
            var fetchedTodoList = await _sut.GetByIdAsync(todoList.Id, _user.Id);

            // Assert
            fetchedTodoList.Should().BeEquivalentTo(todoList);
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task GetByIdAsync_ShouldThrowArgumentNullException_WhenUserIdIsNull(string userId)
        {
            // Arrange
            int id = 1;

            // Act
            Func<Task> act = async () => await _sut.GetByIdAsync(id, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("User Id cannot be null or empty (Parameter 'userId')");
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllToDoListsWithTasksForUser()
        {
            // Arrange
            var todoList1 = new ToDoList { Title = "List 1", UserId = _user.Id, User = _user, ToDoListTasks = new List<ToDoListTask>() { new ToDoListTask() { Title = "Task of List 1", CreationDate = DateTime.Now, Status = TodoStatus.NotStarted } } };
            var todoList2 = new ToDoList { Title = "List 2", UserId = _user.Id, User = _user, ToDoListTasks = new List<ToDoListTask>() { new ToDoListTask() { Title = "Task of List 2", CreationDate = DateTime.Now, Status = TodoStatus.NotStarted } } };
            var todoList3 = new ToDoList { Title = "List 3", UserId = _user.Id, User = _user, ToDoListTasks = new List<ToDoListTask>() { new ToDoListTask() { Title = "Task of List 3", CreationDate = DateTime.Now, Status = TodoStatus.NotStarted } } };
            await _sut.AddAsync(todoList1);
            await _sut.AddAsync(todoList2);
            await _sut.AddAsync(todoList3);

            // Act
            var fetchedTodoLists = await _sut.GetAllAsync(_user.Id);

            // Assert
            fetchedTodoLists.Should().NotBeNull();
            fetchedTodoLists.Count.Should().Be(3);
            fetchedTodoLists.Should().ContainEquivalentOf(todoList1);
            fetchedTodoLists.Should().ContainEquivalentOf(todoList2);
            fetchedTodoLists.Should().ContainEquivalentOf(todoList3);
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
        public async Task UpdateAsync_ShouldUpdateOnlyToDoListTitle_WhenTitleIsChanged()
        {
            // Arrange
            var todoList = new ToDoList
            {
                Title = "Test List",
                UserId = _user.Id,
                User = _user,
                ToDoListTasks = new List<ToDoListTask>() { new ToDoListTask() { Title = "Task of List 1", CreationDate = DateTime.Now, Status = TodoStatus.NotStarted } }
            };
            await _sut.AddAsync(todoList);

            var updatedTodoList = new ToDoList
            {
                Id = todoList.Id,
                Title = "Updated Test List",
                UserId = _user.Id,
                User = _user
            };

            // Act
            await _sut.UpdateAsync(updatedTodoList);

            // Assert
            var fetchedTodoList = await _sut.GetByIdAsync(todoList.Id, _user.Id);
            fetchedTodoList.Should().BeEquivalentTo(todoList, options => options.Excluding(list => list.Title));
            fetchedTodoList.Title.Should().Be(updatedTodoList.Title);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateTitleToUniqueTitle_WhenTitleAlreadyExists()
        {
            // Arrange
            var todoList1 = new ToDoList
            {
                Title = "Test List",
                UserId = _user.Id,
                User = _user
            };
            var todoList2 = new ToDoList
            {
                Title = "Another Test List",
                UserId = _user.Id,
                User = _user
            };

            await _sut.AddAsync(todoList1);
            await _sut.AddAsync(todoList2);

            var updatedTodoList = new ToDoList
            {
                Id = todoList2.Id,
                Title = "Test List",
                UserId = _user.Id,
                User = _user
            };

            // Act
            await _sut.UpdateAsync(updatedTodoList);

            // Assert
            var fetchedTodoList = await _sut.GetByIdAsync(todoList2.Id, _user.Id);
            fetchedTodoList.Should().BeEquivalentTo(todoList2, options => options.Excluding(list => list.Title));
            fetchedTodoList.Title.Should().Be($"{updatedTodoList.Title} (1)");
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateTitleToUniqueTitle_WhenMaxLengthTitleAlreadyExists()
        {
            // Arrange
            var maxLengthTitle = new string('A', AppConstants.MaxToDoListTitleLength);
            var cutLength = AppConstants.MaxToDoListTitleLength - " (1)".Length;
            var expectedTitle = maxLengthTitle[..cutLength] + " (1)";

            var todoList1 = new ToDoList
            {
                Title = maxLengthTitle,
                UserId = _user.Id,
                User = _user
            };
            var todoList2 = new ToDoList
            {
                Title = "Another Test List",
                UserId = _user.Id,
                User = _user
            };

            await _sut.AddAsync(todoList1);
            await _sut.AddAsync(todoList2);

            var updatedTodoList = new ToDoList
            {
                Id = todoList2.Id,
                Title = maxLengthTitle,
                UserId = _user.Id,
                User = _user
            };

            // Act
            await _sut.UpdateAsync(updatedTodoList);

            // Assert
            var fetchedTodoList = await _sut.GetByIdAsync(todoList2.Id, _user.Id);
            fetchedTodoList.Should().BeEquivalentTo(todoList2, options => options.Excluding(list => list.Title));
            fetchedTodoList.Title.Should().Be(expectedTitle);
        }

        [Test]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenTodoListDoesNotExist()
        {
            // Arrange
            var nonExistentTodoList = new ToDoList
            {
                Id = 999,
                Title = "Non-existent List",
                UserId = _user.Id,
                User = _user
            };

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(nonExistentTodoList);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Todo list with the specified ID \"{nonExistentTodoList.Id}\" and user ID \"{nonExistentTodoList.UserId}\" does not exist.");
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty(string userId)
        {
            // Arrange
            var todoList = new ToDoList
            {
                Id = 999,
                Title = "Non-existent List",
                UserId = userId,
                User = _user
            };

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(todoList);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("User Id cannot be null or empty.");
        }

        [Test]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenUserIsNull()
        {
            // Arrange
            var todoList = new ToDoList
            {
                Id = 999,
                Title = "Non-existent List",
                UserId = _user.Id,
            };

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(todoList);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("User cannot be null.");
        }

        [TestCase(null)]
        [TestCase("        ")]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenTitleIsNullOrWhitespace(string title)
        {
            // Arrange
            var todoList = new ToDoList
            {
                Title = title,
                UserId = _user.Id,
                User = _user
            };

            // Act and Assert
            Func<Task> act = async () => await _sut.UpdateAsync(todoList);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Title cannot be empty or whitespace.");
        }

        [Test]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenTitleIsTooLong()
        {
            // Arrange
            var tooLongTitle = new string('A', AppConstants.MaxToDoListTitleLength + 1);
            var todoList = new ToDoList
            {
                Title = tooLongTitle,
                UserId = _user.Id,
                User = _user
            };

            // Act and Assert
            Func<Task> act = async () => await _sut.UpdateAsync(todoList);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Title cannot be longer than {AppConstants.MaxToDoListTitleLength} characters");
        }

        [Test]
        public async Task DeleteAsync_ShouldDeleteToDoList_WhenIdAndUserIdAreValid()
        {
            // Arrange
            var todoList = new ToDoList
            {
                Title = "Test List",
                UserId = _user.Id,
                User = _user
            };
            await _sut.AddAsync(todoList);

            // Act
            await _sut.DeleteAsync(todoList.Id, _user.Id);

            // Assert
            var fetchedTodoList = await _sut.GetByIdAsync(todoList.Id, _user.Id);
            fetchedTodoList.Should().BeNull();
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
            var id = 999;

            // Act
            Func<Task> act = async () => await _sut.DeleteAsync(id, _user.Id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Todo list with the specified ID \"{id}\" and user ID \"{_user.Id}\" does not exist. (Parameter 'id')");
        }

        [Test]
        public async Task DuplicateAsync_ShouldDuplicateToDoListWithUniqueTitle_WhenIdAndUserIdAreValid()
        {
            // Arrange
            var todoList = new ToDoList
            {
                Title = "Test List",
                UserId = _user.Id,
                User = _user,
            };
            await _sut.AddAsync(todoList);

            // Act
            await _sut.DuplicateAsync(todoList.Id, _user.Id);

            // Assert
            var allTodoLists = await _sut.GetAllAsync(_user.Id);
            allTodoLists.Should().HaveCount(2);
            allTodoLists[1].User.Should().Be(todoList.User);
            allTodoLists[1].UserId.Should().Be(todoList.UserId);
            allTodoLists[1].Title.Should().Be($"{todoList.Title} (1)");
        }

        [Test]
        public async Task DuplicateAsync_ShouldDuplicateToDoListWithUniqueTitle_WhenToDoListTitleHasMaxLength()
        {
            // Arrange
            var maxLengthTitle = new string('A', AppConstants.MaxToDoListTitleLength);
            var cutLength = AppConstants.MaxToDoListTitleLength - " (1)".Length;
            var expectedTitle = maxLengthTitle[..cutLength] + " (1)";

            var todoList = new ToDoList
            {
                Title = maxLengthTitle,
                UserId = _user.Id,
                User = _user,
            };
            await _sut.AddAsync(todoList);

            // Act
            await _sut.DuplicateAsync(todoList.Id, _user.Id);

            // Assert
            var allTodoLists = await _sut.GetAllAsync(_user.Id);
            allTodoLists.Should().HaveCount(2);
            allTodoLists[1].User.Should().Be(todoList.User);
            allTodoLists[1].UserId.Should().Be(todoList.UserId);
            allTodoLists[1].Title.Should().Be(expectedTitle);
        }

        [Test]
        public async Task DuplicateAsync_ShouldDuplicateToDoListTasksWithNewCreationDate_WhenIdAndUserIdAreValid()
        {
            // Arrange
            var todoList = new ToDoList
            {
                Title = "Test List",
                UserId = _user.Id,
                User = _user,
                ToDoListTasks = new List<ToDoListTask>
                {
                    new ToDoListTask { Title = "Task 1", Description = "Description 1", Status = TodoStatus.NotStarted },
                    new ToDoListTask { Title = "Task 2", Description = "Description 2", Status = TodoStatus.Completed }
                }
            };
            await _sut.AddAsync(todoList);

            // Act
            await _sut.DuplicateAsync(todoList.Id, _user.Id);

            // Assert
            var allTodoLists = await _sut.GetAllAsync(_user.Id);
            allTodoLists.Should().HaveCount(2);
            allTodoLists[1].User.Should().Be(todoList.User);
            allTodoLists[1].UserId.Should().Be(todoList.UserId);
            allTodoLists[1].Title.Should().Be($"{todoList.Title} (1)");

            allTodoLists[1].ToDoListTasks.Should().BeEquivalentTo(allTodoLists[0].ToDoListTasks, options => options
                .Excluding(task => task.Id)
                .Excluding(task => task.ToDoList)
                .Excluding(task => task.ToDoListId)
                .Excluding(task => task.CreationDate));

            allTodoLists[1].ToDoListTasks.Should().AllSatisfy(t => t.CreationDate.Should().BeCloseTo(DateTime.Now, precision: TimeSpan.FromSeconds(1)));
        }

        [TestCase("")]
        [TestCase(null)]
        public async Task DuplicateAsync_ShouldThrowArgumentNullException_WhenUserIdIsNullOrEmpty(string userId)
        {
            // Arrange
            var id = 1;

            // Act
            Func<Task> act = async () => await _sut.DuplicateAsync(id, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("User Id cannot be null or empty (Parameter 'userId')");
        }

        [Test]
        public async Task DuplicateAsync_ShouldThrowArgumentException_WhenToDoListDoesNotExist()
        {
            // Arrange
            var id = 999;

            // Act
            Func<Task> act = async () => await _sut.DuplicateAsync(id, _user.Id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Todo list with the specified ID \"{id}\" and user ID \"{_user.Id}\" does not exist. (Parameter 'id')");
        }
    }
}