using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using IntroSE.Kanban.Backend.DataAccessLayer;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

[assembly: InternalsVisibleTo("BusinessLayerTests")]


namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class BoardFacade
    {
        private int boardCount;
        internal Dictionary<User, Dictionary<string, Board>> UserBoards;
        internal Dictionary<int, Board> Boards;
        private BoardController boardController;
        private BoardMembersController boardMembersController;
        private static readonly ILog log = Logger.Instance;

        // Initializes a new instance of the BoardFacade class.
        public BoardFacade()
        {
            UserBoards = new Dictionary<User, Dictionary<string, Board>>();
            Boards = new Dictionary<int, Board>();
            boardController = new BoardController();
            boardMembersController = new BoardMembersController();
            boardCount = 1;
            log.Info("BoardFacade initialzed.");
        }

        // Loads all board and board member data from the database.
        public void LoadData()
        {
            Boards.Clear();
            foreach (var userDict in UserBoards.Values)
                userDict.Clear();

            LoadBoards();
            LoadBoardMembers();
            log.Info("Data loaded successfully.");
        }

        // Loads all boards from the database into the Boards dictionary.
        private void LoadBoards()
        {
            List<BoardDTO> bDTOs = boardController.SelectAll();
            foreach (BoardDTO boardDTO in bDTOs)
            {
                Boards.Add(boardDTO.ID, new Board(boardDTO));
            }
            boardCount = Boards.Any() ? Boards.Keys.Max() + 1 : 1;

            log.Info($"Loaded {boardCount} boards from the database.");
        }

        // Loads all board members from the database into the UserBoards dictionary.
        public void LoadBoardMembers()
        {
            List<BoardMemberDTO> bmDTOs = boardMembersController.SelectAll();
            foreach (BoardMemberDTO bmDTO in bmDTOs)
            {
                User currU = FindUser(bmDTO.Email);
                Board currB = Boards[bmDTO.BoardID];

                UserBoards[currU].Add(currB.Name, currB);
            }
            log.Info($"Loaded {UserBoards.Count} board members from the database.");
        }

        // Deletes all board and board member data from the database and memory.
        public void DeleteData()
        {
            boardController.DeleteAll();
            boardMembersController.DeleteAll();
        }

        // Adds a new user to the UserBoards dictionary.
        public void AddNewUser(User user)
        {
            this.UserBoards.Add(user, new Dictionary<string, Board>());
            log.Info($"User '{user.Email}' added to UserBoards.");
        }

        // Creates a new board for the specified user.
        public Board CreateBoard(string email, string boardName)
        {
            User user = FindUserLoggedIn(email);

            ValidateBoardCreation(user, boardName);

            Board newBoard = new Board(boardCount,boardName, email);
            UserBoards[user].Add(boardName, newBoard);
            Boards.Add(boardCount, newBoard);
            boardMembersController.Insert(email,newBoard.BoardID);
            boardCount++;
            log.Info($"Board '{boardName}' successfully created for user '{email}'.");
            return newBoard;
        }
        
        // Deletes a board for the specified user.
        public void DeleteBoard(string email, string boardName)
        {
            User user = FindUserLoggedIn(email);
            Board board = FindBoard(user, boardName);
            if (!board.Owner.Equals(email))
            {
                throw new InvalidOperationException($"{email} is not the board owner");
            }

            board.Delete();
            boardMembersController.DeleteAllByBoardID(board.BoardID);
            UserBoards[user].Remove(boardName);
            Boards.Remove(board.BoardID);
            log.Info($"Board '{boardName}' successfully deleted for user '{email}'.");
        }

        // Retrieves a board for the specified user.
        public Board GetBoard(string email, string boardName)
        {
            User user = FindUserLoggedIn(email);
            return FindBoard(user, boardName);
        }
        
        // Adds a task to the specified board for the specified user.
        public Task AddTask(string email, string boardName, string title, string description, DateTime dueDate)
        {
            User user = FindUserLoggedIn(email);
            Board board = FindBoard(user, boardName);
            ValidateBoardMember(user,board);

            log.Info($"Attempting to add task '{title}' to board '{boardName}' for user '{email}'.");
            return board.AddTask(title, description, dueDate);
        }

        //Moves a task from one column to the next.
        public void AdvanceTask(string email, string boardName, int columnIndex, int taskID)
        {
            User user = FindUserLoggedIn(email);
            Board board = FindBoard(user, boardName);
            ValidateTaskAssnigee(user, board, columnIndex, taskID);

            log.Info($"Attempting to advance task ID '{taskID}' in board '{boardName}' for user '{email}'.");
            board.AdvanceTask(columnIndex, taskID);
        }

        //Updates the description of a task in the specified column.
        public Task UpdateTaskDescription(string email, string boardName, int columnIndex, int taskId, string description)
        {
            User user = FindUserLoggedIn(email);
            Board board = FindBoard(user, boardName);
            ValidateTaskAssnigee(user, board, columnIndex, taskId);

            log.Info($"Attempting to update task description for task ID '{taskId}' in board '{boardName}' for user '{email}'.");
            return board.UpdateTaskDescription(columnIndex, taskId, description);
        }

        //Updates the due date of a task in the specified column.
        public Task UpdateTaskDueDate(string email, string boardName, int columnIndex, int taskId, DateTime dueDate)
        {
            User user = FindUserLoggedIn(email);
            Board board = FindBoard(user, boardName);

            ValidateTaskAssnigee(user, board, columnIndex, taskId);

            log.Info($"Attempting to update due date for task ID '{taskId}' in board '{boardName}' for user '{email}'.");
            return board.UpdateTaskDueDate(columnIndex, taskId, dueDate);
        }

        //Updates the title of a task in the specified column.
        public Task UpdateTaskTitle(string email, string boardName, int columnIndex, int taskId, string title)
        {
            User user = FindUserLoggedIn(email);
            Board board = FindBoard(user, boardName);
            ValidateTaskAssnigee(user, board, columnIndex, taskId);

            log.Info($"Attempting to update title for task ID '{taskId}' in board '{boardName}' for user '{email}'.");
            return board.UpdateTaskTitle(columnIndex, taskId, title);
        }

        //Retrieves a column from the specified board for the specified user.
        public Column GetColumn(string email, string boardName, int columnIndex)
        {
            User user = FindUserLoggedIn(email);
            Board board = FindBoard(user, boardName);

            log.Info($"Attempting to retrieve column '{columnIndex}' from board '{boardName}' for user '{email}'.");
            return board.GetColumn(columnIndex);
        }

        //Retrieves the limit of a column from the specified board for the specified user.
        public int GetColumnLimit(string email, string boardName, int columnIndex)
        {
            User user = FindUserLoggedIn(email);
            Board board = FindBoard(user, boardName);

            log.Info($"Attempting to retrieve column limit for column '{columnIndex}' from board '{boardName}' for user '{email}'.");
            return board.GetColumnLimit(columnIndex);
        }

        //Retrieves the name of a column from the specified board for the specified user.
        public string GetColumnName(string email, string boardName, int columnIndex)
        {
            User user = FindUserLoggedIn(email);
            Board board = FindBoard(user, boardName);

            log.Info($"Attempting to retrieve column name for column '{columnIndex}' from board '{boardName}' for user '{email}'.");
            return board.GetColumnName(columnIndex);
        }

        //Sets the limit of tasks of a column in the specified board for the specified user.
        public void LimitColumn(string email, string boardName, int columnIndex, int limit)
        {
            User user = FindUserLoggedIn(email);
            Board board = FindBoard(user, boardName);

            log.Info($"Attempting to set limit for column '{columnIndex}' in board '{boardName}' for user '{email}'.");
            board.LimitColumn(columnIndex, limit);
        }

        //Retrieves all tasks in progress for the specified user.
        public List<Task> InProgressTasks(string email)
        {
            User user = FindUserLoggedIn(email);

            List<Task> tasks = new List<Task>();
            foreach (Board board in UserBoards[user].Values)
            {
                Column inProgress = board.GetColumn(1);
                foreach (Task task in inProgress.Tasks.Values)
                {
                    if(task.Assignee.Equals(email))
                        tasks.Add(task);
                }
            }
            return tasks;
        }

        //Returns all the boards related to specific user
        public List<Board> GetUserBoards(string email)
        {
            User user = FindUserLoggedIn(email);
            return UserBoards[user].Values.ToList();
        }

        //Returns board name by boardID.
        public string GetBoardName(int boardID)
        {
            ValidateBoardExist(boardID);

            return Boards[boardID].Name;
        }

        // Adds a participant to the board 
        public void JoinBoard(string email, int boardId)
        {
            User toAdd = FindUserLoggedIn(email);
            Board board = ValidateJoinAndGetBoard(toAdd, boardId);
            
            UserBoards[toAdd][Boards[boardId].Name] = Boards[boardId];//updating userBoards[user] with the new board
            boardMembersController.Insert(email, boardId);
            log.Info($"{email} joined board {GetBoardName(boardId)}.");
        }

        // Removes a participant from the board 
        public void LeaveBoard(string email, int boardId)
        {
            User toRemove = FindUserLoggedIn(email);
            ValidateLeaveBoard(toRemove, boardId);

            UserBoards[toRemove].Remove(Boards[boardId].Name);
            Boards[boardId].UnassignTasks(email);
            boardMembersController.Delete(email,boardId);
            log.Info($"{email} left board {GetBoardName(boardId)}.");
        }

        // Transfers board ownership from the old owner to the new owner.
        public void TransferOwnership(string boardName, string oldOwner, string newOwner)
        {
            User oldUser = FindUserLoggedIn(oldOwner);
            User newUser = FindUser(newOwner);
            Board board = FindBoard(oldUser, boardName);

            ValidateBoardMember(newUser, board);
            board.TransferOwnership(newOwner);
        }

        // Assigns a task to a user in the specified board and column.
        public void AssignTask(string email, string boardName, int columnIndex, int taskID, string emailAssignee)
        {
            User assigner = FindUserLoggedIn(email);
            User newA = FindUser(emailAssignee);
            Board board = FindBoard(assigner, boardName);

            ValidateBoardMember(newA, board);

            board.AssignTask(email,emailAssignee, taskID,columnIndex);
        }


        /////////////////////////
        /// Private Functions ///
        /////////////////////////

        // Finds a user by email and checks if they are logged in.
        private User FindUserLoggedIn(string email)
        {
            User user = FindUser(email);
            CheckLoggedIn(user);
            return user;
        }

        // Finds a user in the UserBoards dictionary by email.
        private User FindUser(string email)
        {
            if (email == null)
            {
                log.Error("Email cannot be null.");
                throw new ArgumentException("Email cannot be null.");
            }

            foreach (User user in UserBoards.Keys)
            {
                if (user.Email.Equals(email))
                {
                    return user;
                } 
            }

            log.Error($"User {email} not found.");
            throw new InvalidOperationException("User not found.");
        }

        // Finds the board by the given user and board name.
        private Board FindBoard(User user, string boardName)
        {
            if (!UserBoards[user].Keys.Any(board => board.Equals(boardName, StringComparison.OrdinalIgnoreCase)))
            {
                log.Error($"Board {boardName} does not exist for user {user.Email}.");
                throw new ArgumentException("Board doesnt exist.");
            }
            return UserBoards[user][boardName];
        }

        // Validates that the user is a member of the board.
        private void ValidateBoardMember(User user, Board board)
        {
            if (!UserBoards.ContainsKey(user) || !UserBoards[user].ContainsValue(board))
                throw new InvalidOperationException($"User '{user.Email}' is not a member of the board '{board.Name}'.");
        }

        // Checks if the user is logged in.
        private void CheckLoggedIn(User user)
        {
            if (!user.IsLoggedIn())
            {
                log.Error($"User {user.Email} is not logged in.");
                throw new InvalidOperationException("User is not logged in.");
            }
        }
      
        // Validates if the user is the assignee of the task in the specified board.
        private void ValidateTaskAssnigee(User user, Board board,int columnindex, int taskID)
        {
            string currAssignee = board.GetTask(taskID, columnindex).Assignee;
            if (!user.Email.Equals(currAssignee))
                throw new InvalidOperationException($"{user.Email} not the assignee of this task.");

        }

        // Validates board creation for a user and board name.
        private void ValidateBoardCreation(User user, string boardName)
        {
            if (string.IsNullOrEmpty(boardName))
            {
                log.Error("Board name cannot be null or empty.");
                throw new ArgumentException("Board name cannot be null or empty.");
            }
            if (UserBoards[user].Keys.Any(board => board.Equals(boardName, StringComparison.OrdinalIgnoreCase)))
            {
                log.Error($"Board '{boardName}' already exists for user '{user.Email}'");
                throw new InvalidOperationException("Board already exists.");
            }
        }

        // Validates join and gets the board by user and board ID.
        private Board ValidateJoinAndGetBoard(User user, int boardId)
        {
            Board board;

            ValidateBoardExist(boardId);
            board = Boards[boardId];

            if (UserBoards.ContainsKey(user) && UserBoards[user].Values.Contains(board))
            {
                log.Warn($"Participant {user.Email} already exists in board {GetBoardName(boardId)}");
                throw new ArgumentException($"Participant {user.Email} already exists in board {GetBoardName(boardId)}.");
            }

            return board;
        }

        // Validates if a user can leave a board.
        private void ValidateLeaveBoard(User user, int boardId)
        {
            ValidateBoardExist(boardId);

            if (user.Email.Equals(Boards[boardId].Owner))
            {
                log.Error($"Cannot remove owner {user.Email} from board {Boards[boardId].Name}.");
                throw new InvalidOperationException("Cannot remove owner from board.");
            }
            if (!UserBoards[user].ContainsKey(Boards[boardId].Name))
            {
                log.Error($"{user.Email} is not a participant in the board.");
                throw new InvalidOperationException($"{user.Email} is not a participant in the board.");
            }
        }

        // Validates if a board exists by board ID.
        private void ValidateBoardExist(int boardId)
        {
            if (!Boards.ContainsKey(boardId))
            {
                log.Error($"Board with ID {boardId} does not exist.");
                throw new InvalidOperationException("Board ID doesn't exist.");
            }
        }
    }
}
