using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

[assembly: InternalsVisibleTo("BusinessLayerTests")]

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class Board
    {
        private const int BACKLOG = 0;
        private const int IN_PROGRESS = 1;
        private const int DONE = 2;

        private static readonly ILog log = Logger.Instance;
        private BoardDTO dto;

        public int BoardID;
        public string Name { get; private set; }
        public string Owner { get; private set; }
        public Column[] States { get; private set; }
        public int TaskCount { get; private set; }

        // Initializes a new instance of the Board class.
        public Board(int id, String name, String Owner)
        {
            this.BoardID = id;
            this.Name = name;
            this.States = new Column[3];
            this.Owner = Owner;

            for (int i = 0; i < 3; i++)
                this.States[i] = new Column(i, BoardID);
            this.TaskCount = 0;
            this.dto = new BoardDTO(BoardID, Name, Owner);
            this.dto.Insert();
            log.Info($"Board '{name}' created successfully.");
        }

        // Initializes a new instance of the Board class from a BoardDTO.
        public Board(BoardDTO bdto)
        {
            this.dto = bdto;
            this.BoardID = bdto.ID;
            this.Name = bdto.Name;
            this.Owner = bdto.Owner;
            LoadData();
            log.Info($"Board '{Name}' loaded successfully.");
        }

        // Loads all board data from the data source.
        private void LoadData()
        {
            LoadColumns();
            LoadTasks();
            SetTaskCount();
            log.Info($"Board '{Name}' data loaded successfully.");
        }

        // Sets the task count to the next available task ID.
        private void SetTaskCount()
        {
            int maxID = -1;
            foreach (Column col in this.States)
            {
                if(col.Tasks.Count > 0)
                    maxID = Math.Max(TaskCount, col.Tasks.Keys.Max());
            }
            TaskCount = maxID + 1;
        }

        // Loads all columns for the board.
        private void LoadColumns()
        {
            this.States = new Column[3];
            List <ColumnDTO> statesDTO = new ColumnController().SelectBoardColumns(BoardID);
            foreach (ColumnDTO column in statesDTO) 
            {
                States[column.ColumnIndex] = new Column(column);
            }
            log.Info($"Board '{Name}' columns loaded successfully.");
        }

        // Loads all tasks for the board.
        private void LoadTasks()
        {
            List<TaskDTO> tDTO = new TaskController().SelectByBoardID(BoardID);
            foreach (TaskDTO taskDTO in tDTO)
            {
                Task t = new Task(taskDTO);
                States[taskDTO.State].AddTask(t);
            }
            log.Info($"Board '{Name}' tasks loaded successfully.");
        }


        /// <summary>
        /// Adds a new task to the "To Do" column.
        /// </summary>
        /// <param name="title">The title of the task.</param>
        /// <param name="description">The description of the task.</param>
        /// <param name="dueDate">The due date of the task.</param>
        /// <returns>The new created Task if succesful</returns>
        public Task AddTask(string title, string description, DateTime dueDate)
        {
            log.Info($"Adding task to board '{Name}'");
            try
            {
                EnsureDestinationColumnHasCapacity(0);// Ensures the backlog column has capacity for a new task.
            }
            catch (InvalidOperationException e)
            {
                log.Error($"Failed to add task to board '{Name}': {e.Message}");
                throw;
            }


            Task toAdd = new Task(BoardID,TaskCount, DateTime.Now, dueDate, title, description);
            this.States[0].AddTask(toAdd);
            TaskCount++;

            log.Info($"Added task to board: {Name}");
            return toAdd;
        }


        /// <summary>
        /// Moves a task from one column to the next.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="taskID"></param>
        /// <returns>True if the task was successfully moved.</returns>
        public void AdvanceTask(int columnIndex, int taskID)
        {
            log.Info($"Moving task {taskID} from column {columnIndex} to column {columnIndex + 1}");

            EnsureColumnIndexIsValid(columnIndex);
            EnsureTaskCanBeAdvancedFrom(columnIndex);
            EnsureDestinationColumnHasCapacity(columnIndex + 1);

            Task taskToAdvance = States[columnIndex].RemoveTask(taskID);
            taskToAdvance.UpdateState();
            States[columnIndex + 1].AddTask(taskToAdvance);

            log.Info($"Task {taskID} moved successfully");
        }

        /// <summary>
        /// Updates the description of a task in the specified column.
        /// </summary>
        /// <param name="columnIndex">The index of the column containing the task.</param>
        /// <param name="taskID">The ID of the task to update.</param>
        /// <param name="description">The new description for the task.</param>
        /// <returns>The Updated Task if successful</returns>
        public Task UpdateTaskDescription(int columnIndex, int taskID, string description)
        {
            log.Info($"Attempting to update description of task {taskID}");
            
            EnsureColumnIndexIsValid(columnIndex);
            EnsureTaskIsNotInDoneColumn(columnIndex);


            Task task = States[columnIndex].UpdateTaskDescription(taskID, description);
            log.Info($"Updated description of task {taskID}");
            return task;
        }


        /// <summary>
        /// Updates the title of a task in the specified column.
        /// </summary>
        /// <param name="columnIndex">The index of the column containing the task.</param>
        /// <param name="taskID">The ID of the task to update.</param>
        /// <param name="title">The new title for the task.</param>
        /// <returns>The updated Task if successful.</returns>
        public Task UpdateTaskTitle(int columnIndex, int taskID, string title)
        {
            log.Info($"Attempting to update title of task {taskID}");

            EnsureColumnIndexIsValid(columnIndex);
            EnsureTaskIsNotInDoneColumn(columnIndex);

            Task task =  States[columnIndex].UpdateTaskTitle(taskID, title);
            log.Info($"Updated title of task {taskID}");
            return task;
        }

        /// <summary>
        /// Updates the due date of a task in the specified column.
        /// </summary>
        /// <param name="columnIndex">The index of the column containing the task.</param>
        /// <param name="taskID">The ID of the task to update.</param>
        /// <param name="dueDate">The new due date for the task.</param>
        /// <returns>The updated Task if successful.</returns>
        public Task UpdateTaskDueDate(int columnIndex, int taskID, DateTime dueDate)
        {
            log.Info($"Attempting to update due date of task {taskID}");

            EnsureColumnIndexIsValid(columnIndex);
            EnsureTaskIsNotInDoneColumn(columnIndex);

            Task task = States[columnIndex].UpdateTaskDueDate(taskID, dueDate);

            log.Info($"Updated due date of task {taskID}");
            return task;
        }

        /// <summary>
        /// Gets the task limit for the specified column.
        /// </summary>
        /// <param name="columnIndex">The index of the column.</param>
        /// <returns>The task limit of the column</returns>
        public int GetColumnLimit(int columnIndex)
        {
            log.Info($"Attempting to get the limit of column at index {columnIndex}");
            EnsureColumnIndexIsValid(columnIndex);
            return States[columnIndex].GetColumnLimit();
        }


        /// <summary>
        /// Gets the name of the column based on its index.
        /// </summary>
        /// <param name="columnIndex">The index of the column.</param>
        /// <returns>The name of the column if successful that represnts the state.</returns>
        public string GetColumnName(int columnIndex)
        {
            log.Info($"Getting name for column {columnIndex}");
            EnsureColumnIndexIsValid(columnIndex);

            return columnIndex switch
            {
                0 => "Backlog",
                1 => "In Progress",
                2 => "Done",
            };
        }


        // Gets the column at the specified index.
        public Column GetColumn(int columnIndex)
        {
            log.Info($"Attempting to get column at index {columnIndex}");
            EnsureColumnIndexIsValid(columnIndex);
            return States[columnIndex];
        }

        /// <summary>
        /// Sets a task limit for the specified column.
        /// </summary>
        /// <param name="columnIndex">The index of the column.</param>
        /// <param name="limit">The new task limit for the column.</param>
        /// <returns>True if the limit set successfully otherwise, false.</returns>
        public void LimitColumn(int columnIndex, int limit)
        {
            log.Info($"Setting limit {limit} for column {columnIndex}");
            EnsureColumnIndexIsValid(columnIndex);

            States[columnIndex].LimitColumn(limit);
            log.Info($"Succesfuly set limit {limit} for column {columnIndex}");
        }

        // Transfers ownership of the board to a new owner.
        public void TransferOwnership(string newOwner)
        {
            if (Owner == newOwner)
                throw new InvalidOperationException("User is already the owner of the board.");
            this.Owner = newOwner;
            this.dto.Owner = newOwner;
            log.Info($"Ownership of board '{Name}' transferred to {newOwner}.");
        }

        // Deletes the board and all its columns.
        public void Delete()
        {
            for (int i = 0; i < States.Length; i++)
            {
                States[i].Delete();
            }
            dto.Delete();
            log.Info($"Board '{Name}' deleted successfully.");
        }

        // Gets a task by its ID and column index.
        public Task GetTask(int taskID, int columnIndex)
        {
            EnsureColumnIndexIsValid(columnIndex);
            return States[columnIndex].GetTask(taskID);
        }

        // Assigns a task to a user.
        public void AssignTask(string assigner,string emailAssignee, int taskID, int columnIndex)
        {
            EnsureColumnIndexIsValid(columnIndex);
            EnsureTaskIsNotInDoneColumn(columnIndex);

            Task toAssign = States[columnIndex].GetTask(taskID);

            if (toAssign.Assignee != Task.UNASSIGNED && toAssign.Assignee != assigner)
            {
                throw new InvalidOperationException($"{assigner} is not the assigner of this task.");
            }

            toAssign.AssignTask(emailAssignee);
            log.Info($"Task {taskID} assigned to {emailAssignee} by {assigner} in column {columnIndex}.");
        }

        // Unassigns all tasks from a user.
        public void UnassignTasks(string email)
        {
            foreach (Column state in States)
            {
               foreach(Task task in state.Tasks.Values)
               {
                    if(task.Assignee == email)
                        task.Unassign();
               }
            }
            log.Info($"All tasks unassigned from {email} in board '{Name}'.");
        }

        // Ensures the column index is valid.
        private void EnsureColumnIndexIsValid(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= States.Length)
            {
                log.Error($"Invalid column index provided: {columnIndex}. Must be between 0 and {States.Length - 1}.");
                throw new ArgumentOutOfRangeException("Column index is out of bounds.");
            }
        }

        // Ensures a task can be advanced from the specified column.
        private void EnsureTaskCanBeAdvancedFrom(int columnIndex)
        {
            // The 'DONE' constant is assumed to be the index of the last column.
            if (columnIndex >= DONE)
            {
                log.Warn($"Attempted to advance a task from the final column (index: {columnIndex}). This is not allowed.");
                throw new InvalidOperationException("Cannot advance a task from the 'done' column.");
            }
        }

        // Ensures the destination column has capacity for a new task.
        private void EnsureDestinationColumnHasCapacity(int sourceColumnIndex)
        {
            int destinationIndex = sourceColumnIndex;
            Column destinationColumn = States[destinationIndex];

            // Assuming the Column class has a helper method to check its capacity.
            // This encapsulates the logic of 'limit != -1 && taskCount >= limit'.
            if (destinationColumn.IsColumnFull())
            {
                log.Error("Cannot advance task, column is full.");
                throw new InvalidOperationException($"Cannot advance task, column is full.");
            }
        }

        // Ensures a task is not in the 'done' column.
        private void EnsureTaskIsNotInDoneColumn(int columnIndex)
        {
            if (columnIndex == DONE)
            {
                log.Warn($"Attempted to modify a task that is already in the 'done' column (index: {columnIndex}).");
                throw new InvalidOperationException("Tasks in the 'done' column cannot be modified.");
            }
        }
    }
}