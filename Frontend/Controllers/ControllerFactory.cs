// File: Frontend/Controllers/ControllerFactory.cs

using IntroSE.Kanban.Backend.ServiceLayer.Services;
// No longer need using Frontend.Model for BackendController here

namespace Frontend.Controllers
{
    internal class ControllerFactory
    {
        public static ControllerFactory Instance { get; } = new ControllerFactory();

        public readonly UserController userController;
        public readonly BoardController boardController;
        public readonly TaskController taskController;

        private ControllerFactory()
        {
            var serviceFactory = new ServiceFactory();
            userController = new UserController(serviceFactory.UserService);
            taskController = new TaskController(serviceFactory.TaskService);

            boardController = new BoardController(serviceFactory.BoardService);
            serviceFactory.LoadData();
        }
    }
}