using System;
using System.Runtime.CompilerServices;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using log4net;

[assembly: InternalsVisibleTo("ServiceLayerTests")]

namespace IntroSE.Kanban.Backend.ServiceLayer.Services
{
    public class UserService
    {
        private UserFacade userFacade;
        private static readonly ILog log = Logger.Instance;

        internal UserService(UserFacade userFacade)
        {
            this.userFacade = userFacade;
            log.Info("UserService initialized successfully.");
        }


        /// <summary>
        /// Registers a new user with the provided email and password.
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <param name="pass">The password for the user.</param>
        /// <returns>A JSON string indicating the result of the registration process.</returns>
        public string Register(string email, string password)
        {
            Response<string> response;
            try
            {
                log.Info($"Attempting to register user with email: {email}");
                User ans = userFacade.Register(email, password);
                response = new Response<string>();
            }
            catch (Exception ex)
            {
                log.Error($"Error appread during User {email} Register try");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }


        /// <summary>
        /// Log in user with when he providing the right email and pass.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="pass">The password for the user.</param>
        /// <returns>A JSON string indicating the result of the login process.</returns>
        public string Login(string email, string pass)
        {
            Response<string> response;
            try
            {
                log.Info($"Attempting to log in");
                User loggedInUser = userFacade.Login(email, pass);
                response = new Response<string>(loggedInUser.Email, null);
            }
            catch (Exception ex)
            {
                log.Error($"Error appread during User {email} login try");
                response = new Response<string>(ex.Message);
            }

            return response.ToJson();
        }

        /// <summary>
        /// Logs out the user with the provided email.
        /// </summary>
        /// <param name="">The email address of the user to log out.</param>
        /// <returns>A JSON string indicating the result of the logout process.</returns>
        public string Logout(string email)
        {
            Response<string> response;
            try
            {
                log.Info($"User {email} trying to log out");
                userFacade.Logout(email);
                log.Info($"User {email} logged out succesfuly");
                response = new Response<string>();
            }
            catch (Exception ex)
            {
                log.Error($"Error appread during {email} logout");
                response = new Response<string>(ex.Message);
            }

            return response.ToJson();
        }

        // Loads all board data from the data source.
        public string LoadData()
        {
            Response<string> response;
            try
            {
                log.Info($"Trying to load users");
                userFacade.LoadUsers();
                log.Info($"Users loaded succesfuly");
                response = new Response<string>();
            }
            catch (Exception ex)
            {
                log.Error($"Error laoding users");
                response = new Response<string>(ex.Message);
            }

            return response.ToJson();
        }

        // Deletes all board data from the data source.
        public string DeleteData()
        {
            Response<string> response;
            try
            {
                log.Info($"Trying to delete users");
                userFacade.DeleteUsers();
                log.Info($"Users deleted succesfuly");
                response = new Response<string>();
            }
            catch (Exception ex)
            {
                log.Error($"Error delete users");
                response = new Response<string>(ex.Message);
            }

            return response.ToJson();
        }
    }
}


