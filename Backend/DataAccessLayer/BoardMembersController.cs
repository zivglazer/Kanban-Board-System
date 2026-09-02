using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal class BoardMembersController : Controller
    {

        public const string EmailColumnName = "Email";
        public const string BoardIDColumnName = "BoardID";
        private static readonly ILog log = Logger.Instance;

        public BoardMembersController() : base("BoardMembers") { }


        // Returns all board members with their associated user and board data.
        public List<BoardMemberDTO> SelectAll()
        {
            List<BoardMemberDTO> boardMembers = new List<BoardMemberDTO>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    SQLiteCommand command = new SQLiteCommand(null, connection);
                    string select = $"SELECT  {BoardMemberDTO.EmailColumnName}, {BoardMemberDTO.BoardIDColumnName} FROM {_tableName}";
                    connection.Open();
                    command.CommandText = select;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            boardMembers.Add(ConvertReaderToDTO(reader));
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Error selecting all board members. Exception: {e.Message}");
                }
                finally
                {
                    connection.Close();
                }
            }
            return boardMembers;
        }

        // Inserts a new board member (user-board relationship) into the database.
        public bool Insert(string email, int boardID)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                int res = -1;

                try
                {
                    connection.Open();

                    command.CommandText = $"INSERT INTO {_tableName} ({BoardMemberDTO.EmailColumnName}, {BoardMemberDTO.BoardIDColumnName}) " +
                                          $"VALUES (@emailVal, @boardIdVal);";

                    SQLiteParameter emailParam = new SQLiteParameter("@emailVal", email);
                    SQLiteParameter boardIdParam = new SQLiteParameter("@boardIdVal", boardID);

                    command.Parameters.Add(emailParam);
                    command.Parameters.Add(boardIdParam);

                    command.Prepare();
                    res = command.ExecuteNonQuery();
                    log.Info($"Successfully inserted BoardMember with Email: {email} and BoardID: {boardID} into the database.");
                }
                catch (Exception ex)
                {
                    log.Error($"Error inserting BoardMember");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }

                return res > 0;
            }
        }

        // Deletes a board member (user-board relationship) from the database.
        public bool Delete(string email, int boardID)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                int res = -1;

                try
                {
                    connection.Open();
                    command.CommandText = $"DELETE FROM {_tableName} WHERE {BoardMemberDTO.EmailColumnName} = @emailVal AND {BoardMemberDTO.BoardIDColumnName} = @boardIdVal;";

                    SQLiteParameter emailParam = new SQLiteParameter("@emailVal", email);
                    SQLiteParameter boardIdParam = new SQLiteParameter("@boardIdVal", boardID);

                    command.Parameters.Add(emailParam);
                    command.Parameters.Add(boardIdParam);

                    command.Prepare();
                    res = command.ExecuteNonQuery();
                    log.Info($"Successfully deleted BoardMember with Email: {email} and BoardID: {boardID} from the database.");
                }
                catch (Exception ex)
                {
                    log.Error($"Error deleting BoardMember with Email: {email} and BoardID: {boardID}. Exception: {ex.Message}");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }

                return res > 0;
            }
        }

        // Deletes all the relation to specific board.
        public bool DeleteAllByBoardID(int boardID)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                int res = -1;

                try
                {
                    connection.Open();
                    command.CommandText = $"DELETE FROM {_tableName} WHERE {BoardMemberDTO.BoardIDColumnName} = @boardIdVal;";

                    SQLiteParameter boardIdParam = new SQLiteParameter("@boardIdVal", boardID);
                    command.Parameters.Add(boardIdParam);

                    command.Prepare();
                    res = command.ExecuteNonQuery();
                    log.Info($"Successfully deleted all BoardMembers related to BoardID: {boardID}.");
                }
                catch (Exception ex)
                {
                    log.Error($"Error deleting BoardMembers with BoardID: {boardID}. Exception: {ex.Message}");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }

                return res > 0;
            }
        }

        // Converts a data reader row to a BoardMemberDTO object.
        public override BoardMemberDTO ConvertReaderToDTO(SQLiteDataReader reader)
        {
            string email = (string)reader.GetValue(0);
            int boardID = Convert.ToInt32(reader.GetValue(1));

            log.Info("BoardMemeberDTO successfully retrieved");

            return new BoardMemberDTO(boardID,email);
        }
    }
}
