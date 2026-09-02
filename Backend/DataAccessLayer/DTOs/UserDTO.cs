
using System.Collections.Generic;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class UserDTO : DTO
    {

        private string _password;
        public string Email { get; set; }

        public string EmailColumnName = "Email";
        public string PasswordColumnName = "Password";
        public UserDTO(string email, string password)
        {
            Email = email;
            this._password = password;

            controller = new UserController();
        }

        public string Password
        {
            get => _password;
            set { _password = value; controller.Update(this); }
        }

        public override Dictionary<string, object> PrimaryKey => new Dictionary<string, object>
        {
            { EmailColumnName, Email }
        };  

        public override Dictionary<string, object> ToColumnValuePairs()
        {
            return new Dictionary<string, object>
            {
                { EmailColumnName, Email },
                { PasswordColumnName, Password }
            };
        }
    }

}
