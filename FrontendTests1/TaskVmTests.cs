using NUnit.Framework;
using Frontend.Model;
using Frontend.ViewModel;
using System;
using System.Linq;
using Frontend.Controllers; // Ensure BackendController is accessible

namespace FrontendUnitTests
{
    [TestFixture]
    public class TaskVmTests
    {
        // OneTimeSetUp/TearDown (or similar setup for cleaning DB and creating users)
        // is assumed to be handled outside this snippet.
        // For these tests to run independently and cleanly, ensure your BackendController
        // is configured for testing (e.g., using an in-memory DB or resetting a test DB).

        // Helper method for common setup
        private (UserModel user, DashboardVM dashboard, BoardModel board, BoardVM boardVM) SetupBoardForTaskTests(string boardNamePrefix, string userEmail = "mail@mail.com")
        {
            var user = new UserModel(new BackendController(), userEmail);
            var dashboard = new DashboardVM(user);
            dashboard.NewBoardName = boardNamePrefix + Guid.NewGuid();

            // It's crucial to handle potential exceptions from CreateBoard or LoadBoards
            // if they can fail in your actual BackendController.
            // For now, assuming they succeed for setup.
            try
            {
                dashboard.CreateBoard(); // Create the board via dashboard
                dashboard.LoadBoards(); // Load it to ensure it's in the collection
            }
            catch (Exception ex)
            {
                Assert.Fail($"Setup failed: Could not create or load board. Error: {ex.Message}");
            }


            // Find the newly created board. It's safer to rely on the actual loaded boards.
            var board = dashboard.Boards.Cast<BoardModel>()
                                   .FirstOrDefault(b => b.Name.StartsWith(boardNamePrefix));

            Assert.IsNotNull(board, $"Board starting with '{boardNamePrefix}' was not created/loaded. Check DashboardVM.CreateBoard and DashboardVM.LoadBoards.");

            var boardVM = new BoardVM(user, board);
            return (user, dashboard, board, boardVM);
        }

        // Helper method for common cleanup
        private void CleanupBoard(DashboardVM dashboard, BoardModel board)
        {
            try
            {
                dashboard.SelectedBoard = board;
                dashboard.DeleteBoard();
            }
            catch (Exception ex)
            {
                // Log or handle cleanup failure if necessary, but don't fail the test
                Console.WriteLine($"Cleanup failed for board {board?.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// הוספת משימה עם כותרת ריקה (אמור להיכשל ולזרוק חריגה)
        /// </summary>
        [Test]
        public void AddTask_EmptyTitle_ThrowsException()
        {
            // Arrange
            var (user, dashboard, board, boardVM) = SetupBoardForTaskTests("EmptyTitleBoard");

            boardVM.NewTaskTitle = ""; // Set to empty
            boardVM.NewTaskDescription = "Some description";
            boardVM.NewTaskDueDate = DateTime.Now.AddDays(2);

            // Act & Assert
            // Expect an Exception to be thrown when AddTask is called
            var ex = Assert.Throws<Exception>(() => boardVM.AddTask());

            // You can optionally assert on the message of the thrown exception if you have specific error messages.
            // For example, if your BackendController throws specific messages, or if your VM has client-side validation
            // that results in a particular message being thrown.
            // Assert.That(ex.Message, Does.Contain("Title cannot be empty")); // Uncomment and adjust if specific message is expected
            Console.WriteLine($"Caught expected exception for EmptyTitle: {ex.Message}"); // For debugging

            // Also assert that the task was NOT added to the board's backlog.
            // This is important because even if an exception is thrown, we want to ensure no side effects.
            Assert.IsEmpty(boardVM.Board.BacklogTasks, "Task should not be added when an empty title causes an exception.");

            // Clean-up
            CleanupBoard(dashboard, board);
        }



        /// <summary>
        /// הוספת משימה עם כותרת ארוכה מדי (אמור להיכשל ולזרוק חריגה)
        /// </summary>
        [Test]
        public void AddTask_LongTitle_ThrowsException()
        {
            // Arrange
            var (user, dashboard, board, boardVM) = SetupBoardForTaskTests("LongTitleBoard");

            boardVM.NewTaskTitle = new string('A', 51); // 51 characters, assuming max is 50
            boardVM.NewTaskDescription = "Valid description";
            boardVM.NewTaskDueDate = DateTime.Now.AddDays(2);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => boardVM.AddTask());
            Console.WriteLine($"Caught expected exception for LongTitle: {ex.Message}");
            Assert.IsEmpty(boardVM.Board.BacklogTasks, "Task should not be added when a long title causes an exception.");

            // Clean-up
            CleanupBoard(dashboard, board);
        }

        /// <summary>
        /// הוספת משימה עם תיאור ארוך מדי (אמור להיכשל ולזרוק חריגה)
        /// </summary>
        [Test]
        public void AddTask_LongDescription_ThrowsException()
        {
            // Arrange
            var (user, dashboard, board, boardVM) = SetupBoardForTaskTests("LongDescBoard");

            boardVM.NewTaskTitle = "Valid";
            boardVM.NewTaskDescription = new string('B', 301); // 301 characters, assuming max is 300
            boardVM.NewTaskDueDate = DateTime.Now.AddDays(2);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => boardVM.AddTask());
            Console.WriteLine($"Caught expected exception for LongDescription: {ex.Message}");
            Assert.IsEmpty(boardVM.Board.BacklogTasks, "Task should not be added when a long description causes an exception.");

            // Clean-up
            CleanupBoard(dashboard, board);
        }

        /// <summary>
        /// הוספת משימה עם תאריך יעד בעבר (אמור להיכשל ולזרוק חריגה)
        /// </summary>
        [Test]
        public void AddTask_PastDueDate_ThrowsException()
        {
            // Arrange
            var (user, dashboard, board, boardVM) = SetupBoardForTaskTests("PastDueDateBoard");

            boardVM.NewTaskTitle = "Valid";
            boardVM.NewTaskDescription = "desc";
            boardVM.NewTaskDueDate = DateTime.Now.AddDays(-1); // Past date

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => boardVM.AddTask());
            Console.WriteLine($"Caught expected exception for PastDueDate: {ex.Message}");
            Assert.IsEmpty(boardVM.Board.BacklogTasks, "Task should not be added when a past due date causes an exception.");

            // Clean-up
            CleanupBoard(dashboard, board);
        }

        // --- Example of a successful add task test (no exception expected) ---
        /// <summary>
        /// הוספת משימה חוקית (אמור להצליח ולא לזרוק חריגה)
        /// </summary>
        [Test]
        public void AddTask_ValidTask_Success()
        {
            // Arrange
            var (user, dashboard, board, boardVM) = SetupBoardForTaskTests("ValidTaskBoard");

            boardVM.NewTaskTitle = "My Valid Task";
            boardVM.NewTaskDescription = "This is a valid task description.";
            boardVM.NewTaskDueDate = DateTime.Now.AddDays(3);

            // Act
            // No Assert.Throws here, as we expect the operation to succeed without an exception.
            boardVM.AddTask();

            // Assert
            // When successful, the ErrorMessage should be cleared (if your VM logic does that)
            // or remain empty if no error occurred.
            Assert.IsTrue(string.IsNullOrEmpty(boardVM.ErrorMessage), "Expected no error message for a valid task, but one was shown.");
            Assert.IsNotEmpty(boardVM.Board.BacklogTasks, "Task should be added to backlog.");
            Assert.AreEqual(1, boardVM.Board.BacklogTasks.Count, "Expected one task in backlog after successful addition.");
            Assert.AreEqual("My Valid Task", boardVM.Board.BacklogTasks[0].Title);
            // Optionally, verify other properties like description, due date, status etc.
            Assert.AreEqual("This is a valid task description.", boardVM.Board.BacklogTasks[0].Description);
            Assert.AreEqual(DateTime.Now.Date.AddDays(3), boardVM.Board.BacklogTasks[0].DueDate.Date); // Compare dates only

            // Clean-up
            CleanupBoard(dashboard, board);
        }

        // --- Additional tests for AdvanceTask and AssignTask, using Assert.Throws ---

        /// <summary>
        /// בדיקת העברת משימה ללא בחירה (אמור להיכשל ולזרוק חריגה)
        /// </summary>
        [Test]
        public void AdvanceTask_NoTaskSelected_ThrowsException()
        {
            // Arrange
            var (user, dashboard, board, boardVM) = SetupBoardForTaskTests("AdvanceTaskNoSelectBoard");
            // Ensure no task is selected
            boardVM.SelectedTask = null;

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => boardVM.AdvanceTask());
            Console.WriteLine($"Caught expected exception for AdvanceTask (no selection): {ex.Message}");
            Assert.That(ex.Message, Does.Contain("Please select a task.")); // Check for specific message

            // Clean-up
            CleanupBoard(dashboard, board);
        }

        /// <summary>
        /// בדיקת השמת משימה ללא בחירה (אמור להיכשל ולזרוק חריגה)
        /// </summary>
        [Test]
        public void AssignTask_NoTaskSelected_ThrowsException()
        {
            // Arrange
            var (user, dashboard, board, boardVM) = SetupBoardForTaskTests("AssignTaskNoSelectBoard");
            // Ensure no task is selected
            boardVM.SelectedTask = null;

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => boardVM.AssignTask("some@email.com"));
            Console.WriteLine($"Caught expected exception for AssignTask (no selection): {ex.Message}");
            Assert.That(ex.Message, Does.Contain("Please select a task.")); // Check for specific message

            // Clean-up
            CleanupBoard(dashboard, board);
        }

        // You'll need to add more complex tests for AdvanceTask and AssignTask
        // that involve actual tasks and interactions with your BackendController,
        // and test both success and failure cases where the BackendController
        // is expected to throw exceptions (e.g., assigning to a non-existent user,
        // advancing a task beyond its allowed state etc.).
    }
}