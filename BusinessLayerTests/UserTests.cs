using System;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using NUnit.Framework;

namespace BusinessLayerTests
{
    [TestFixture]
    public class UserTests
    {
        private User user;
        private readonly string validEmail = "test@gmail.com";
        private readonly string validPassword = "Test123";

        [SetUp]
        public void Setup()
        {
            // Create a fresh user for each test
            user = new User(validEmail, validPassword);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up by deleting the test user's DTO from the database
            try
            {
                UserDTO dto = new UserDTO(validEmail, validPassword);
                dto.Delete();
            }
            catch
            {
                // If deletion fails, we can ignore it for tests
            }
        }

        [Test]
        public void Constructor_WithEmailAndPassword_CreatesUserAndPersistsToDatabase()
        {
            // Arrange & Act - User is created in Setup

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Email, Is.EqualTo(validEmail));
            Assert.That(user.IsLoggedIn(), Is.True, "New users should be logged in by default");

            // Test persistence by creating a new facade and checking if user exists
            var userFacade = new UserFacade();
            userFacade.LoadUsers();
            Assert.That(userFacade.users.ContainsKey(validEmail), Is.True, "User should be persisted in database");
        }

        [Test]
        public void Constructor_WithUserDTO_CreatesUserFromPersistedData()
        {
            // Arrange - Create DTO (this would normally come from database)
            UserDTO dto = new UserDTO(validEmail, validPassword);

            // Act
            var userFromDTO = new User(dto);

            // Assert
            Assert.That(userFromDTO, Is.Not.Null);
            Assert.That(userFromDTO.Email, Is.EqualTo(validEmail));
            Assert.That(userFromDTO.IsLoggedIn(), Is.False, "Users created from DTO should not be logged in by default");
        }

        [TestCase("less6")] // shorter than 6
        [TestCase("longerthanlongerthan20")] // longer than 20
        [TestCase("s123456")] // No uppercase
        [TestCase("NoDigits")] // no digits
        public void Constructor_InvalidPassword_ThrowsException(string password)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new User(validEmail, password));
            Assert.That(ex.Message, Is.EqualTo("Password must be between 6 and 20 characters long, contain at least one uppercase letter, one lowercase letter, and one digit."));
        }

        [Test]
        public void Login_ValidPassword_UserIsLoggedIn()
        {
            // Arrange
            user.Logout(); // Logout first to test login

            // Act
            user.Login(validPassword);

            // Assert
            Assert.That(user.IsLoggedIn(), Is.True);
        }

        [Test]
        public void Login_EmptyPassword_ThrowsException()
        {
            // Arrange
            user.Logout();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => user.Login(""));
            StringAssert.Contains("Password cannot be null or empty", ex.Message);
        }

        [Test]
        public void Login_InvalidPassword_ThrowsException()
        {
            // Arrange
            user.Logout();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => user.Login("WrongPass123"));
            Assert.That(ex.Message, Is.EqualTo("Invalid password."));
        }

        [Test]
        public void Login_AlreadyLoggedIn_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => user.Login(validPassword));
            Assert.That(ex.Message, Is.EqualTo("User is already logged in."));
        }

        [Test]
        public void Logout_WhenLoggedIn_ChangesLoggedInStatusToFalse()
        {
            // Act
            user.Logout();

            // Assert
            Assert.That(user.IsLoggedIn(), Is.False);
        }

        [Test]
        public void Logout_WhenNotLoggedIn_ThrowsException()
        {
            // Arrange
            user.Logout(); // Ensure user is logged out

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => user.Logout());
            Assert.That(ex.Message, Is.EqualTo("User is not logged in."));
        }

        [Test]
        public void ChangeLoggedIn_TogglesLoggedInStatus()
        {
            // Arrange - user is logged in by default
            bool initialState = user.IsLoggedIn();

            // Act
            user.ChangeLoggedIn();

            // Assert
            Assert.That(user.IsLoggedIn(), Is.Not.EqualTo(initialState));

            // Act again
            user.ChangeLoggedIn();

            // Assert
            Assert.That(user.IsLoggedIn(), Is.EqualTo(initialState));
        }

        [Test]
        public void PasswordAuthentication_CorrectPassword_ReturnsTrue()
        {
            // Act
            bool result = user.PasswordAuthentication(validPassword);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void PasswordAuthentication_IncorrectPassword_ReturnsFalse()
        {
            // Act
            bool result = user.PasswordAuthentication("WrongPass123");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsLoggedIn_InitiallyTrue_AfterConstructor()
        {
            // Act & Assert
            Assert.That(user.IsLoggedIn(), Is.True);
        }

        [Test]
        public void IsLoggedIn_ReturnsFalse_AfterLogout()
        {
            // Arrange
            user.Logout();

            // Act & Assert
            Assert.That(user.IsLoggedIn(), Is.False);
        }
    }
}
