using System.Text.Json;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using IntroSE.Kanban.Backend.ServiceLayer.Services;

/**
 * Black-box tests for the Service Layer.
 * NUnit is used for convenience; the goal is to validate external behavior,
 * not to perform white-box testing of the Service Layer classes.
 **/

namespace ServiceLayerTests
{
    internal class UserServiceTests
    {
        private BoardFacade boardFacade;
        private UserService userService;
        private UserFacade userFacade;
        private string email;
        private string password;

        [SetUp]
        public void Setup()
        {
            boardFacade = new BoardFacade();
            userFacade = new UserFacade();
            userService = new UserService(userFacade);
            email = "testuser@gmail.com";
            password = "Password123";
        }

        [Test]
        public void Register_TestValidInput()
        {
            // Act
            string responseJson = userService.Register(email, password);
            Response<UserSL> response = JsonSerializer.Deserialize<Response<UserSL>>(responseJson);

            // Assert
            Assert.That(response.ErrorMessage, Is.EqualTo(null));
        }

        [Test]
        public void Register_TestUserAlreadyExists()
        {
            // Arrange
            userService.Register(email, password);

            // Act
            string responseJson = userService.Register(email, password);
            Response<UserSL> response = JsonSerializer.Deserialize<Response<UserSL>>(responseJson);

            // Assert
            Assert.That(response.ErrorMessage, Is.EqualTo($"User already exists."));
        }

        [Test]
        public void Login_TestValidInput()
        {
            // Arrange
            userService.Register(email, password);
            userService.Logout(email);
            // Act
            string responseJson = userService.Login(email, password);
            Response<string> response = JsonSerializer.Deserialize<Response<string>>(responseJson);

            // Assert
            Assert.That(response.ErrorMessage, Is.Null);
        }

        [Test]
        public void Login_TestInvalidPassword()
        {
            // Arrange
            userService.Register(email, password);
            userService.Logout(email);
            // Act
            string responseJson = userService.Login(email, "WrongPassword123");
            Response<UserSL> response = JsonSerializer.Deserialize<Response<UserSL>>(responseJson);

            // Assert
            Assert.That(response.ErrorMessage, Is.EqualTo("Invalid password."));
        }

        [Test]
        public void Login_TestUserDoesNotExist()
        {
            // Act
            string responseJson = userService.Login("nonexistent@gmail.com", password);
            Response<object> response = JsonSerializer.Deserialize<Response<object>>(responseJson);

            // Assert
            Assert.That(response.ErrorMessage, Is.EqualTo("User does not exist."));
        }

        [Test]
        public void Logout_TestValidInput()
        {
            // Arrange
            userService.Register(email, password);

            // Act
            string responseJson = userService.Logout(email);
            Response<UserSL> response = JsonSerializer.Deserialize<Response<UserSL>>(responseJson);

            // Assert
            Assert.That(response.ErrorMessage, Is.Null);
        }

        [Test]
        public void Logout_TestUserNotLoggedIn()
        {
            // Arrange
            userService.Register(email, password);
            userService.Logout(email);

            // Act
            string responseJson = userService.Logout(email);
            Response<UserSL> response = JsonSerializer.Deserialize<Response<UserSL>>(responseJson);

            // Assert
            Assert.That(response.ErrorMessage, Is.EqualTo("User is not logged in."));
        }
    }
}
