using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class BoardController : Controller
    {
        public BoardController() : base("Boards") { }
        private static readonly ILog log = Logger.Instance;

        // Retrieves a board by its ID.
        public BoardDTO SelectBoard(int boardId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"SELECT * FROM {_tableName} WHERE BoardID = @boardId;";
                command.Parameters.AddWithValue("@boardId", boardId);
                SQLiteDataReader dataReader = null;

                try
                {
                    connection.Open();
                    dataReader = command.ExecuteReader();

                    if (dataReader.Read())
                    {
                        log.Info($"Board with ID: {boardId} successfully retrieved.");
                        return ConvertReaderToDTO(dataReader);
                    }
                    log.Warn($"No board found with ID: {boardId}.");
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
            return null; 
        }

        // Retrieves all boards from the database.
        public List<BoardDTO> SelectAll()
        {
            List<BoardDTO> boards = new List<BoardDTO>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(null, connection);
                    string select = $"SELECT BoardID,Name,Owner FROM {_tableName}";
                    connection.Open();
                    command.CommandText = select;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            boards.Add(ConvertReaderToDTO(reader));
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Error selecting all boards. Exception: {e.Message}");
                }
                finally
                {
                    connection.Close();
                }
            }
            return boards;
        }

        // Converts a data reader row to a BoardDTO object.
        public override BoardDTO ConvertReaderToDTO(SQLiteDataReader reader)
        {
            int boardID = Convert.ToInt32(reader.GetValue(0));
            string name = (string)reader.GetValue(1);
            string owner = (string)reader.GetValue(2);


            return new BoardDTO(boardID, name, owner);
        }
    }
}
