using System;
using System.IO;
using System.Reflection;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using log4net;
using log4net.Config;
namespace IntroSE.Kanban.Backend.ServiceLayer.Services
{
    public class ServiceFactory
    {
        private static readonly ILog log = Logger.Instance;


        public UserService UserService { get; private set; }
        public BoardService BoardService { get; private set; }
        public TaskService TaskService { get; private set; }

        public ServiceFactory()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            BoardFacade boardFacade = new BoardFacade();
            UserFacade userFacade = new UserFacade();

            userFacade.SetAddUserCallback(boardFacade.AddNewUser);

            UserService = new UserService(userFacade);
            BoardService = new BoardService(boardFacade);
            TaskService = new TaskService(boardFacade);
            log.Info("ServiceFactory initialized successfully");
        }

        // Loads all board data from the data source.
        public string LoadData()
        {
            log.Info($"Trying to load data");

            Response<string> response;
            try
            {
                UserService.LoadData();
                BoardService.LoadData();
                response = new Response<string>();
                log.Info($"Data loaded succesfully");
            }
            catch (Exception ex)
            {
                log.Error($"Data load failed. {ex.Message}");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();

        }

        // Deletes all board data from the data source.
        public string DeleteData()
        {
            log.Info($"Trying to delete data");

            Response<string> response;
            try
            {
                UserService.DeleteData();
                BoardService.DeleteData();
                response = new Response<string>();
                log.Info($"Data Deletion succesfully");
            }
            catch (Exception ex)
            {
                log.Error($"Data Deletion failed. {ex.Message}");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }
    }
}
