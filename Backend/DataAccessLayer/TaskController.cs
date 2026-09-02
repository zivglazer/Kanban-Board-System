using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class TaskController : Controller
    {
        private static readonly ILog log = Logger.Instance;

        public TaskController() : base("Tasks") { }

        // Retrieves all tasks from the database.
        public List<TaskDTO> SelectAll()
        {
            List<TaskDTO> results = new List<TaskDTO>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"select * from {_tableName};";
                SQLiteDataReader dataReader = null;
                try
                {
                    connection.Open();
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        results.Add(ConvertReaderToDTO(dataReader));
                    }
                    log.Info($"Successfully selected all tasks from the database. Count: {results.Count}");
                }
                finally
                {
                    if (dataReader != null)
                    {
                        dataReader.Close();
                    }

                    command.Dispose();
                    connection.Close();
                }

            }
            return results;
        }


        // Retrieves all tasks for a specific board.
        public List<TaskDTO> SelectByBoardID(int BoardID)
        {
            List<TaskDTO> results = new List<TaskDTO>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"SELECT * FROM {_tableName} WHERE BoardID = @BoardID;";
                command.Parameters.AddWithValue("@BoardID", BoardID);

                SQLiteDataReader dataReader = null;
                try
                {
                    connection.Open();
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        results.Add(ConvertReaderToDTO(dataReader));
                    }
                    log.Info($"Successfully selected all tasks for BoardID: {BoardID} from the database. Count: {results.Count}");
                }
                finally
                {
                    if (dataReader != null)
                    {
                        dataReader.Close();
                    }

                    command.Dispose();
                    connection.Close();
                }
            }
            return results;
        }

        // Converts a data reader row to a TaskDTO object.
        public override TaskDTO ConvertReaderToDTO(SQLiteDataReader reader)
        {
            int id = Convert.ToInt32(reader.GetValue(0));
            int boardID = Convert.ToInt32(reader.GetValue(1));
            string assignee = reader.GetString(2);
            int state = Convert.ToInt32(reader.GetValue(3));
            string title = reader.GetString(4);
            string description = reader.GetString(5);
            DateTime creationDate = DateTime.Parse(reader.GetString(6));
            DateTime dueDate = DateTime.Parse(reader.GetString(7));

            log.Info("TaskDTO successfully retrieved");
            return new TaskDTO(id, boardID, assignee, state, title, description, creationDate, dueDate);
        }
    }
}
