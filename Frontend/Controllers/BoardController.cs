
using Frontend.Model;
using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using IntroSE.Kanban.Backend.ServiceLayer.Services;

using System.Text.Json;

namespace Frontend.Controllers
{
    public class BoardController
    {
        private readonly BoardService boardService;
        // BackendController dependency is removed.
        // private readonly BackendController backendController; 

        // The constructor is simplified.
        internal BoardController(BoardService boardService)
        {
            this.boardService = boardService;
        }

        private void HandleError<T>(Response<T> response)
        {
            if (response.ErrorOccured) throw new Exception(response.ErrorMessage);
        }

        // This method now takes a BackendController instance to construct models.
        // This responsibility is moved up to the calling ViewModel.
        public List<BoardModel> GetUserBoards(string email, BackendController backendController)
        {
            var boardsRes = JsonSerializer.Deserialize<Response<List<BoardSL>>>(boardService.GetUserBoards(email));
            HandleError(boardsRes);

            var boards = new List<BoardModel>();
            foreach (BoardSL board in boardsRes.ReturnValue)
            {
                boards.Add(new BoardModel(backendController, board.BoardID, board.BoardName, board.Owner));
            }
            return boards;
        }

        public void CreateBoard(string email, string boardName)
        {
            var response = JsonSerializer.Deserialize<Response<string>>(boardService.CreateBoard(email, boardName));
            HandleError(response);
        }

        public void DeleteBoard(string email, string boardName)
        {
            var response = JsonSerializer.Deserialize<Response<string>>(boardService.DeleteBoard(email, boardName));
            HandleError(response);
        }

        // This method is also updated to take the controller as a parameter.
        public BoardModel LoadBoardDetails(string userEmail, int boardId, BackendController backendController)
        {
            var boardResponse = JsonSerializer.Deserialize<Response<BoardSL>>(boardService.GetBoard(userEmail, boardId));
            HandleError(boardResponse);

            BoardSL boardData = boardResponse.ReturnValue;
            BoardModel boardModel = new BoardModel(backendController, boardData.BoardID, boardData.BoardName, boardData.Owner);

            for (int i = 0; i < 3; i++)
            {
                var columnResponse = JsonSerializer.Deserialize<Response<List<TaskSL>>>(boardService.GetColumn(userEmail, boardData.BoardName, i));
                if (columnResponse.ErrorOccured) continue;

                foreach (var taskData in columnResponse.ReturnValue)
                {
                    // Construct TaskModel with the provided controller.
                    var taskModel = new TaskModel(backendController, taskData.TaskID, taskData.CreationDate, taskData.DueDate, taskData.Title, taskData.Description, taskData.Assignee, i);
                    switch (i)
                    {
                        case 0: boardModel.BacklogTasks.Add(taskModel); break;
                        case 1: boardModel.InProgressTasks.Add(taskModel); break;
                        case 2: boardModel.DoneTasks.Add(taskModel); break;
                    }
                }
            }
            return boardModel;
        }
    }
}