using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class ColumnController : Controller
    {
        private static readonly ILog log = Logger.Instance;


        public ColumnController() : base("Columns") { }

        // Retrieves all columns from the database.
        public List<ColumnDTO> SelectAll()
        {
            List<ColumnDTO> results = new List<ColumnDTO>();
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
                    log.Info($"Successfully selected all columns from the database. Count: {results.Count}");
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

        // Converts a data reader row to a ColumnDTO object.
        public override ColumnDTO ConvertReaderToDTO(SQLiteDataReader reader)
        {
            int boardID = Convert.ToInt32(reader.GetValue(0));
            int columnIndex = Convert.ToInt32(reader.GetValue(1));
            int limit = Convert.ToInt32(reader.GetValue(2));

            log.Info($"Column {columnIndex} successfully retrieved");

            return new ColumnDTO(boardID,columnIndex,limit);
        }

        // Retrieves the columns of specific board from the database.
        public List<ColumnDTO> SelectBoardColumns(int boardID)
        {
            List<ColumnDTO> results = new List<ColumnDTO>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"SELECT * FROM Columns WHERE BoardID = @boardIDVal;";

                SQLiteParameter boardIdParam = new SQLiteParameter("@boardIDVal", boardID);
                command.Parameters.Add(boardIdParam);

                SQLiteDataReader dataReader = null;

                try
                {
                    connection.Open();
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        results.Add(ConvertReaderToDTO(dataReader));
                    }
                    log.Info($"Successfully selected columns for BoardID: {boardID}. Count: {results.Count}");
                }
                finally
                {
                    if (dataReader != null)
                        dataReader.Close();

                    command.Dispose();
                    connection.Close();
                }
            }

            return results;
        }

    }
}
