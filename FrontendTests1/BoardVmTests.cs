using Frontend.Model;
using Frontend.ViewModel;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using IntroSE.Kanban.Backend.DataAccessLayer;
using System.IO;

namespace FrontendUnitTests
{
    [TestFixture]
    public class BoardVmTests
    {

        private static BackendController _backendController;
        private static UserModel _mainUser;
        private static UserModel _otherUser;
        private static string _dbPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "kanban_test.db");

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // Ensure the database file is not locked by a previous run
            CleanupDatabase();

            _backendController = new BackendController();

            try
            {
                _backendController.UserController.Register("mail@mail.com", "Password1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering main user (mail@mail.com): {ex.Message}. Ignoring if user already exists.");
            }
            _mainUser = new UserModel(_backendController, "mail@mail.com");

            try
            {
                _backendController.UserController.Register("other@mail.com", "Password2");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering other user (other@mail.com): {ex.Message}. Ignoring if user already exists.");
            }
            _otherUser = new UserModel(_backendController, "other@mail.com");

            Assert.IsNotNull(_mainUser, "Main user should not be null after setup.");
            Assert.IsNotNull(_otherUser, "Other user should not be null after setup.");
        }

        private static void CleanupDatabase()
        {
            // Attempt to close any existing connections to the database
            // This might require reflection to access and dispose of internal connections
            // Or, ensure that the BackendController is properly disposed of after each test run

            // As a last resort, try to delete the database file if it exists
            if (File.Exists(_dbPath))
            {
                try
                {
                    // Wait a short time to allow any processes to release the file
                    Thread.Sleep(100);
                    File.Delete(_dbPath);
                    Console.WriteLine("Existing database file deleted successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting database file: {ex.Message}.  This might indicate a locking issue.");
                    // Consider logging the exception or re-throwing it if the database is essential
                }
            }
        }


        [Test]
        public void CreateBoard_DuplicateName_ErrorShown()
        {
            // Arrange
            var dashboard = new DashboardVM(_mainUser);
            string boardName = "MyBoard" + Guid.NewGuid();
            dashboard.NewBoardName = boardName;
            dashboard.CreateBoard();
            dashboard.LoadBoards();

            dashboard.NewBoardName = boardName.ToUpper();

            // Act & Assert
            Assert.Throws<Exception>(() => dashboard.CreateBoard(), "Expected an exception for duplicate board name");

            // Clean-up
            var board = dashboard.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name.Equals(boardName, StringComparison.OrdinalIgnoreCase));
            if (board != null)
            {
                dashboard.SelectedBoard = board;
                dashboard.DeleteBoard();
            }
            dashboard.ErrorMessage = "";
        }

        [Test]
        public void CreateBoard_NewName_Success()
        {
            // Arrange
            var dashboard = new DashboardVM(_mainUser);
            string boardName = "UniqueBoard" + Guid.NewGuid();
            dashboard.NewBoardName = boardName;

            // Act
            dashboard.CreateBoard();
            dashboard.LoadBoards();

            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(dashboard.ErrorMessage), "Unexpected error message was displayed");
            Assert.IsTrue(dashboard.Boards.Cast<BoardModel>().Any(b => b.Name == boardName), "New board was not added");


            // Clean-up
            var board = dashboard.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name == boardName);
            if (board != null)
            {
                dashboard.SelectedBoard = board;
                dashboard.DeleteBoard();
            }
            dashboard.ErrorMessage = "";
        }

        [Test]
        public void CreateBoard_EmptyName_ErrorShown()
        {
            // Arrange
            var dashboard = new DashboardVM(_mainUser);
            dashboard.NewBoardName = "";

            // Act
            Assert.Throws<Exception>(() => dashboard.CreateBoard(), "Expected an exception for empty board name");
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(dashboard.ErrorMessage), "System did not show error for empty name");
            // You might want to assert for a specific error message if your backend provides one.
            // Assert.AreEqual("Board name cannot be empty.", dashboard.ErrorMessage, "Error message does not match for empty name");
        }


        [Test]
        public void BoardVM_AddTask_ValidData_TaskAdded()
        {
            var dashboard = new DashboardVM(_mainUser);
            string boardName = "TestBoard" + Guid.NewGuid();
            dashboard.NewBoardName = boardName;
            dashboard.CreateBoard();
            dashboard.LoadBoards();
            var board = dashboard.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name == boardName);
            Assert.IsNotNull(board, "Board was not created successfully");

            // Load board details to ensure columns are populated (if not automatically on LoadBoards)
            // This is crucial if AddTask relies on columns existing.
            var detailedBoard = _backendController.BoardController.LoadBoardDetails(_mainUser.Email, board.Id, _backendController);
            Assert.IsNotNull(detailedBoard, "Detailed board could not be loaded.");

            var boardVM = new BoardVM(_mainUser, detailedBoard); // Use the detailed board
            boardVM.NewTaskTitle = "New Task";
            boardVM.NewTaskDescription = "desc";
            boardVM.NewTaskDueDate = DateTime.Now.AddDays(2);

            int initialCount = detailedBoard.BacklogTasks?.Count ?? 0;

            try
            {
                boardVM.AddTask();
                Assert.IsTrue(string.IsNullOrEmpty(boardVM.ErrorMessage), "Unexpected error message was displayed");
                // Reload board details to get updated task count from backend
                detailedBoard = _backendController.BoardController.LoadBoardDetails(_mainUser.Email, board.Id, _backendController);
                Assert.IsTrue(detailedBoard.BacklogTasks.Count > initialCount, "Task was not added successfully to backend");
            }
            finally
            {
                if (board != null)
                {
                    dashboard.SelectedBoard = board;
                    dashboard.DeleteBoard();
                    dashboard.LoadBoards();
                    Assert.IsFalse(dashboard.Boards.Cast<BoardModel>().Any(b => b.Name == board.Name), "Board was not properly deleted in cleanup");
                }
            }
        }


        // ב-FrontendUnitTests\BoardVmTests.cs


        [Test]
        public void DeleteBoard_ByOwner_Success()
        {
            // Arrange
            var dashboard = new DashboardVM(_mainUser);
            string boardName = "DeleteMe" + Guid.NewGuid();
            dashboard.NewBoardName = boardName;
            dashboard.CreateBoard();
            dashboard.LoadBoards();
            var board = dashboard.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name == boardName);
            Assert.IsNotNull(board, "Board was not created successfully for the test");

            dashboard.SelectedBoard = board;

            // Act
            dashboard.DeleteBoard();
            dashboard.LoadBoards();

            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(dashboard.ErrorMessage), "Unexpected error message was displayed");
            Assert.IsFalse(dashboard.Boards.Cast<BoardModel>().Any(b => b.Name == boardName), "Board was not deleted properly");
        }

        [Test]
        public void DeleteBoard_NotOwner_ErrorShown()
        {
            // Arrange
            var dashboardOwner = new DashboardVM(_mainUser);
            string boardName = "OwnerBoard" + Guid.NewGuid();
            dashboardOwner.NewBoardName = boardName;
            dashboardOwner.CreateBoard();
            dashboardOwner.LoadBoards();
            var board = dashboardOwner.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name == boardName);
            Assert.IsNotNull(board, "Board was not created successfully for the test");

            var dashboardOther = new DashboardVM(_otherUser);
            // In your current setup, DashboardVM doesn't implicitly load all boards for all users.
            // If the board is meant to be joinable/visible, you might need a backend call here
            // to simulate the other user "seeing" or "joining" the board.
            // For this test, we are assuming 'dashboardOther.SelectedBoard = board' is sufficient
            // to represent the other user attempting to interact with this specific board object.
            dashboardOther.SelectedBoard = board;

            // Act
            Assert.Throws<Exception>(() => dashboardOther.DeleteBoard(), "");
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(dashboardOther.ErrorMessage), "System did not show error for deletion by non-owner");
            // Ensure the board still exists for the owner
            dashboardOwner.LoadBoards();
            Assert.IsTrue(dashboardOwner.Boards.Cast<BoardModel>().Any(b => b.Name == boardName), "Board was accidentally deleted by non-owner");

            // Clean-up
            if (board != null)
            {
                dashboardOwner.SelectedBoard = board;
                dashboardOwner.DeleteBoard();
            }
            dashboardOwner.ErrorMessage = "";
        }

        [Test]
        public void DeleteBoard_NonExisting_ErrorShown()
        {
            // Arrange
            var dashboard = new DashboardVM(_mainUser);
            // Creating a dummy BoardModel for a non-existing board
            // The BackendController.DeleteBoard method accepts boardName.
            dashboard.SelectedBoard = new BoardModel(_backendController, 9999, "NoSuchBoard" + Guid.NewGuid(), "nonexistent@mail.com");

            // Act & Assert

            Assert.Throws<Exception>(() => dashboard.DeleteBoard(), "Expected an exception for deleting a non-existing board");

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(dashboard.ErrorMessage), "System did not show error for deleting a non-existing board");
            // Assert for a specific error message if your backend provides one
            // Assert.IsTrue(dashboard.ErrorMessage.Contains("Board not found") || dashboard.ErrorMessage.Contains("invalid"), "Error message does not match for non-existing board");
        }

        [Test]
        public void GetUserBoards_OnlyMemberBoards_Shown()
        {
            // Arrange
            var dashboardMainUser = new DashboardVM(_mainUser);
            string mainUserBoardName = "MainUserBoard" + Guid.NewGuid();
            dashboardMainUser.NewBoardName = mainUserBoardName;
            dashboardMainUser.CreateBoard();

            // Create a board for the other user. Assuming it's not automatically visible to mainUser.
            var dashboardOtherUser = new DashboardVM(_otherUser);
            string otherUserBoardName = "OtherUserBoard" + Guid.NewGuid();
            dashboardOtherUser.NewBoardName = otherUserBoardName;
            dashboardOtherUser.CreateBoard();

            // Act
            dashboardMainUser.LoadBoards(); // Load boards for the main user

            // Assert
            Assert.IsTrue(dashboardMainUser.Boards.Cast<BoardModel>().Any(b => b.Name == mainUserBoardName), "Main user's own board was not displayed");
            // Assert that the other user's board is NOT visible to the main user (unless explicitly joined/shared)
            Assert.IsFalse(dashboardMainUser.Boards.Cast<BoardModel>().Any(b => b.Name == otherUserBoardName), "Other user's board was mistakenly displayed to main user");


            // Clean-up
            var boardToCleanMain = dashboardMainUser.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name == mainUserBoardName);
            if (boardToCleanMain != null)
            {
                dashboardMainUser.SelectedBoard = boardToCleanMain;
                dashboardMainUser.DeleteBoard();
            }
            var boardToCleanOther = dashboardOtherUser.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name == otherUserBoardName);
            if (boardToCleanOther != null)
            {
                dashboardOtherUser.SelectedBoard = boardToCleanOther;
                dashboardOtherUser.DeleteBoard();
            }
        }

        [Test]
        public void GetUserBoards_OwnerVisibleForEachBoard()
        {
            // Arrange
            var dashboard = new DashboardVM(_mainUser);
            string boardName = "CheckOwnerBoard" + Guid.NewGuid();
            dashboard.NewBoardName = boardName;
            dashboard.CreateBoard();

            // Act
            dashboard.LoadBoards();

            // Assert
            foreach (BoardModel board in dashboard.Boards)
            {
                Assert.IsFalse(string.IsNullOrEmpty(board.OwnerEmail), $"Board owner for '{board.Name}' is not displayed");
            }

            // Clean-up
            var boardToClean = dashboard.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name == boardName);
            if (boardToClean != null)
            {
                dashboard.SelectedBoard = boardToClean;
                dashboard.DeleteBoard();
            }
        }


    }
}