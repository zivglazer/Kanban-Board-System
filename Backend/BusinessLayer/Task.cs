using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

[assembly: InternalsVisibleTo("BusinessLayerTests")]

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class Task
    {

        public const string UNASSIGNED = "unassigned";
        private const int DESCRIPTION_MAX_LENGTH = 300;
        private const int TITLE_MAX_LENGTH = 50;
        private const int BACKLOG = 0;

        private static readonly ILog log = Logger.Instance;
        private TaskDTO dto;

        public int TaskID { get; private set; }
        public DateTime CreationDate { get; private set; }
        public DateTime DueDate { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Assignee { get; private set; }

        // Initializes a new instance of the Task class from a TaskDTO.
        public Task(TaskDTO tDTO)
        {
            this.dto = tDTO;
            this.TaskID = tDTO.Id;
            this.CreationDate = tDTO.CreationDate;
            this.DueDate = tDTO.DueDate;
            this.Title = tDTO.Title;
            this.Description = tDTO.Description;
            this.Assignee = tDTO.Assignee;
        }

        // Initializes a new instance of the Task class with specified parameters.
        public Task(int boardID, int taskID, DateTime creationDate, DateTime dueDate, string title, string description)
        {
            log.Info($"Initializing Task with ID: {taskID}");

            ValidateTaskTitle(title);
            ValidateTaskDescription(description);

            CreationDate = creationDate; //For validate Due date
            ValidateDueDate(dueDate);

            TaskID = taskID;
            DueDate = dueDate;
            Title = title;
            Description = description;
            Assignee = UNASSIGNED;
            dto = new TaskDTO(TaskID, boardID, Assignee, BACKLOG, Title, Description, CreationDate, DueDate);
            dto.Insert();
            log.Info($"Task with ID: {taskID} successfully created.");
        }

        // Updates the task's title after validating it.
        public void UpdateTaskTitle(string title)
        {
            log.Info($"Updating title for Task ID: {TaskID}");

            ValidateTaskTitle(title);

            this.Title = title;
            this.dto.Title = title;
            log.Info($"Title updated for Task ID: {TaskID}");
        }

        // Updates the task's description after validating it.
        public void UpdateTaskDescription(string description)
        {
            log.Info($"Updating description for Task ID: {TaskID}");

            ValidateTaskDescription(description);

            this.Description = description;
            this.dto.Description = description;
            log.Info($"Description updated for Task ID: {TaskID}");
        }

        // Updates the task's due date after validating it.
        public void UpdateTaskDueDate(DateTime dueDate)
        {
            log.Info($"Updating due date for Task ID: {TaskID}");

            ValidateDueDate(dueDate);

            this.DueDate = dueDate;
            this.dto.DueDate = dueDate;
            log.Info($"Due date updated for Task ID: {TaskID}");
        }

        // Assigns the task to a new assignee and updates the DTO.
        public void AssignTask(string newAssignee)
        {
            this.Assignee = newAssignee;
            this.dto.Assignee = newAssignee;
        }

        // Unassigns the task from any assignee.
        public void Unassign()
        {
            this.Assignee = UNASSIGNED;
            this.dto.Assignee = UNASSIGNED;
        }

        // Advances the state of the task and updates the DTO.
        public void UpdateState()
        {
            this.dto.State += 1;
        }

        // Validates the task title: must not be null/empty and must be <= 50 characters.
        private void ValidateTaskTitle(string taskTitle)
        {
            if (string.IsNullOrEmpty(taskTitle))
            {
                log.Error("Validation failed:Task title is null or empty.");
                throw new ArgumentException("Task title can't be null or empty.");
            }

            if (taskTitle.Length > TITLE_MAX_LENGTH)
            {
                log.Error("Validation failed: Title is too long.");
                throw new ArgumentException("Title too long.");
            }
        }

        // Validates the task description: must not be null and must be <= 300 characters.
        private void ValidateTaskDescription(string taskDescription)
        {
            if (taskDescription == null)
            {
                log.Error("Validation failed: Description is null.");
                throw new ArgumentException("Description can't be null.");
            }

            if (taskDescription.Length > DESCRIPTION_MAX_LENGTH)
            {
                log.Error("Validation failed: Description is too long.");
                throw new ArgumentException("Description too long.");
            }
        }

        // Validates that the due date is not in the past.
        private void ValidateDueDate(DateTime dueDate)
        {
            if (dueDate < CreationDate)
            {
                log.Error("Validation failed: Due date is invalid.");
                throw new ArgumentException("Due date invalid.");
            }
        }

        //Deletes task from DB
        public void Delete()
        {
            this.dto.Delete();
            log.Info($"Task with ID: {TaskID} has been deleted.");
        }
    }
}
