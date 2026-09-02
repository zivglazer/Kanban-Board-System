using Frontend.Model;
using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using IntroSE.Kanban.Backend.ServiceLayer.Services;
using System;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class UserController
    {
        private readonly UserService userService;

        internal UserController(UserService userService)
        {
            this.userService = userService;
        }

        private void HandleError<T>(Response<T> response)
        {
            if (response.ErrorOccured) throw new Exception(response.ErrorMessage);
        }

        public string Login(string email, string password)
        {
            var response = JsonSerializer.Deserialize<Response<string>>(userService.Login(email, password));
            HandleError(response);
            return response.ReturnValue;
        }

        public string Register(string email, string password)
        {
            var response = JsonSerializer.Deserialize<Response<string>>(userService.Register(email, password));
            HandleError(response);
            return email;
        }

        public void Logout(string email)
        {
            var response = JsonSerializer.Deserialize<Response<string>>(userService.Logout(email));
            HandleError(response);
        }
    }
}

