using NUnit.Framework;
using IntroSE.Kanban.Backend.ServiceLayer;
using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using IntroSE.Kanban.Backend.BusinessLayer;
using System;
using System.Linq;

namespace BusinessLayerTests
{    public class BoardFacadeTests
    {
        private GradingService _service;
        private BoardController _boardController;
        private BoardMembersController _boardMembersController;


        private string _ownerEmail;
        private string _memberEmail;
        private string _nonMemberEmail;
        private string _password;

        [SetUp]
        public void SetUp()
        {
            // Arrange: Initialize services and ensure a clean database state.
            _service = new GradingService();
            _service.DeleteData();

            // Initialize DAL controllers for direct database verification.
            _boardController = new BoardController();
            _boardMembersController = new BoardMembersController();

            // Setup common users for tests
            _ownerEmail = "owner@test.com";
            _memberEmail = "member@test.com";
            _nonMemberEmail = "nonmember@test.com";
            _password = "Password123";
            _service.Register(_ownerEmail, _password);
            _service.Register(_memberEmail, _password);
            _service.Register(_nonMemberEmail, _password);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the database after each test to ensure isolation.
            _service.DeleteData();
        }

        // =================================================================================
        // CreateBoard Tests
        // =================================================================================

        [Test]
        public void CreateBoard_ValidUserAndName_ShouldPersistBoardAndMembership()
        {
            // Act
            string boardName = "New Persistent Board";
            _service.CreateBoard(_ownerEmail, boardName);

            // Assert
            var boardFromDb = _boardController.SelectAll().FirstOrDefault(b => b.Name == boardName);
            Assert.IsNotNull(boardFromDb, "Board was not found in the database.");
            Assert.AreEqual(_ownerEmail, boardFromDb.Owner, "Board owner is incorrect.");

            var memberFromDb = _boardMembersController.SelectAll().FirstOrDefault(m => m.BoardID == boardFromDb.ID && m.Email == _ownerEmail);
            Assert.IsNotNull(memberFromDb, "Owner's membership was not persisted.");
        }

        [Test]
        public void CreateBoard_DuplicateNameForSameUser_ShouldFailAndNotPersist()
        {
            // Arrange
            string boardName = "Duplicate Board";
            _service.CreateBoard(_ownerEmail, boardName);

            // Act
            var response = _service.CreateBoard(_ownerEmail, boardName);

            // Assert
            Assert.That(response, Does.Contain("ErrorMessage"), "Service should have returned an error for duplicate board name.");
            var boardCount = _boardController.SelectAll().Count(b => b.Name == boardName);
            Assert.AreEqual(1, boardCount, "A duplicate board was incorrectly persisted to the database.");
        }

        [TestCase(null)]
        [TestCase("")]
        public void CreateBoard_NullOrEmptyName_ShouldFail(string invalidName)
        {
            // Act
            var response = _service.CreateBoard(_ownerEmail, invalidName);

            // Assert
            Assert.That(response, Does.Contain("ErrorMessage"));
            Assert.IsEmpty(_boardController.SelectAll(), "Board with invalid name was incorrectly persisted.");
        }

        // =================================================================================
        // DeleteBoard Tests
        // =================================================================================

        [Test]
        public void DeleteBoard_ByOwner_ShouldRemoveFromDatabase()
        {
            // Arrange
            string boardName = "BoardToDelete";
            _service.CreateBoard(_ownerEmail, boardName);
            var board = _boardController.SelectAll().First();
            _service.JoinBoard(_memberEmail, board.ID);

            // Act
            _service.DeleteBoard(_ownerEmail, boardName);

            // Assert
            Assert.IsNull(_boardController.SelectBoard(board.ID), "Board was not removed from the database.");
            Assert.IsEmpty(_boardMembersController.SelectAll().Where(m => m.BoardID == board.ID), "Board memberships were not cleared.");
        }

        [Test]
        public void DeleteBoard_ByNonOwner_ShouldFailAndNotDelete()
        {
            // Arrange
            string boardName = "ProtectedBoard";
            _service.CreateBoard(_ownerEmail, boardName);
            var board = _boardController.SelectAll().First();

            // Act
            var response = _service.DeleteBoard(_memberEmail, boardName);

            // Assert
            Assert.That(response, Does.Contain("ErrorMessage"));
            Assert.IsNotNull(_boardController.SelectBoard(board.ID), "Board was incorrectly deleted by a non-owner.");
        }

        // =================================================================================
        // Join/Leave Board Tests
        // =================================================================================

        [Test]
        public void JoinAndLeaveBoard_WhenCalled_ShouldUpdateMembershipInDatabase()
        {
            // Arrange
            _service.CreateBoard(_ownerEmail, "Public Board");
            var board = _boardController.SelectAll().First();

            // Act (Join) & Assert (Join)
            _service.JoinBoard(_memberEmail, board.ID);
            var membersAfterJoin = _boardMembersController.SelectAll();
            Assert.IsTrue(membersAfterJoin.Any(m => m.BoardID == board.ID && m.Email == _memberEmail), "Member was not persisted after joining.");

            // Act (Leave) & Assert (Leave)
            _service.LeaveBoard(_memberEmail, board.ID);
            var membersAfterLeave = _boardMembersController.SelectAll();
            Assert.IsFalse(membersAfterLeave.Any(m => m.BoardID == board.ID && m.Email == _memberEmail), "Member was not removed from DB after leaving.");
        }

        [Test]
        public void LeaveBoard_OwnerTriesToLeave_ShouldFail()
        {
            // Arrange
            _service.CreateBoard(_ownerEmail, "Owned Board");
            var board = _boardController.SelectAll().First();

            // Act
            var response = _service.LeaveBoard(_ownerEmail, board.ID);

            // Assert
            Assert.That(response, Does.Contain("ErrorMessage"));
        }

        // =================================================================================
        // Task Management and Permissions Tests
        // =================================================================================

        [Test]
        public void AddTask_ByNonMember_ShouldFail()
        {
            // Arrange
            _service.CreateBoard(_ownerEmail, "Private Board");

            // Act
            var response = _service.AddTask(_nonMemberEmail, "Private Board", "Illegal Task", "d", DateTime.Now);

            // Assert
            Assert.That(response, Does.Contain("ErrorMessage"));
        }

        [Test]
        public void AssignTask_ToNonMember_ShouldFail()
        {
            // Arrange
            _service.CreateBoard(_ownerEmail, "Team Board");
            _service.AddTask(_ownerEmail, "Team Board", "Task 0", "d", DateTime.Now.AddDays(1));

            // Act
            var response = _service.AssignTask(_ownerEmail, "Team Board", 0, 0, _nonMemberEmail);

            // Assert
            Assert.That(response, Does.Contain("ErrorMessage"));
        }
    }
}