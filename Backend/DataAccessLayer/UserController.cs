using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class UserController : Controller
    {
        public UserController() : base("Users") { }
        private static readonly ILog log = Logger.Instance;


        // Converts a data reader row to a UserDTO object.
        public override UserDTO ConvertReaderToDTO(SQLiteDataReader reader)
        {
            string email = reader.GetString(0);
            string password = reader.GetString(1);
            return new UserDTO(email, password);
        }

        // Retrieves all users from the Users table.
        public List<UserDTO> SelectAllUsers()
        {
            List<UserDTO> users = new List<UserDTO>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(null, connection);
                    string select = $"SELECT Email, Password FROM {_tableName}";
                    connection.Open();
                    command.CommandText = select;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(ConvertReaderToDTO(reader));
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Error selecting all users. Exception: {e.Message}");
                }
                finally
                {
                    connection.Close();
                }
            }
            return users;
        }

        // Retrieves a user by email from the Users table.
        public UserDTO SelectUser(string email)
        {
            UserDTO user = null;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(null, connection);
                    string select = $"SELECT Email, Password FROM {_tableName} WHERE Email = @Email";
                    connection.Open();
                    SQLiteParameter emailParam = new SQLiteParameter("@Email", email);
                    command.CommandText = select;
                    command.Parameters.Add(emailParam);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = ConvertReaderToDTO(reader);
                        }
                    }

                }
                catch (Exception e)
                {
                    log.Error($"Error selecting user.");
                }
                finally
                {
                    connection.Close();
                }
            }
            return user;
        }
        
    }
}

