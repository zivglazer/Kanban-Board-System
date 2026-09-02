using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal abstract class Controller
    {
        protected readonly string _tableName;
        protected readonly string _connectionString;
        private static readonly ILog log = Logger.Instance;

        public static int _state = 0; //For our program tests. IMPORTANT: KEEP STATE = 1, PUSH AND EDIT TO 0 FROM GIT
                                      //state = 1 is test mode
                                      //state = 0 is VPL mode

        // Initializes a new instance of the Controller class.
        public Controller(string name) 
        {
            _tableName = name;
          
            //This is for the VPL

            string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "kanban.db"));

            if(_state == 1)
            {
                string solutionDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
                path = Path.Combine(solutionDir, "Data", "kanban.db");
            }

            this._connectionString = $"Data Source={path}; Version=3;";
        }

        // Inserts a DTO into the database.
        public bool Insert(DTO dto)
        {
            var values = dto.ToColumnValuePairs();
            int res = -1;

            using (var connection = new SQLiteConnection(_connectionString))
            using (var command = new SQLiteCommand(null, connection))
            {
                try
                {
                    connection.Open();

                    string columns = string.Join(", ", values.Keys);
                    string parameters = string.Join(", ", values.Keys.Select(k => $"@{k}"));

                    command.CommandText = $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters});";

                    foreach (var pair in values)
                        command.Parameters.AddWithValue($"@{pair.Key}", pair.Value);

                    res = command.ExecuteNonQuery();
                    log.Info($"Inserted row into {_tableName} successfully.");
                }
                catch (Exception ex)
                {
                    log.Error($"Insert failed for table {_tableName}: {ex.Message}");
                }
            }

            return res > 0;
        }

        // Updates a DTO in the database.
        public bool Update(DTO dto)
        {
            var values = dto.ToColumnValuePairs();
            var primaryKey = dto.PrimaryKey;
            int res = -1;

            using (var connection = new SQLiteConnection(_connectionString))
            using (var command = new SQLiteCommand(null, connection))
            {
                try
                {
                    connection.Open();

                    string setClause = string.Join(", ",
                        values.Where(kv => !primaryKey.ContainsKey(kv.Key))
                              .Select(kv => $"{kv.Key} = @{kv.Key}"));

                    string whereClause = string.Join(" AND ",
                        primaryKey.Keys.Select(k => $"{k} = @PK_{k}"));

                    command.CommandText = $"UPDATE {_tableName} SET {setClause} WHERE {whereClause};";

                    foreach (var pair in values.Where(kv => !primaryKey.ContainsKey(kv.Key)))
                        command.Parameters.AddWithValue($"@{pair.Key}", pair.Value);

                    foreach (var pair in primaryKey)
                        command.Parameters.AddWithValue($"@PK_{pair.Key}", pair.Value);

                    res = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    log.Error($"Update failed for table {_tableName}: {ex.Message}");
                }
            }

            return res > 0;
        }

        // Deletes a DTO from the database.
        public bool Delete(DTO dto)
        {
            var primaryKey = dto.PrimaryKey;
            int res = -1;

            using (var connection = new SQLiteConnection(_connectionString))
            using (var command = new SQLiteCommand(null, connection))
            {
                try
                {
                    connection.Open();

                    // Build WHERE clause for all primary key columns
                    var whereClause = string.Join(" AND ", primaryKey.Keys.Select(k => $"{k} = @{k}"));
                    command.CommandText = $"DELETE FROM {_tableName} WHERE {whereClause};";

                    // Add parameters
                    foreach (var pair in primaryKey)
                        command.Parameters.AddWithValue($"@{pair.Key}", pair.Value);

                    res = command.ExecuteNonQuery();
                    //log.Info($"Deleted row from {_tableName} where {whereClause}");
                }
                catch (Exception ex)
                {
                    log.Error($"Delete failed for table {_tableName}: {ex.Message}");
                }
            }
            return res > 0;
        }

        // Deletes all data from all tables in the database.
        public bool DeleteAll()
        {
            int res = -1;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                var command = new SQLiteCommand
                {
                    Connection = connection
                };

                try
                {
                    connection.Open();

                    // Disable foreign key checks
                    command.CommandText = "PRAGMA foreign_keys = OFF";
                    command.ExecuteNonQuery();

                    // Delete from all tables (adjust the order based on FK constraints)
                    string[] tableNames = { "BoardMembers", "Tasks", "Columns", "Boards", "Users" };
                    res = 0;
                    foreach (var table in tableNames)
                    {
                        command.CommandText = $"DELETE FROM {table}";
                        res += command.ExecuteNonQuery();
                    }

                    // Re-enable foreign key checks
                    command.CommandText = "PRAGMA foreign_keys = ON";
                    command.ExecuteNonQuery();
                    log.Info("All data successfully deleted from the database.");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }
            }

            return res >= 0; // true if no error occurred
        }

        public abstract DTO ConvertReaderToDTO(SQLiteDataReader reader);

    }
}
