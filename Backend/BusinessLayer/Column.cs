using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;

[assembly: InternalsVisibleTo("BusinessLayerTests")]

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class Column
    {
        private const int NOT_LIMITED = -1;
        private ColumnDTO dto;
        public int ColumnIndex { get; private set; }
        public int BoardID { get; private set; }
        public Dictionary<int, Task> Tasks { get; private set; }
        public  int Limit { get; private set; }


        // Initializes a new instance of the Column class from a ColumnDTO.
        public Column(ColumnDTO cdto)
        {
            this.dto = cdto;
            this.BoardID = cdto.BoardID;
            this.ColumnIndex = cdto.ColumnIndex;
            this.Limit = cdto.Limit;
            this.Tasks = new Dictionary<int, Task>();
        }

        // Initializes a new instance of the Column class with a column index and board ID.
        public Column(int columnIndex, int boardID)
        {
            this.ColumnIndex = columnIndex;
            this.BoardID = boardID;
            this.Limit = NOT_LIMITED;
            Tasks = new Dictionary<int, Task>();
            dto = new ColumnDTO(BoardID,ColumnIndex,NOT_LIMITED);
            dto.Insert();
        }

        /// <summary>
        /// Adds a task to the column if the column limit is not reached and the task does not already exist in the column.
        /// </summary>
        /// <param name="toAdd"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddTask(Task toAdd)
        {
            if(toAdd == null)
            {
                throw new ArgumentNullException("Task cannot be null.");
            }
            if(Tasks.Count >= Limit && Limit != -1)
            {
                throw new InvalidOperationException("Column limit reached.");
            }
            if (Tasks.ContainsKey(toAdd.TaskID))
            {
                throw new ArgumentException("Task with the same ID already exists in the column.");
            }
            Tasks.Add(toAdd.TaskID, toAdd);
        }

        /// <summary>
        /// Removes a task from the column and returns it if it exists.
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Task RemoveTask(int taskID)
        {
            if(!Tasks.ContainsKey(taskID))
            {
                throw new ArgumentException($"Task with ID {taskID} not found in the column.");
            }
            Task task = Tasks[taskID];
            Tasks.Remove(taskID);
            return task;
        }

        /// <summary>
        /// Updates the description of a task in the column if it exists.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Task UpdateTaskDescription(int taskId, string description)
        {
            if(!Tasks.ContainsKey(taskId))
            {
                throw new ArgumentException($"Task with ID {taskId} not found in the column.");
            }
            Tasks[taskId].UpdateTaskDescription(description);
            return Tasks[taskId];
        }

        /// <summary>
        /// Updates the title of a task in the column if it exists.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Task UpdateTaskTitle(int taskId, string title)
        {
            if (!Tasks.ContainsKey(taskId))
            {
                throw new ArgumentException($"Task with ID {taskId} not found in the column.");
            }
            Tasks[taskId].UpdateTaskTitle(title);
            return Tasks[taskId];
        }

        /// <summary>
        /// Updates the due date of a task in the column if it exists.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="dueDate"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task UpdateTaskDueDate(int taskId, DateTime dueDate)
        {
            if (!Tasks.ContainsKey(taskId))
            {
                throw new ArgumentException($"Task with ID {taskId} not found in the column.");
            }
            Tasks[taskId].UpdateTaskDueDate(dueDate);
            return Tasks[taskId];
        }

        /// <summary>
        /// Gets the column limit.
        /// </summary>
        /// <returns></returns>
        public int GetColumnLimit()
        {
            return Limit;
        }

        /// <summary>
        /// Sets the column limit to a specified value. The limit cannot be negative and cannot be set lower than the current task count.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public void LimitColumn(int limit)
        {
            if (limit < 0)
            {
                throw new InvalidOperationException("Column limit cannot be negative.");
            }
            if (Tasks.Count > limit)
            {
                throw new InvalidOperationException("Cannot set limit lower than current task count.");
            }
            this.Limit = limit;
            dto.Limit = limit;
        }

        // Checks if the column is full.
        public bool IsColumnFull()
        {
            return Tasks.Count == Limit;
        }

        /// <summary>
        /// Retrieves a task by its ID from the column.
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns>Required Task</returns>
        public Task GetTask(int taskID)   
        {
            if (!Tasks.ContainsKey(taskID))
            {
                throw new ArgumentException($"Task with ID {taskID} not found in the column.");
            }
            return Tasks[taskID];
        }

        //Deletes the column from the DB.
        public void Delete()
        {
            foreach(Task task in Tasks.Values)
            {
                task.Delete();
            }

            this.dto.Delete();
        }
    }
}
