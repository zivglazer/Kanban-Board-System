using System;
using System.Collections.Generic;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using NUnit.Framework;

namespace BusinessLayerTests
{
    [TestFixture]
    internal class UserFacadeTests
    {
        private UserFacade userFacade;
        private readonly string validEmail = "testuser@gmail.com";
        private readonly string validPassword = "Password123";
        private readonly string alternateEmail = "alternate@gmail.com";

        [SetUp]
        public void Setup()
        {
            // Create a fresh UserFacade for each test
            userFacade = new UserFacade();

            // Ensure database is clean before each test
            userFacade.DeleteUsers();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after tests
            userFacade.DeleteUsers();
        }

        [Test]
        public void Register_WithValidInput_CreatesUserAndPersistsToDatabase()
        {
            // Act
            User user = userFacade.Register(validEmail, validPassword);

            // Assert - Check that user is created
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Email, Is.EqualTo(validEmail));
            Assert.That(user.IsLoggedIn(), Is.True, "User should be logged in after registration");

            // Assert - Check that user exists in the in-memory dictionary
            Assert.That(userFacade.users.ContainsKey(validEmail), Is.True, "User should be in the users dictionary");

            // Assert - Check persistence by creating a new facade and loading from database
            var newUserFacade = new UserFacade();
            newUserFacade.LoadUsers();
            Assert.That(newUserFacade.users.ContainsKey(validEmail), Is.True, "User should be persisted in the database");
        }

        [Test]
        public void Register_WithExistingUser_ThrowsException()
        {
            // Arrange
            userFacade.Register(validEmail, validPassword);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => userFacade.Register(validEmail, validPassword));
            Assert.That(ex.Message, Is.EqualTo("User already exists."));
        }

        [Test]
        public void Register_WithInvalidPassword_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => userFacade.Register(validEmail, "short"));
            Assert.That(ex.Message, Is.EqualTo("Password must be between 6 and 20 characters long, contain at least one uppercase letter, one lowercase letter, and one digit."));
        }

        [Test]
        public void Register_WithInvalidEmail_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => userFacade.Register("invalid", validPassword));
            StringAssert.Contains("Email cannot be null", ex.Message);
        }

        [Test]
        public void Login_WithValidCredentials_ReturnsLoggedInUser()
        {
            // Arrange
            userFacade.Register(validEmail, validPassword);
            userFacade.Logout(validEmail); // Ensure user is logged out

            // Act
            User user = userFacade.Login(validEmail, validPassword);

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user.IsLoggedIn(), Is.True, "User should be logged in after login");
        }

        [Test]
        public void Login_WithNonExistentUser_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => userFacade.Login(validEmail, validPassword));
            Assert.That(ex.Message, Is.EqualTo("User does not exist."));
        }

        [Test]
        public void Login_WithIncorrectPassword_ThrowsException()
        {
            // Arrange
            userFacade.Register(validEmail, validPassword);
            userFacade.Logout(validEmail);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => userFacade.Login(validEmail, "WrongPassword123"));
            Assert.That(ex.Message, Is.EqualTo("Invalid password."));
        }

        [Test]
        public void Login_WhenAlreadyLoggedIn_ThrowsException()
        {
            // Arrange
            userFacade.Register(validEmail, validPassword);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => userFacade.Login(validEmail, validPassword));
            Assert.That(ex.Message, Is.EqualTo("User is already logged in."));
        }

        [Test]
        public void Login_PersistenceCheck_LoginAfterReload()
        {
            // Arrange
            userFacade.Register(validEmail, validPassword);
            userFacade.Logout(validEmail);

            // Create a new facade and load users from database
            var newUserFacade = new UserFacade();
            newUserFacade.LoadUsers();

            // Act
            User user = newUserFacade.Login(validEmail, validPassword);

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user.IsLoggedIn(), Is.True, "User should be able to log in after reload");
        }

        [Test]
        public void Logout_WhenLoggedIn_ChangesUserStatus()
        {
            // Arrange
            userFacade.Register(validEmail, validPassword);

            // Act
            userFacade.Logout(validEmail);

            // Assert
            User user = userFacade.users[validEmail];
            Assert.That(user.IsLoggedIn(), Is.False, "User should be logged out after logout");
        }

        [Test]
        public void Logout_WithNonExistentUser_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => userFacade.Logout(validEmail));
            Assert.That(ex.Message, Is.EqualTo("User does not exist."));
        }

        [Test]
        public void Logout_WhenNotLoggedIn_ThrowsException()
        {
            // Arrange
            userFacade.Register(validEmail, validPassword);
            userFacade.Logout(validEmail);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => userFacade.Logout(validEmail));
            Assert.That(ex.Message, Is.EqualTo("User is not logged in."));
        }

        [Test]
        public void DeleteUsers_RemovesAllUsersFromDatabaseAndMemory()
        {
            // Arrange
            userFacade.Register(validEmail, validPassword);
            userFacade.Register(alternateEmail, validPassword);

            // Act
            userFacade.DeleteUsers();

            // Assert - Check in-memory collection
            Assert.That(userFacade.users.Count, Is.EqualTo(0), "In-memory users collection should be empty");

            // Assert - Check database persistence
            var newUserFacade = new UserFacade();
            newUserFacade.LoadUsers();
            Assert.That(newUserFacade.users.Count, Is.EqualTo(0), "Database should contain no users");
        }

        [Test]
        public void LoadUsers_LoadsAllUsersFromDatabase()
        {
            // Arrange
            userFacade.Register(validEmail, validPassword);
            userFacade.Register(alternateEmail, validPassword);

            // Clear in-memory collection without affecting database
            userFacade.users.Clear();

            // Act
            userFacade.LoadUsers();

            // Assert
            Assert.That(userFacade.users.Count, Is.EqualTo(2), "Should load two users from database");
            Assert.That(userFacade.users.ContainsKey(validEmail), Is.True, "Should load first user");
            Assert.That(userFacade.users.ContainsKey(alternateEmail), Is.True, "Should load second user");
        }

        [Test]
        public void UserCallback_IsCalledWhenUserIsAdded()
        {
            // Arrange
            bool callbackCalled = false;
            User callbackUser = null;

            userFacade.SetAddUserCallback((user) => {
                callbackCalled = true;
                callbackUser = user;
            });

            // Act
            User user = userFacade.Register(validEmail, validPassword);

            // Assert
            Assert.That(callbackCalled, Is.True, "Callback should be called");
            Assert.That(callbackUser, Is.EqualTo(user), "Callback should receive the created user");
        }

        [Test]
        public void UserCallback_IsCalledWhenUserIsLoadedFromDatabase()
        {
            // Arrange
            userFacade.Register(validEmail, validPassword);

            // Create new facade
            var newUserFacade = new UserFacade();

            int callbackCount = 0;
            newUserFacade.SetAddUserCallback((user) => {
                callbackCount++;
            });

            // Act
            newUserFacade.LoadUsers();

            // Assert
            Assert.That(callbackCount, Is.EqualTo(1), "Callback should be called once for each loaded user");
        }
    }
}
