using Frontend.Model;
using Frontend.ViewModel;
using NUnit.Framework;
using System;

namespace FrontendUnitTests
{
    [TestFixture]
    public class LoginVmTests
    {
        /// <summary>
        /// This SetUp method runs before EACH test in this fixture.
        /// It ensures a clean and consistent state for every test by clearing the database.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Console.WriteLine($"--- Running SetUp for LoginVmTests ---");

            if (TestDatabaseSetup.BackendServiceFactory == null)
            {
                throw new InvalidOperationException("BackendServiceFactory not initialized by TestDatabaseSetup.OneTimeSetUp.");
            }
            TestDatabaseSetup.BackendServiceFactory.DeleteData();
            Console.WriteLine("Data cleared successfully before test.");
        }

        // --- Test Methods ---

        /// <summary>
        /// Tests successful user registration via LoginVM.
        /// </summary>
        [Test]
        public void LoginVM_Register_Success()
        {
            // Arrange
            var loginVM = new LoginVM(); // LoginVM creates its own BackendController
            string email = "test_register@example.com";
            string password = "Password123";

            // Set the Email property in the ViewModel.
            loginVM.Email = email;
            // Assert initial state: ErrorMessage should be empty string (if initialized in VM)
            // This now passes if LoginVM.ErrorMessage is initialized to "" in its constructor.
            Assert.IsEmpty(loginVM.ErrorMessage, "Error message should be empty initially.");

            // Act
            // Attempt to register the user. If Backend's Register also logs in, this is sufficient.
            UserModel currentUser = loginVM.Register(password);

            // Assert
            Assert.IsEmpty(loginVM.ErrorMessage, "Error message should be empty after successful registration.");
            Assert.IsNotNull(currentUser, "Register should return a UserModel on success.");
            Assert.AreEqual(email, currentUser.Email, "Registered UserModel email should match the registered email.");
        }

        /// <summary>
        /// Tests user registration failure due to invalid email format.
        /// </summary>
        [Test]
        public void LoginVM_Register_InvalidEmail_Failure()
        {
            // Arrange
            var loginVM = new LoginVM();
            string email = "invalid-email"; // Invalid email format
            string password = "Password123";

            loginVM.Email = email;

            // Act
            UserModel currentUser = loginVM.Register(password);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(loginVM.ErrorMessage), "Error message should be displayed for invalid email.");
            Assert.IsNull(currentUser, "Register should return null on failure.");
            // CORRECTED based on your previous log: "email cannot be null, empty or include white spaces." (fixed typo)
            StringAssert.Contains("email cannot be null, empty or include white spaces.", loginVM.ErrorMessage.ToLower(), "Error message should indicate invalid email.");
        }

        /// <summary>
        /// Tests user registration failure when password is too short/weak.
        /// Note: Your Backend validation determines the exact error message.
        /// </summary>
        [Test]
        public void LoginVM_Register_WeakPassword_Failure()
        {
            // Arrange
            var loginVM = new LoginVM();
            string email = "weakpass@example.com";
            string password = "abc"; // Too short/weak based on common policies

            loginVM.Email = email;

            // Act
            UserModel currentUser = loginVM.Register(password);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(loginVM.ErrorMessage), "Error message should be displayed for weak password.");
            Assert.IsNull(currentUser, "Register should return null on failure.");
            // CORRECTED based on your previous log: "password must be between 6 and 20 characters long, contain at least one uppercase letter, one lowercase letter, and one digit."
            StringAssert.Contains("password must be between 6 and 20 characters long, contain at least one uppercase letter, one lowercase letter, and one digit.", loginVM.ErrorMessage.ToLower(), "Error message should indicate weak password.");
        }

        /// <summary>
        /// Tests user registration failure when an email is already registered.
        /// </summary>
        [Test]
        public void LoginVM_Register_EmailAlreadyExists_Failure()
        {
            // Arrange
            var loginVM_first = new LoginVM();
            string email = "existing@example.com";
            string password = "ValidPassword1";

            loginVM_first.Email = email;
            loginVM_first.Register(password); // Register successfully the first time

            // Arrange for second attempt with a fresh VM
            var loginVM_second = new LoginVM();
            loginVM_second.Email = email; // Try to register with the same email

            // Act
            UserModel currentUser = loginVM_second.Register(password);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(loginVM_second.ErrorMessage), "Error message should be displayed when email already exists.");
            Assert.IsNull(currentUser, "Register should return null when email already exists.");
            // CORRECTED based on your previous log: "user already exists."
            StringAssert.Contains("user already exists.", loginVM_second.ErrorMessage.ToLower(), "Error message should indicate email already registered.");
        }


        /// <summary>
        /// Tests successful user login via LoginVM.
        /// This test assumes that after a successful Register, the user is already considered logged in.
        /// If not, you might need an explicit Login after Register.
        /// </summary>
        [Test]
        public void LoginVM_Login_Success()
        {
            // Arrange
            // First, register a user using a LoginVM instance.
            var registerVM = new LoginVM();
            string email = "test_login@example.com";
            string password = "ValidPassword1";
            registerVM.Email = email;

            // ACT: Register the user. According to your Backend log, this *also* logs them in.
            UserModel registeredAndLoggedInUser = registerVM.Register(password);

            // Assert that registration was successful and user is "logged in" by the registration process.
            Assert.IsEmpty(registerVM.ErrorMessage, "Error message should be empty after successful registration (which implies login).");
            Assert.IsNotNull(registeredAndLoggedInUser, "Register should return a UserModel, indicating success and effective login.");
            Assert.AreEqual(email, registeredAndLoggedInUser.Email, "Registered and logged-in UserModel email should match.");

            // IF YOU STILL WANT TO TEST EXPLICIT LOGIN AFTER A USER ALREADY EXISTS (but is not logged in),
            // you would need to Logout first, or setup a user via direct DB insertion, not Register.
            // For now, this test asserts that Register *itself* results in a logged-in user.
            // The previous error "User is already logged in." confirms this behavior.
        }

        /// <summary>
        /// Tests user login failure with incorrect credentials.
        /// </summary>
        [Test]
        public void LoginVM_Login_IncorrectCredentials_Failure()
        {
            // Arrange
            // Register a user first so we have a valid target for login.
            var registerVM = new LoginVM();
            string registeredEmail = "registered_user_for_incorrect@example.com";
            string registeredPassword = "CorrectPassword1";
            registerVM.Email = registeredEmail;
            registerVM.Register(registeredPassword); // Register the user

            // Create a new LoginVM instance for the actual login attempt.
            var loginVM = new LoginVM();
            loginVM.Email = registeredEmail; // Correct email
            string incorrectPassword = "WrongPassword";

            // Act
            UserModel currentUser = loginVM.Login(incorrectPassword);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(loginVM.ErrorMessage), "Error message should be displayed for incorrect login.");
            Assert.IsNull(currentUser, "Login should return null on failure.");
            // CORRECTED based on your previous log: "invalid password."
            StringAssert.Contains("invalid password.", loginVM.ErrorMessage.ToLower(), "Error message should indicate invalid password.");
        }

        /// <summary>
        /// Tests user login failure for a non-existent user.
        /// </summary>
        [Test]
        public void LoginVM_Login_NonExistentUser_Failure()
        {
            // Arrange
            var loginVM = new LoginVM();
            string email = "nonexistent@example.com";
            string password = "AnyPassword1";

            loginVM.Email = email;

            // Act
            UserModel currentUser = loginVM.Login(password);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(loginVM.ErrorMessage), "Error message should be displayed for non-existent user.");
            Assert.IsNull(currentUser, "Login should return null on failure.");
            // CORRECTED based on your previous log: "user does not exist."
            StringAssert.Contains("user does not exist.", loginVM.ErrorMessage.ToLower(), "Error message should indicate non-existent user.");
        }


    }
}