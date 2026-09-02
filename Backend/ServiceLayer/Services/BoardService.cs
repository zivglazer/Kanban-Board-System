using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using log4net;
using Task = IntroSE.Kanban.Backend.BusinessLayer.Task;

[assembly: InternalsVisibleTo("ServiceLayerTests")]

namespace IntroSE.Kanban.Backend.ServiceLayer.Services
{
    public class BoardService

    {
        private static readonly ILog log = Logger.Instance;
        private BoardFacade boardFacade;

        internal BoardService(BoardFacade boardFacade)
        {
            this.boardFacade = boardFacade;
            log.Info("BoardService initialized");
        }

        public string GetBoard(string email, int boardID)
        {
            Response<BoardSL> response;
            try
            {
                log.Info($"Attempting to Get board {boardID} for user {email}");
                Board b = boardFacade.GetBoard(email, boardFacade.GetBoardName(boardID));
                BoardSL ans = new BoardSL(b.BoardID, b.Name, b.Owner);
                response = new Response<BoardSL>(ans, null);
                log.Info($"Board {boardID} returned successfully for user {email}");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to return board {boardID}: {ex.Message}");
                response = new Response<BoardSL>(ex.Message);
            }
            return response.ToJson();
        }

        // Creates a new board for the specified user.
        public string CreateBoard(string email, string boardName)
        {
            Response<string> response;
            try
            {
                log.Info($"Attempting to create board {boardName} for user: {email}");
                Board ans = boardFacade.CreateBoard(email, boardName);
                response = new Response<string>();
                log.Info($"Board {boardName} created successfully for user {email}");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to create board {boardName} for user {email}");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }

        // Deletes a board for the specified user.
        public string DeleteBoard(string email, string boardName)
        {
            Response<string> response;
            try
            {
                log.Info($"Attempting to delete board {boardName} for user {email}");
                boardFacade.DeleteBoard(email, boardName);
                response = new Response<string>();
                log.Info($"Board {boardName} deleted successfully for user {email}");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to delete board {boardName}: {ex.Message}");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }

        // Sets a limit for a column in the specified board.
        public string LimitColumn(string email, string boardName, int columnIndex, int limit)
        {
            Response<string> response;
            try
            {
                log.Info($"Attempting to limit column {columnIndex} in board {boardName} to {limit}");
                boardFacade.LimitColumn(email, boardName, columnIndex, limit);
                response = new Response<string>();
                log.Info($"Column {columnIndex} in board {boardName} limited to {limit}");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to limit column {columnIndex} at board {boardName}");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }

        // Gets the limit of a column in the specified board.
        public string GetColumnLimit(string email, string boardName, int columnIndex)
        {
            Response<int?> response;
            try
            {
                log.Info($"Attempting to get limit for column {columnIndex} in board {boardName}");
                int ans = boardFacade.GetColumnLimit(email, boardName, columnIndex);
                response = new Response<int?>(ans, null);
                log.Info($"Got limit {ans} for column {columnIndex}");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get column limit for column {columnIndex}");
                return new Response<string>(ex.Message).ToJson();
            }
            return response.ToJson();
        }

        // Gets the name of a column in the specified board.
        public string GetColumnName(string email, string boardName, int columnIndex)
        {
            Response<string> response;
            try
            {
                log.Info($"Attempting to get name for column {columnIndex} in board {boardName}");
                string ans = boardFacade.GetColumnName(email, boardName, columnIndex);
                response = new Response<string>(ans, null);
                log.Info($"Column {columnIndex} name is {ans}");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get column name for column {columnIndex} at board {boardName}");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }

        // Retrieves a column from the specified board.
        public string GetColumn(string email, string boardName, int columnIndex)
        {
            Response<List<TaskSL>> response;
            try
            {
                log.Info($"Attempting to get column {columnIndex} from board {boardName}");
                Column ans = boardFacade.GetColumn(email, boardName, columnIndex);
                List<TaskSL> tasks = new List<TaskSL>();
                foreach (Task t in ans.Tasks.Values)
                    tasks.Add(new TaskSL(t.TaskID, t.Title, t.Description, t.DueDate, t.CreationDate, email));

                response = new Response<List<TaskSL>>(tasks, null);
                log.Info($"Got column {columnIndex}");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get column {columnIndex} from board {boardName}");
                response = new Response<List<TaskSL>>(ex.Message);
            }
            return response.ToJson();
        }
        // Transfers board ownership from one user to another.
        public string TransferOwnership(string boardName, string oldOwner, string newOwner)
        {

            Response<string> response;
            try
            {
                log.Info($"Attempting to get tranfer ownership for Board {boardName} from {oldOwner} to {newOwner}");
                boardFacade.TransferOwnership(boardName, oldOwner, newOwner);
                response = new Response<string>();
                log.Info($"Transfer succedd");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to transfer ownership");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }

        // Adds a user to a board by board ID.
        public string JoinBoard(string email, int boardId)
        {
            Response<string> response;
            try
            {
                log.Info($"Attempting to add user {email} to board with ID {boardId}.");
                boardFacade.JoinBoard(email, boardId);
                response = new Response<string>();
                log.Info($"User {email} successfully joined board with ID {boardId}.");
            }
            catch(Exception ex)
            {
                log.Error($"Failed to add user {email} to board with ID {boardId}");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }

        // Removes a user from a board by board ID.
        public string LeaveBoard(string email, int boardID)
        {
            Response<string> response;
            try
            {
                log.Info($"Attempting to remove user {email} to board with ID {boardID}.");
                boardFacade.LeaveBoard(email, boardID);
                response = new Response<string>();
                log.Info($"User {email} successfully removed board with ID {boardID}.");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to removed user {email} to board with ID {boardID}");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }

        // Gets all board IDs for a user.
        public string GetUserBoards(string email)
        {
            Response<List<BoardSL>> response;
            try
            {
                log.Info($"Attempting to get {email} boards");
                List<Board> ans = boardFacade.GetUserBoards(email);
                List<BoardSL> toRet = new List<BoardSL>();
                foreach (Board b in ans)
                    toRet.Add(new BoardSL(b.BoardID,b.Name,b.Owner));
                response = new Response<List<BoardSL>>(toRet, null);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get boards for {email}");
                response = new Response<List<BoardSL>>(ex.Message);
            }
            return response.ToJson();
        }

        // Gets the name of a board by its ID.
        public string GetBoardName(int boardId)
        {
            Response<string> response;
            try
            {
                log.Info($"Attempting to get name for Board with ID {boardId}");
                string ans = boardFacade.GetBoardName(boardId);
                response = new Response<string>(ans, null);
                log.Info($"Board name is {ans}");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get board name");
                response = new Response<string>(ex.Message);
            }
            return response.ToJson();
        }

        // Loads all board data from the data source.
        public void LoadData()
        {
            boardFacade.LoadData();

        }

        // Deletes all board data from the data source.
        public void DeleteData()
        {
            boardFacade.DeleteData();
        }
    }
}