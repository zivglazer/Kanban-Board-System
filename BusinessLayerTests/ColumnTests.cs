using NUnit.Framework;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using System;
using Task = IntroSE.Kanban.Backend.BusinessLayer.Task;

namespace BusinessLayerTests
{
    [TestFixture]
    public class ColumnTests
    {
        private Column _column;
        private const int BoardId = 1;
        private const int ColumnIndex = 0;

        private BoardController _boardController;
        private ColumnController _columnController;
        private TaskController _taskController;

        private Task CreateTask(int taskId)
        {
            return new Task(BoardId, taskId, DateTime.Now, DateTime.Now.AddDays(1), $"Task {taskId}", "Description");
        }

        [SetUp]
        public void SetUp()
        {
            // Clean persistent database tables before each test
            new BoardController().DeleteAll();
            new ColumnController().DeleteAll();
            new TaskController().DeleteAll();

            // Create a new Column (persists a ColumnDTO)
            _column = new Column(ColumnIndex, BoardId);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean persistent database tables after each test
            new BoardController().DeleteAll();
            new ColumnController().DeleteAll();
            new TaskController().DeleteAll();
        }

        // === AddTask Tests ===
        [Test]
        public void AddTask_ValidNewTask_ShouldBeAddedSuccessfully()
        {
            var task = CreateTask(1);
            _column.AddTask(task);
            Assert.IsTrue(_column.Tasks.ContainsKey(1));
            Assert.AreEqual(1, _column.Tasks.Count);
        }

        [Test]
        public void AddTask_NullTask_ShouldThrowArgumentNullException()
            => Assert.Throws<ArgumentNullException>(() => _column.AddTask(null));

        [Test]
        public void AddTask_WithDuplicateTaskId_ShouldThrowArgumentException()
        {
            _column.AddTask(CreateTask(1));
            Assert.Throws<ArgumentException>(() => _column.AddTask(CreateTask(1)));
        }

        [Test]
        public void AddTask_WhenColumnIsAtLimit_ShouldThrowInvalidOperationException()
        {
            _column.LimitColumn(1);
            _column.AddTask(CreateTask(1));
            Assert.Throws<InvalidOperationException>(() => _column.AddTask(CreateTask(2)));
        }

        [Test]
        public void AddTask_WithUnlimitedLimit_ShouldAlwaysSucceed()
        {
            for (int i = 1; i <= 100; i++)
                _column.AddTask(CreateTask(i));
            Assert.AreEqual(100, _column.Tasks.Count);
        }

        [Test]
        public void AddTask_ToColumnWithZeroLimit_ShouldThrowInvalidOperationException()
        {
            _column.LimitColumn(0);
            Assert.Throws<InvalidOperationException>(() => _column.AddTask(CreateTask(1)));
        }

        // === RemoveTask Tests ===
        [Test]
        public void RemoveTask_ExistingTask_ShouldBeRemovedAndReturned()
        {
            var task = CreateTask(1);
            _column.AddTask(task);
            var removed = _column.RemoveTask(1);
            Assert.IsFalse(_column.Tasks.ContainsKey(1));
            Assert.AreEqual(0, _column.Tasks.Count);
            Assert.AreSame(task, removed);
        }

        [Test]
        public void RemoveTask_NonExistentTaskId_ShouldThrowArgumentException()
            => Assert.Throws<ArgumentException>(() => _column.RemoveTask(999));

        // === UpdateTask Tests ===
        [Test]
        public void UpdateTaskTitle_NonExistentTask_ShouldThrowArgumentException()
            => Assert.Throws<ArgumentException>(() => _column.UpdateTaskTitle(999, "New Title"));

        // === LimitColumn Tests ===
        [Test]
        public void LimitColumn_ValidLimit_ShouldUpdateLimit()
        {
            _column.LimitColumn(5);
            Assert.AreEqual(5, _column.Limit);
        }

        [Test]
        public void LimitColumn_NegativeLimit_ShouldThrowInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => _column.LimitColumn(-10));

        [Test]
        public void LimitColumn_LimitLowerThanCurrentTaskCount_ShouldThrowInvalidOperationException()
        {
            _column.AddTask(CreateTask(1));
            _column.AddTask(CreateTask(2));
            Assert.Throws<InvalidOperationException>(() => _column.LimitColumn(1));
        }

        [Test]
        public void LimitColumn_SetToCurrentTaskCount_ShouldSucceed()
        {
            _column.AddTask(CreateTask(1));
            _column.AddTask(CreateTask(2));
            _column.LimitColumn(2);
            Assert.AreEqual(2, _column.Limit);
        }

        [Test]
        public void LimitColumn_SetToZeroOnEmptyColumn_ShouldSucceed()
        {
            _column.LimitColumn(0);
            Assert.AreEqual(0, _column.Limit);
        }

        // === IsColumnFull Tests ===
        [Test]
        public void IsColumnFull_WhenTasksMatchLimit_ShouldReturnTrue()
        {
            _column.LimitColumn(2);
            _column.AddTask(CreateTask(1));
            _column.AddTask(CreateTask(2));
            Assert.IsTrue(_column.IsColumnFull());
        }

        [Test]
        public void IsColumnFull_WhenTasksAreBelowLimit_ShouldReturnFalse()
        {
            _column.LimitColumn(3);
            _column.AddTask(CreateTask(1));
            _column.AddTask(CreateTask(2));
            Assert.IsFalse(_column.IsColumnFull());
        }

        [Test]
        public void IsColumnFull_WhenUnlimited_ShouldAlwaysReturnFalse()
        {
            _column.AddTask(CreateTask(1));
            _column.AddTask(CreateTask(2));
            Assert.IsFalse(_column.IsColumnFull());
        }

        [Test]
        public void IsColumnFull_WhenZeroLimitOnEmpty_ShouldReturnTrue()
        {
            _column.LimitColumn(0);
            Assert.IsTrue(_column.IsColumnFull());
        }

        // === GetTask Tests ===
        [Test]
        public void GetTask_ExistingTaskId_ShouldReturnTask()
        {
            var task = CreateTask(123);
            _column.AddTask(task);
            var actual = _column.GetTask(123);
            Assert.AreSame(task, actual);
        }

        [Test]
        public void GetTask_NonExistentTaskId_ShouldThrowArgumentException()
            => Assert.Throws<ArgumentException>(() => _column.GetTask(999));

        // === Cross-Cutting Tests ===
        [Test]
        public void AddRemoveAdd_MultipleOperations_ShouldMaintainConsistentState()
        {
            var t1 = CreateTask(1);
            var t2 = CreateTask(2);
            var t3 = CreateTask(3);

            _column.AddTask(t1);
            _column.AddTask(t2);
            _column.RemoveTask(1);
            _column.AddTask(t3);

            Assert.AreEqual(2, _column.Tasks.Count);
            Assert.IsFalse(_column.Tasks.ContainsKey(1));
            Assert.IsTrue(_column.Tasks.ContainsKey(2));
            Assert.IsTrue(_column.Tasks.ContainsKey(3));
        }
    }
}