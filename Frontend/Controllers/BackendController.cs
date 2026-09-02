using Frontend.Controllers;

namespace Frontend.Model
{
    public class BackendController
    {
        public UserController UserController { get; private set; }
        public BoardController BoardController { get; private set; }
        public TaskController TaskController { get; private set; }

        public BackendController()
        {
            var factory = ControllerFactory.Instance;
            this.UserController = factory.userController;
            this.BoardController = factory.boardController;
            this.TaskController = factory.taskController;
        }
    }
}


