namespace Frontend.Model
{
    public class UserModel : NotifiableModelObject
    {
        public string Email { get; }
        public UserModel(BackendController controller, string email) : base(controller)
        {
            this.Email = email;
        }
    }
}