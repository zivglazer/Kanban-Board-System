using Frontend.Model;
using System;

namespace Frontend.ViewModel
{
    public class BoardVM : NotifiableObject
    {
        private UserModel user;
        private BackendController Controller => user.Controller;
        public string Title { get; private set; }
        private BoardModel _board;
        public BoardModel Board { get => _board; private set { _board = value; RaisePropertyChanged("Board"); } }
        private TaskModel _selectedTask;
        public TaskModel SelectedTask { get => _selectedTask; set { _selectedTask = value; RaisePropertyChanged("SelectedTask"); } }
        private string _newTaskTitle;
        public string NewTaskTitle { get => _newTaskTitle; set { _newTaskTitle = value; RaisePropertyChanged("NewTaskTitle"); } }
        private string _newTaskDescription;
        public string NewTaskDescription { get => _newTaskDescription; set { _newTaskDescription = value; RaisePropertyChanged("NewTaskDescription"); } }
        private DateTime _newTaskDueDate = DateTime.Now.AddDays(7);
        public DateTime NewTaskDueDate { get => _newTaskDueDate; set { _newTaskDueDate = value; RaisePropertyChanged("NewTaskDueDate"); } }
        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; RaisePropertyChanged("ErrorMessage"); } }

        public BoardVM(UserModel user, BoardModel board)
        {
            this.user = user;
            this.Title = "Board: " + board.Name;
            LoadFullBoard(board.Id);
        }

        private void LoadFullBoard(int boardId)
        {
            ErrorMessage = "";
            try
            {
                Board = Controller.BoardController.LoadBoardDetails(user.Email, boardId, this.Controller);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }

        public void AddTask()
        {
            ErrorMessage = "";
            try
            {
                Controller.TaskController.AddTask(user.Email, Board.Name, NewTaskTitle, NewTaskDescription, NewTaskDueDate);
                LoadFullBoard(Board.Id);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                throw e;
            }
            finally
            {
                NewTaskTitle = "";
                NewTaskDescription = "";
                NewTaskDueDate = DateTime.Now.AddDays(7);
            }
        }

        public void AdvanceTask()
        {
            ErrorMessage = "";
            try
            {
                if (SelectedTask == null) throw new Exception("Please select a task.");
                Controller.TaskController.AdvanceTask(user.Email, Board.Name, SelectedTask.ColumnIndex, SelectedTask.Id);
                LoadFullBoard(Board.Id);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                throw e;
            }
        }

        public void AssignTask(string assigneeEmail)
        {
            ErrorMessage = "";
            try
            {
                if (SelectedTask == null) throw new Exception("Please select a task.");
                Controller.TaskController.AssignTask(user.Email, Board.Name, SelectedTask.ColumnIndex, SelectedTask.Id, assigneeEmail);
                LoadFullBoard(Board.Id);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                throw e;
            }
        }
    }
}
