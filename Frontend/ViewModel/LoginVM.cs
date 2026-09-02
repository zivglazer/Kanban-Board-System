using Frontend.Model;
using System;

namespace Frontend.ViewModel
{
    public class LoginVM : NotifiableObject
    {
        private BackendController controller;
        public string _email;
        public string _password;

        public string Email
        {
            get => _email;
            set
            {
                this._email = value;
                RaisePropertyChanged("Email");
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                this._password = value;
                RaisePropertyChanged("Password");
            }
        }

        private string _errorMessage; public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; RaisePropertyChanged("ErrorMessage"); } }

        public LoginVM()
        {
            this.controller = new BackendController();
            ErrorMessage = "";
        }

        public UserModel Login(string password)
        {
            ErrorMessage = "";
            try
            {
                string loggedInUserEmail = controller.UserController.Login(Email, password);
                return new UserModel(controller, loggedInUserEmail);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return null;
            }
        }

        public UserModel Register(string password)
        {
            ErrorMessage = "";
            try
            {
                string registeredUserEmail = controller.UserController.Register(Email, password);
                return new UserModel(controller, registeredUserEmail);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return null;
            }
        }
    }
}
