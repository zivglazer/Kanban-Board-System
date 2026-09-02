using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using log4net;
using Microsoft.VisualBasic;
using Task = IntroSE.Kanban.Backend.BusinessLayer.Task;

[assembly: InternalsVisibleTo("ServiceLayerTests")]

namespace IntroSE.Kanban.Backend.ServiceLayer.Services
{
    public class TaskService
    {
        private static readonly ILog log = Logger.Instance;


        private readonly BoardFacade boardFacade;

        /// <summary>
        /// Initializes the field of TaskService with the given boardFacade.
        /// </summary>
        /// <param name="bf">The board facade that providing the access to businessLayer</param>
        internal TaskService(BoardFacade bf)
        {
            log.Info($"Initializing TaskService");

            boardFacade = bf;
        }

        /// <summary>
        /// Adds a new task to a specific board for the user.
        /// </summary>
        /// <param name="email">The users email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="title">The title of the new task.</param>
        /// <param name="description">The description of the new task.</param>
        /// <param name="dueDate">The due date of the new task.</param>
        /// <returns>A JSON string representing a Response with the added task.</returns>
        public string AddTask(string email, string boardName, string title, string description, DateTime dueDate)
        {
            log.Info($"User {email} adding to board {boardName} a new task");

            Response<string> response;
            try
            {
                Task ans = boardFacade.AddTask(email, boardName, title, description, dueDate);
                response = new Response<string>();
                log.Info($"New task added succesfully");
            }
            catch (Exception ex)
            {
                log.Error($"Addition failed. {ex.Message}");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }

        /// <summary>
        /// Advances a task to the next column on the board.
        /// </summary>
        /// <param name="email">The users email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="columnIndex">The current column index of the task.</param>
        /// <param name="taskID">The ID of the task to advance.</param>
        /// <returns>A JSON string representing a success message or an error.</returns>
        public string AdvanceTask(string email, string boardName, int columnIndex, int taskID)
        {
            Response<string> response;
            try
            {
                log.Info($"the Assignee {email} advancing task in board  {boardName}");
                boardFacade.AdvanceTask(email, boardName, columnIndex, taskID);
                response = new Response<string>();
                log.Info($"Task advanced succesfully");
            }
            catch (Exception ex)
            {
                response = new Response<string>(ex.Message);
                log.Error($"Task Advance failed. {ex.Message}");
            }
            return response.ToJson();
        }

        /// <summary>
        /// Updates the title of a task.
        /// </summary>
        /// <param name="email">The users email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="columnIndex">The column index of the task.</param>
        /// <param name="taskID">The ID of the task.</param>
        /// <param name="title">The new title for the task.</param>
        /// <returns>A JSON string representing the updated task or an error.</returns>
        public string UpdateTaskTitle(string email, string boardName, int columnIndex, int taskID, string title)
        {
            Response<string> response;
            try
            {
                log.Info($"User {email} updating task title in board  {boardName}");
                Task ans = boardFacade.UpdateTaskTitle(email, boardName, columnIndex, taskID, title);
                response = new Response<string>();
                log.Info($"Title updated succesfully");
            }
            catch (Exception ex)
            {
                response = new Response<string>(ex.Message);
                log.Error($"Title Update failed. {ex.Message}");
            }
            return response.ToJson();
        }

        /// <summary>
        /// Updates the description of a task.
        /// </summary>
        /// <param name="email">The users email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="columnIndex">The column index of the task.</param>
        /// <param name="taskID">The ID of the task to update.</param>
        /// <param name="description">The new description for the task.</param>
        /// <returns>A JSON string representing the updated task or an error.</returns>
        public string UpdateTaskDescription(string email, string boardName, int columnIndex, int taskID, string description)
        {
            Response<string> response;
            try
            {
                log.Info($"User {email} updating task description in board  {boardName}");
                Task ans = boardFacade.UpdateTaskDescription(email, boardName, columnIndex, taskID, description);
                response = new Response<string>();
                log.Info($"Description updated succesfully");
            }
            catch (Exception ex)
            {
                response = new Response<string>(ex.Message);
                log.Error($"Description Update failed. {ex.Message}");
            }
            return response.ToJson();
        }

        /// <summary>
        /// Updates the due date of a task.
        /// </summary>
        /// <param name="email">The users email address.</param>
        /// <param name="boardName">The name of the board.</param>
        /// <param name="columnIndex">The column index of the task.</param>
        /// <param name="taskID">The ID of the task to update.</param>
        /// <param name="dueDate">The new due date for the task.</param>
        /// <returns>A JSON string representing the updated task or an error.</returns>
        public string UpdateTaskDueDate(string email, string boardName, int columnIndex, int taskID, DateTime dueDate)
        {
            Response<string> response;
            try
            {
                log.Info($"User {email} updating task due date in board  {boardName}");
                Task ans = boardFacade.UpdateTaskDueDate(email, boardName, columnIndex, taskID, dueDate);
                response = new Response<string>();
                log.Info($"Duedate updated succesfully");
            }
            catch (Exception ex)
            {
                response = new Response<string>(ex.Message);
                log.Error($"Duedate Update failed. {ex.Message}");
            }
            return response.ToJson();
        }

        /// <summary>
        /// Retrieves all tasks in progress for this user fromm all boards.
        /// </summary>
        /// <param name="email">The users email address.</param>
        /// <returns>A JSON string representing a list of tasks in progress.</returns>
        public string InProgressTasks(string email)
        {
            Response<List<TaskSL>> response;
            try
            {
                log.Info($"User {email} request for all tasks that are in progress");
                List<Task> ans = boardFacade.InProgressTasks(email);
                List<TaskSL> taskSLs = new List<TaskSL>();

                foreach(Task t in ans)
                    taskSLs.Add(new TaskSL(t.TaskID, t.Title, t.Description, t.DueDate, t.CreationDate, t.Assignee));

                response = new Response<List<TaskSL>>(taskSLs, null);
                log.Info($"In progress tasks has been sent succesfully");
            }
            catch (Exception ex)
            {
                response = new Response<List<TaskSL>>(ex.Message);
                log.Error($"Request failed. {ex.Message}");
            }
            return response.ToJson();
        }

        //Assigns the specified task to 'emailAssignee' by user 'email'
        public string AssignTask(string email, string boardName, int columnIndex, int taskID, string emailAssignee)
        {
            Response<string> response;
            try
            {
                log.Info("Attempting to assign task");
                boardFacade.AssignTask(email, boardName, columnIndex, taskID, emailAssignee);
                response = new Response<string>();
                log.Info($"Task Assigned");
            }
            catch (Exception ex)
            {
                response = new Response<string>(ex.Message);
                log.Error($"Request failed. {ex.Message}");
            }
            return response.ToJson();
        }
    }
}
