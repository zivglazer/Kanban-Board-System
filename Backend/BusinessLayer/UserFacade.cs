using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

[assembly: InternalsVisibleTo("BusinessLayerTests")]

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class UserFacade
    {
        public Action<User>? OnUserAddedCallback { get; set; }
        public Dictionary<string, User> users { get; private set; }
        private static readonly ILog log = Logger.Instance;
        private UserController userController;

        // Initializes a new instance of the UserFacade class.
        public UserFacade()
        {
            users = new Dictionary<string, User>();
            userController = new UserController();
            log.Info("UserFacade initialized.");
        }

        // Loads all users from the database into memory.
        public void LoadUsers()
        {
            users.Clear();

            List<UserDTO> DTOs = userController.SelectAllUsers();
            foreach (UserDTO user in DTOs)
            {
                User toAdd = new User(user);
                users.Add(user.Email, toAdd);
                OnUserAddedCallback?.Invoke(toAdd);
            }
        }

        // Deletes all users from the database.
        public void DeleteUsers()
        {
            userController.DeleteAll();
            this.users.Clear();
        }

        // Registers a new user and adds them to the board.
        public User Register(string email, string password)
        {
            ValidateEmail(email);

            if (users.ContainsKey(email))
                throw new ArgumentException("User already exists.");

            User user = new User(email, password);
            AddUser(user);
            OnUserAddedCallback?.Invoke(user);

            log.Info($"User registered successfully: {email}");
            return user;
        }

        //Logs in the user if the password is correct and the user is not already logged in.
        public User Login(string email, string password)
        {
            if (!users.ContainsKey(email))
                throw new ArgumentException("User does not exist.");

            User user = users[email];
            user.Login(password);
            log.Info($"User logged in successfully: {email}");
            return user;
        }

        // Logs out the user if he is currently logged in.
        public void Logout(string email)
        {
            if (!users.ContainsKey(email))
                throw new ArgumentException("User does not exist.");

            User user = users[email];
            user.Logout();
        }

        // Adds a user to the dictionary of users.
        private void AddUser(User toAdd)
        {
            users.Add(toAdd.Email, toAdd);
            log.Info($"User added to collection: {toAdd.Email}");
        }

        // Validates the format of the provided email.
        private void ValidateEmail(string email)
        {
            Regex EmailRegex = new Regex(@"(([^<>()\[\]\\.,;:\s@""]+(\.[^<>()\[\]\\.,;:\s@""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))", RegexOptions.Compiled);
            if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
            {
                log.Error("Email cannot be null, emoty or include white spaces.");
                throw new ArgumentException("Email cannot be null, empty or include white spaces.");
            }
        }

        // Sets the callback to be invoked when a user is added.
        internal void SetAddUserCallback(Action<User> callback)
        {
            this.OnUserAddedCallback = callback;
        }
    }
}
