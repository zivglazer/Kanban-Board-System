using IntroSE.Kanban.Backend.ServiceLayer.Services;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using System.Text.Json;
using IntroSE.Kanban.Backend.ServiceLayer;


/**
 * Black-box tests for the Service Layer.
 * NUnit is used for convenience; the goal is to validate external behavior,
 * not to perform white-box testing of the Service Layer classes.
 **/

namespace ServiceLayerTests
{
    public class BoardServiceTests
    {
        private string email = "test@gmail.com";
        private string guest = "guest@gmail.com";
        private string boardName = "MyBoard";
        private int boardId = 1;
        ServiceFactory serviceFactory;

        [SetUp]
        public void Setup()
        {
            // Arrange: Initialize services and ensure a clean database state.
            serviceFactory = new ServiceFactory();

            TearDown();

           
            serviceFactory.UserService.Register(email, "Test123");
            serviceFactory.UserService.Register(guest, "Pass123");
            boardId = 1;
        }

        [TearDown]
        public void TearDown()
        {
            serviceFactory.DeleteData();
        }

        [Test]
        public void CreateBoard_ValidInput_PersistsBoard()
        {
            string boardName = "TestBoard";
            string json = serviceFactory.BoardService.CreateBoard(email, boardName);
            Response<string> response = JsonSerializer.Deserialize<Response<string>>(json);
            Assert.That(response.ErrorMessage, Is.Null);

            // Persistence check: board should exist
            var boards = serviceFactory.BoardService.GetUserBoards(email);
            var boardsResponse = JsonSerializer.Deserialize<Response<List<BoardSL>>>(boards);
            Assert.IsTrue(boardsResponse.ReturnValue.Any());
        }

        [Test]
        public void DeleteBoard_ValidInput_RemovesBoard()
        {
            string boardName = "ToDelete";
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string json = serviceFactory.BoardService.DeleteBoard(email, boardName);
            Response<BoardSL> response = JsonSerializer.Deserialize<Response<BoardSL>>(json);
            Assert.That(response.ErrorMessage, Is.Null);

            // Persistence check: board should not exist
            var boards = serviceFactory.BoardService.GetUserBoards(email);
            var boardsResponse = JsonSerializer.Deserialize<Response<List<BoardSL>>>(boards);
            Assert.IsFalse(boardsResponse.ReturnValue.Any(b => b.BoardID == boardId));
        }

        [Test]
        public void LimitColumn_ValidInput_PersistsLimit()
        {
            string boardName = "LimitTest";
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.BoardService.LimitColumn(email, boardName, 0, 5);

            // Persistence check: limit should be 5
            string json = serviceFactory.BoardService.GetColumnLimit(email, boardName, 0);
            Response<int> response = JsonSerializer.Deserialize<Response<int>>(json);
            Assert.That(response.ReturnValue, Is.EqualTo(5));
        }

        [Test]
        public void AddParticipant_ValidUser_PersistsMembership()
        {
            string board = "SharedBoard";
            serviceFactory.BoardService.CreateBoard(email, board);
            serviceFactory.BoardService.JoinBoard(guest, boardId);

            // Persistence check: guest should be able to leave (is a member)
            string json = serviceFactory.BoardService.LeaveBoard(guest, boardId);
            Response<BoardSL> response = JsonSerializer.Deserialize<Response<BoardSL>>(json);
            Assert.IsNull(response.ErrorMessage);
        }

        [Test]
        public void RemoveParticipant_ValidUser_RemovesMembership()
        {
            string board = "SharedBoard";
            serviceFactory.BoardService.CreateBoard(email, board);
            serviceFactory.BoardService.JoinBoard(guest, boardId);
            serviceFactory.BoardService.LeaveBoard(guest, boardId);

            // Persistence check: guest should not be able to leave again (not a member)
            string json = serviceFactory.BoardService.LeaveBoard(guest, boardId);
            Response<BoardSL> response = JsonSerializer.Deserialize<Response<BoardSL>>(json);
            Assert.IsNotNull(response.ErrorMessage);
        }

        [Test]
        public void GuestLeavesBoardUnassignsTasks_Persistency()
        {
            string boardName = "GuestLeaveBoardUnassignsTasks";
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.BoardService.JoinBoard(guest, boardId);
            serviceFactory.TaskService.AddTask(email, boardName, "Task1", "Description1", DateTime.Now.AddDays(5));
            serviceFactory.TaskService.AddTask(email, boardName, "Task2", "Description2", DateTime.Now.AddDays(5));
            serviceFactory.BoardService.LeaveBoard(guest, boardId);

            // Persistence check: tasks still exist
            string json = serviceFactory.BoardService.GetColumn(email, boardName, 0);
            Response<List<TaskSL>> response = JsonSerializer.Deserialize<Response<List<TaskSL>>>(json);
            Assert.That(response.ReturnValue.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetUserBoardsTest_Persistency()
        {
            string boardName2 = "GetUserBoardsTest";
            serviceFactory.BoardService.CreateBoard(email, boardName);
            serviceFactory.BoardService.CreateBoard(email, boardName2);

            string json = serviceFactory.BoardService.GetUserBoards(email);
            Response<List<BoardSL>> response = JsonSerializer.Deserialize<Response<List<BoardSL>>>(json);

            Assert.That(response.ReturnValue.Count, Is.EqualTo(2));
        }

        [Test]
        public void TransferOwnership_InvalidBoardName()
        {
            string invalidBoardName = "InvalidBoard";
            string json = serviceFactory.BoardService.TransferOwnership(invalidBoardName, email, guest);
            Response<BoardSL> response = JsonSerializer.Deserialize<Response<BoardSL>>(json);
            Assert.That(response.ErrorMessage, Is.EqualTo("Board doesnt exist."));
        }

        [Test]
        public void TrafficTransferOwnership_InvalidOldOwner()
        {
            string json = serviceFactory.BoardService.TransferOwnership(boardName, "invalidEmail", guest);
            Response<BoardSL> response = JsonSerializer.Deserialize<Response<BoardSL>>(json);
            Assert.That(response.ErrorMessage, Is.EqualTo("User not found."));
        }

        [Test]
        public void TransferOwnership_InvalidNewOwner()
        {
            string invalidNewOwner = "invalidEmail";
            string json = serviceFactory.BoardService.TransferOwnership(boardName, email, invalidNewOwner);
            Response<BoardSL> response = JsonSerializer.Deserialize<Response<BoardSL>>(json);
            Assert.That(response.ErrorMessage, Is.EqualTo("User not found."));
        }

        [Test]
        public void TransferOwnership_AlreadyOwner()
        {
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string json = serviceFactory.BoardService.TransferOwnership(boardName, email, email);
            Response<string>? response = JsonSerializer.Deserialize<Response<string>>(json);
            Assert.IsNotNull(response, "Deserialization returned null.");
            Assert.That(response.ErrorMessage, Is.EqualTo("User is already the owner of the board."));
        }

        [Test]
        public void AddParticipant_NonExistentBoard_ReturnsError()
        {
            string json = serviceFactory.BoardService.LeaveBoard(guest, boardId + 100);
            Response<BoardSL> response = JsonSerializer.Deserialize<Response<BoardSL>>(json);
            Assert.IsNotNull(response.ErrorMessage);
        }

        [Test]
        public void RemoveParticipant_NonExistentBoard_ReturnsError()
        {
            string json = serviceFactory.BoardService.LeaveBoard(guest, boardId + 100);
            Response<BoardSL> response = JsonSerializer.Deserialize<Response<BoardSL>>(json);
            Assert.IsNotNull(response.ErrorMessage);
        }

        [Test]
        public void AddParticipant_AlreadyParticipant_ReturnsError()
        {
            string board = "SharedBoard";
            serviceFactory.BoardService.CreateBoard(email, board);
            serviceFactory.BoardService.JoinBoard(guest, boardId);
            string json = serviceFactory.BoardService.JoinBoard(guest, boardId);
            Response<BoardSL> response = JsonSerializer.Deserialize<Response<BoardSL>>(json);
            Assert.IsNotNull(response.ErrorMessage);
        }

        [Test]
        public void RemoveParticipant_NotAParticipant_ReturnsError()
        {
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string json = serviceFactory.BoardService.LeaveBoard(guest, boardId);
            Response<BoardSL> response = JsonSerializer.Deserialize<Response<BoardSL>>(json);
            Assert.IsNotNull(response.ErrorMessage);
        }

        [Test]
        public void DeleteBoard_OnlyOwnerCanDeleteBoard()
        {
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string jsonOwnerDelete = serviceFactory.BoardService.DeleteBoard(email, boardName);
            Response<string> responseOwnerDelete = JsonSerializer.Deserialize<Response<string>>(jsonOwnerDelete);
            Assert.That(responseOwnerDelete.ErrorMessage, Is.Null);

            serviceFactory.BoardService.CreateBoard(email, boardName);
            boardId++;
            serviceFactory.BoardService.JoinBoard(guest, boardId);
            string jsonNotOwnerDelete = serviceFactory.BoardService.DeleteBoard(guest, boardName);
            Response<string> responseNotOwnerDelete = JsonSerializer.Deserialize<Response<string>>(jsonNotOwnerDelete);
            Assert.That(responseNotOwnerDelete.ErrorMessage, Is.EqualTo("guest@gmail.com is not the board owner"));
        }

        [Test]
        public void OwnerWantsToLeaveBoard()
        {
            string boardName = "OwnerLeaveBoard";
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string json = serviceFactory.BoardService.LeaveBoard(email, boardId);
            Response<string> response = JsonSerializer.Deserialize<Response<string>>(json);
            Assert.That(response.ErrorMessage, Is.EqualTo("Cannot remove owner from board."));
        }

        [Test]
        public void GuestWantsToLeaveBoardNotIn()
        {
            string boardName = "GuestLeaveBoardNotIn";
            serviceFactory.BoardService.CreateBoard(email, boardName);
            string json = serviceFactory.BoardService.LeaveBoard(guest, boardId);
            Response<string> response = JsonSerializer.Deserialize<Response<string>>(json);
            Assert.That(response.ErrorMessage, Is.EqualTo($"{guest} is not a participant in the board."));
        }

        [Test]
        public void GetUserBoardsTestNoBoards()
        {
            string json = serviceFactory.BoardService.GetUserBoards(email);
            Response<List<BoardSL>> response = JsonSerializer.Deserialize<Response<List<BoardSL>>>(json);
            Assert.That(response.ReturnValue.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetUserBoardsTestNotRegistered()
        {
            string userNotRegistered = "notRegistered";
            string json = serviceFactory.BoardService.GetUserBoards(userNotRegistered);
            Response<List<BoardSL>> response = JsonSerializer.Deserialize<Response<List<BoardSL>>>(json);
            Assert.That(response.ErrorMessage, Is.EqualTo("User not found."));
        }

        [Test]
        public void GetUserBoardsTestNotLoggedIn()
        {
            serviceFactory.UserService.Logout(email);
            string json = serviceFactory.BoardService.GetUserBoards(email);
            Response<List<BoardSL>> response = JsonSerializer.Deserialize<Response<List<BoardSL>>>(json);
            Assert.That(response.ErrorMessage, Is.EqualTo("User is not logged in."));
        }
    }
}
