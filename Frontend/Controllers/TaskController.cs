using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using IntroSE.Kanban.Backend.ServiceLayer.Services;
using System;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class TaskController
    {
        private readonly TaskService taskService;

        internal TaskController(TaskService taskService)
        {
            this.taskService = taskService;
        }

        private void HandleError<T>(Response<T> response)
        {
            if (response.ErrorOccured) throw new Exception(response.ErrorMessage);
        }

        public void AddTask(string email, string boardName, string title, string description, DateTime dueDate)
        {
            var response = JsonSerializer.Deserialize<Response<string>>(taskService.AddTask(email, boardName, title, description, dueDate));
            HandleError(response);
        }

        public void AdvanceTask(string email, string boardName, int columnIndex, int taskId)
        {
            var response = JsonSerializer.Deserialize<Response<string>>(taskService.AdvanceTask(email, boardName, columnIndex, taskId));
            HandleError(response);
        }
        public void AssignTask(string email, string boardName, int columnIndex, int taskId, string emailAssignee)
        {
            var response = JsonSerializer.Deserialize<Response<string>>(taskService.AssignTask(email, boardName, columnIndex, taskId, emailAssignee));
            HandleError(response);
        }
    }
}

