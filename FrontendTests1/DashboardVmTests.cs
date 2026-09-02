using Frontend.Model;
using Frontend.ViewModel;
using NUnit.Framework;
using System;
using System.Linq;

namespace FrontendUnitTests
{
    [TestFixture]
    public class DashboardVmTests
    {

        private BackendController _backendController;
        private UserModel _mainUser;
        private UserModel _otherUser;

        /// <summary>
        /// This SetUp method runs before EACH test in this fixture.
        /// It ensures a clean and consistent state for every test by creating fresh objects
        /// and clearing the database specifically for this test's context.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Console.WriteLine($"--- Running SetUp for DashboardVmTests ---");

            // 1. Ensure the BackendServiceFactory from TestDatabaseSetup is initialized and available for database operations.
            if (TestDatabaseSetup.BackendServiceFactory == null)
            {
                // This indicates an issue in TestDatabaseSetup.OneTimeSetUp, which should run first.
                throw new InvalidOperationException("BackendServiceFactory not initialized by TestDatabaseSetup.OneTimeSetUp.");
            }

            // 2. Clear all existing data in the database before this test runs.
            // This is crucial for test isolation, ensuring each test starts from a clean slate.
            TestDatabaseSetup.BackendServiceFactory.DeleteData();
            Console.WriteLine("Data cleared successfully before test.");

            // 3. Create NEW instances of BackendController and UserModel for EACH test.
            // This ensures that each test gets its own independent controller and user model,
            // preventing side effects from one test impacting another.
            _backendController = new BackendController();

            // Register and then log in the main user ("mail@mail.com") for this test.
            try
            {
                _backendController.UserController.Register("mail@mail.com", "Password1");
                _backendController.UserController.Login("mail@mail.com", "Password1");
            }
            catch (Exception ex)
            {
                // Log any errors during user setup (e.g., user already exists from a previous failed run
                // if DeleteData didn't fully clean up, though it should).
                Console.WriteLine($"Error during setup for main user (mail@mail.com): {ex.Message}. Ignoring if user already exists.");
            }
            // Initialize the _mainUser UserModel instance for this test.
            _mainUser = new UserModel(_backendController, "mail@mail.com");

            // Register and then log in the "other" user ("other@mail.com"), if this user is needed in any tests in this fixture.
            try
            {
                _backendController.UserController.Register("other@mail.com", "Password2");
                _backendController.UserController.Login("other@mail.com", "Password2");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during setup for other user (other@mail.com): {ex.Message}. Ignoring if user already exists.");
            }
            // Initialize the _otherUser UserModel instance for this test.
            _otherUser = new UserModel(_backendController, "other@mail.com");

            Console.WriteLine("New BackendController and UserModels created and users logged in for test.");
        }

        // --- Test Methods ---

        /// <summary>
        /// Tests that the dashboard correctly displays boards and their owners.
        /// This test creates a board and then verifies it appears correctly in the UI.
        /// </summary>
        [Test]
        public void Dashboard_DisplayBoards_BoardsAndOwnersVisible()
        {
            // Arrange
            // _mainUser (and its associated _backendController) are already initialized in the SetUp method.
            var dashboard = new DashboardVM(_mainUser);

            // Create a board *before* attempting to load boards to ensure there's data to display.
            // A unique name is used to prevent conflicts if previous cleanup failed for some reason.
            string boardName = "TestBoardForDisplay_" + Guid.NewGuid().ToString().Substring(0, 8);
            dashboard.NewBoardName = boardName;
            dashboard.CreateBoard(); // This calls the backend via BackendController to create the board.

            // Act
            dashboard.LoadBoards(); // Load the boards from the backend into the ViewModel.

            // Assert
            // 1. Verify the Boards collection is not null.
            Assert.IsNotNull(dashboard.Boards, "Boards collection should not be null after loading.");
            // 2. Verify at least one board is present (the one we just created).
            Assert.IsTrue(dashboard.Boards.Count > 0, "Expected at least one board to be displayed after creation and loading.");

            // 3. Verify the specific board we created is found and its properties are correct.
            var createdBoard = dashboard.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name == boardName);
            Assert.IsNotNull(createdBoard, $"Expected board '{boardName}' to be found in the displayed boards list.");
            Assert.IsFalse(string.IsNullOrEmpty(createdBoard.Name), "Board name should not be empty for the created board.");
            Assert.IsFalse(string.IsNullOrEmpty(createdBoard.OwnerEmail), "Board owner email should not be empty for the created board.");
            Assert.AreEqual(_mainUser.Email, createdBoard.OwnerEmail, "The owner email of the created board should match the main user's email.");

            // 4. Loop through all displayed boards to ensure general display properties are met (e.g., no empty names/owners).
            foreach (BoardModel board in dashboard.Boards)
            {
                Assert.IsFalse(string.IsNullOrEmpty(board.Name), $"Board name for board ID {board.Id} is empty or null for board: {board.Name}");
                Assert.IsFalse(string.IsNullOrEmpty(board.OwnerEmail), $"Board owner email for board ID {board.Id} is empty or null for board: {board.Name}");
            }

            // Cleanup specific to this test (optional, as SetUp/TearDown handle general cleanup,
            // but good for immediate verification and debugging if a test fails midway).
            if (createdBoard != null)
            {
                dashboard.SelectedBoard = createdBoard;
                dashboard.DeleteBoard(); // Delete the board we created.
                dashboard.LoadBoards(); // Reload to confirm it's gone for this test's context.
                Assert.IsFalse(dashboard.Boards.Cast<BoardModel>().Any(b => b.Name == boardName), "Board should be deleted as part of test-specific cleanup.");
            }
        }

        /// <summary>
        /// Tests the functionality of creating a new board and ensuring it's correctly added to the dashboard's list.
        /// </summary>
        [Test]
        public void Dashboard_CreateBoard_BoardAdded()
        {
            // Arrange
            // _mainUser is initialized in SetUp.
            var dashboard = new DashboardVM(_mainUser);

            // Load initial boards to get a baseline count before creating a new one.
            dashboard.LoadBoards();
            int initialCount = dashboard.Boards.Count;

            string uniqueBoardName = "TestBoard_Create_" + Guid.NewGuid().ToString().Substring(0, 8);
            dashboard.NewBoardName = uniqueBoardName; // Set the name for the new board.

            BoardModel createdBoard = null; // Variable to hold the reference to the created board for cleanup.

            try
            {
                // Act
                dashboard.CreateBoard(); // Perform the board creation action via the ViewModel.
                dashboard.LoadBoards(); // Reload boards to reflect the change from the backend.

                // Assert
                // 1. Verify the board count increased by one.
                Assert.IsTrue(dashboard.Boards.Count == initialCount + 1, "Board count should increase by one after successful creation.");
                // 2. Verify the newly created board can be found in the dashboard's list.
                createdBoard = dashboard.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name == uniqueBoardName);
                Assert.IsNotNull(createdBoard, "The newly created board should be found in the dashboard's boards list after creation.");
            }
            finally
            {
                // Cleanup: Ensure the board created during the test is deleted, even if assertions fail.
                if (createdBoard != null)
                {
                    dashboard.SelectedBoard = createdBoard; // Select the board to be deleted.
                    dashboard.DeleteBoard(); // Delete the board.
                    dashboard.LoadBoards(); // Reload to confirm deletion.
                    Assert.IsFalse(dashboard.Boards.Cast<BoardModel>().Any(b => b.Name == uniqueBoardName), "Board should be deleted during cleanup after test.");
                }
            }
        }

        /// <summary>
        /// Tests the functionality of deleting an existing board from the dashboard.
        /// </summary>
        [Test]
        public void Dashboard_DeleteBoard_BoardRemoved()
        {
            // Arrange
            // _mainUser is initialized in SetUp.
            var dashboard = new DashboardVM(_mainUser);

            // First, create a board that will be the target of the deletion action in this test.
            string boardNameToDelete = "ToDelete_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            dashboard.NewBoardName = boardNameToDelete;
            dashboard.CreateBoard();

            // Load boards to get the reference of the board we just created.
            dashboard.LoadBoards();
            var boardToDelete = dashboard.Boards.Cast<BoardModel>().FirstOrDefault(b => b.Name == boardNameToDelete);
            Assert.IsNotNull(boardToDelete, "Board to delete was not found after creation, cannot proceed with deletion test.");

            int initialCountAfterCreation = dashboard.Boards.Count; // Get count before deletion.

            // Act
            dashboard.SelectedBoard = boardToDelete; // Select the board to be deleted in the UI.
            dashboard.DeleteBoard(); // Perform the deletion action via the ViewModel.

            dashboard.LoadBoards(); // Reload boards to verify the state after deletion.

            // Assert
            // 1. Verify the board is no longer present in the dashboard's list.
            Assert.IsFalse(dashboard.Boards.Cast<BoardModel>().Any(b => b.Name == boardNameToDelete), "Board should be removed from the dashboard after deletion.");
            // 2. Verify the board count decreased by one.
            Assert.IsTrue(dashboard.Boards.Count == initialCountAfterCreation - 1, "Board count should decrease by one after successful deletion.");
        }
    }
}