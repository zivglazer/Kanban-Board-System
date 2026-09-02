using NUnit.Framework;
using IntroSE.Kanban.Backend.BusinessLayer;
using System;
using System.Linq;
using IntroSE.Kanban.Backend.DataAccessLayer;

namespace BusinessLayerTests
{
    public class BoardTests
    {
        private Board _board;
        private const string OwnerEmail = "owner@test.com";
        private const string MemberEmail = "member@test.com";

        [SetUp]
        public void SetUp()
        {
            _board = new Board(1, "Test Board", OwnerEmail);
        }

        // =================================================================================
        // AddTask Tests
        // =================================================================================
        [TearDown]
        public void TearDown()
        {
            // Clean persistent database tables after each test
            new BoardController().DeleteAll();
            new ColumnController().DeleteAll();
            new TaskController().DeleteAll();
        }
        [Test]
        public void AddTask_ValidParameters_ShouldAddTaskToBacklogAndIncrementTaskCount()
        {
            // Arrange
            int initialTaskCount = _board.TaskCount;

            // Act
            var task = _board.AddTask("Valid Title", "Valid Description", DateTime.Now.AddDays(1));

            // Assert
            Assert.IsNotNull(task);
            Assert.AreEqual(initialTaskCount, task.TaskID);
            Assert.AreEqual(initialTaskCount + 1, _board.TaskCount);
            Assert.IsTrue(_board.States[0].Tasks.ContainsKey(task.TaskID));
            Assert.AreEqual("Valid Title", task.Title);
        }

    
        [Test]
        public void AddTask_BacklogColumnIsFull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _board.LimitColumn(0, 1);
            _board.AddTask("First Task", "Desc", DateTime.Now.AddDays(1));
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _board.AddTask("Second Task", "Desc", DateTime.Now.AddDays(1)));
        }

        // =================================================================================
        // AdvanceTask Tests
        // =================================================================================
        
        [Test]
        public void AdvanceTask_FromBacklogToInProgress_ShouldSucceed()
        {
            // Arrange
            var task = _board.AddTask("Test Task", "Desc", DateTime.Now.AddDays(1));
            _board.AssignTask(OwnerEmail, OwnerEmail, task.TaskID, 0);

            // Act
            _board.AdvanceTask(0, task.TaskID);

            // Assert
            Assert.IsFalse(_board.States[0].Tasks.ContainsKey(task.TaskID));
            Assert.IsTrue(_board.States[1].Tasks.ContainsKey(task.TaskID));
        }

        [Test]
        public void AdvanceTask_FromInProgressToDone_ShouldSucceed()
        {
            // Arrange
            var task = _board.AddTask("Test Task", "Desc", DateTime.Now.AddDays(1));
            _board.AssignTask(OwnerEmail, OwnerEmail, task.TaskID, 0);
            _board.AdvanceTask(0, task.TaskID); // Move to in-progress

            // Act
            _board.AdvanceTask(1, task.TaskID);

            // Assert
            Assert.IsFalse(_board.States[1].Tasks.ContainsKey(task.TaskID));
            Assert.IsTrue(_board.States[2].Tasks.ContainsKey(task.TaskID));
        }
        
        [Test]
        public void AdvanceTask_FromDoneColumn_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var task = _board.AddTask("Test Task", "Desc", DateTime.Now.AddDays(1));
            _board.AssignTask(OwnerEmail, OwnerEmail, task.TaskID, 0);
            _board.AdvanceTask(0, task.TaskID);
            _board.AdvanceTask(1, task.TaskID); // Task is now in 'done'

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _board.AdvanceTask(2, task.TaskID));
        }

        [Test]
        public void AdvanceTask_WithNonExistentTaskId_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _board.AdvanceTask(0, 999));
        }
        
        [Test]
        public void AdvanceTask_ToFullColumn_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _board.LimitColumn(1, 1); // Limit in-progress to 1
            var task1 = _board.AddTask("Task 1", "Desc", DateTime.Now.AddDays(1));
            var task2 = _board.AddTask("Task 2", "Desc", DateTime.Now.AddDays(1));
            _board.AssignTask(OwnerEmail, OwnerEmail, task1.TaskID, 0);
            _board.AssignTask(OwnerEmail, OwnerEmail, task2.TaskID, 0);
            _board.AdvanceTask(0, task1.TaskID); // in-progress is now full

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _board.AdvanceTask(0, task2.TaskID));
        }
        
        [TestCase(-1)]
        [TestCase(3)]
        public void AdvanceTask_InvalidColumnIndex_ShouldThrowArgumentOutOfRangeException(int columnIndex)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _board.AdvanceTask(columnIndex, 1));
        }


   

        // =================================================================================
        // Ownership & Assignment Tests
        // =================================================================================
        
        [Test]
        public void TransferOwnership_ToNewOwner_ShouldSucceed()
        {
            // Act
            _board.TransferOwnership(MemberEmail);

            // Assert
            Assert.AreEqual(MemberEmail, _board.Owner);
        }

        [Test]
        public void TransferOwnership_ToSameOwner_ShouldThrowInvalidOperationException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _board.TransferOwnership(OwnerEmail));
        }

        [Test]
        public void AssignTask_WhenUnassigned_ShouldSucceed()
        {
            // Arrange
            var task = _board.AddTask("Unassigned Task", "Desc", DateTime.Now.AddDays(1));

            // Act
            _board.AssignTask(OwnerEmail, MemberEmail, task.TaskID, 0);

            // Assert
            Assert.AreEqual(MemberEmail, task.Assignee);
        }

        [Test]
        public void AssignTask_WhenAssignedByNonAssigner_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var task = _board.AddTask("Task", "Desc", DateTime.Now.AddDays(1));
            _board.AssignTask(OwnerEmail, MemberEmail, task.TaskID, 0); // Assigned to MemberEmail

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _board.AssignTask(OwnerEmail, "another@user.com", task.TaskID, 0));
        }

        [Test]
        public void UnassignTasks_ForSpecificUser_ShouldOnlyUnassignTheirTasks()
        {
            // Arrange
            var task1 = _board.AddTask("Task 1", "Desc", DateTime.Now.AddDays(1));
            var task2 = _board.AddTask("Task 2", "Desc", DateTime.Now.AddDays(1));
            _board.AssignTask(OwnerEmail, MemberEmail, task1.TaskID, 0);
            _board.AssignTask(OwnerEmail, OwnerEmail, task2.TaskID, 0);

            // Act
            _board.UnassignTasks(MemberEmail);

            // Assert
            Assert.AreEqual("unassigned", _board.GetTask(task1.TaskID, 0).Assignee);
            Assert.AreEqual(OwnerEmail, _board.GetTask(task2.TaskID, 0).Assignee);
        }
    }
}