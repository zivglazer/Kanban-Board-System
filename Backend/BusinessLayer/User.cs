using System;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

[assembly: InternalsVisibleTo("BusinessLayerTests")]

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class User
    {
        public string Email { get; private set; }
        private string _password;
        private static readonly ILog log = Logger.Instance;
        private UserDTO userDTO;
        public bool LoggedIn { get; private set; }

        // Constructor for the User class. Initializes the user with a UserDTO object.
        public User(UserDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException("DTO is null");
          
            Email = dto.Email;
            _password = dto.Password;
            userDTO = dto;
            LoggedIn = false; 
            log.Info($"User '{Email}' loaded from data layer.");
        }

        // Constructor for the User class. Initializes the user with an email and password.
        public User(string email, string password)
        {
            log.Info($"Creating user with email: {email}");
            ValidatePassword(password);

            Email = email;
            _password = password;
            LoggedIn = true;
            userDTO = new UserDTO(email, password);
            userDTO.Insert();
            log.Info($"User '{email}' was created");
        }

        //Logs in the user if the password is correct and the user is not already logged in.
        public void Login(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                log.Error("Password cannot be null or empty.");
                throw new ArgumentException("Password cannot be null or empty.");
            }

            if (!PasswordAuthentication(password))
            {
                log.Warn("Invalid password provided for login.");
                throw new ArgumentException("Invalid password.");
            }

            if (LoggedIn)
            {
                log.Warn($"User '{Email}' is already logged in.");
                throw new InvalidOperationException("User is already logged in.");
            }

            ChangeLoggedIn();
            log.Info($"User '{Email}' logged in successfully.");
        }

        // Logs out the user if he is currently logged in.
        public void Logout()
        {
            if (LoggedIn)
            {
                log.Info($"User '{Email}' logged out successfully.");
                ChangeLoggedIn();
            }
            else
            {
                log.Warn($"User '{Email}' is not logged in.");
                throw new InvalidOperationException("User is not logged in.");
            }
        }

        // Toggles the user's logged-in status.
        public void ChangeLoggedIn()
        {
            log.Debug($"Changing logged-in status for user '{Email}'.");
            LoggedIn = !LoggedIn;
        }

        //Checks if the provided password matches the stored password.
        public bool PasswordAuthentication(string password)
        {
            bool isEqual = this._password.Equals(password);
            log.Debug($"Authenticating password for user '{Email}' : {isEqual}");
            return isEqual;
        }

        //Checks if user is logged in.
        public bool IsLoggedIn()
        {
            return LoggedIn;
        }


        /////////////////////////
        /// Private Functions ///
        /////////////////////////

        // Validates that the password meets length and complexity requirements.
        private void ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                log.Error("Password cannot be null or empty.");
                throw new ArgumentException("Password cannot be null or empty.");
            }

            bool validLength = password.Length >= 6 && password.Length <= 20;
            bool validCharacters = password.Any(char.IsLower) && password.Any(char.IsUpper) && password.Any(char.IsDigit);

            if (!validLength || !validCharacters)
            {
                log.Error("Password validation failed.");
                throw new ArgumentException("Password must be between 6 and 20 characters long, contain at least one uppercase letter, one lowercase letter, and one digit.");
            }
            log.Info($"Password validation succeded for user '{Email}'.");
        }
    }
}
