namespace IntroSE.Kanban.Backend.ServiceLayer.Entities
{
    public class UserSL
    {
        public string Email { get; set; }

        public UserSL (string email)
        {
            Email = email;
        }
    }
}
