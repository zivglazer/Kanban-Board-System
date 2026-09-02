using Frontend.Model;
using System;
using System.Collections.ObjectModel;

namespace Frontend.ViewModel
{
    public class DashboardVM : NotifiableObject
    {
        public UserModel User { get; private set; }
        private BackendController Controller => User.Controller;

        public string Title { get; private set; }
        private ObservableCollection<BoardModel> _boards;

        private string _owner;

        public string Email => User.Email;
        public string Owner
        {
            get => _owner;
            set
            {
                if (_owner != value)
                {
                    _owner = value;
                    RaisePropertyChanged(nameof(Owner)); 
                }
            }
        }
        public ObservableCollection<BoardModel> Boards
        {
            get => _boards;
            set
            {
                _boards = value;
                RaisePropertyChanged(nameof(Boards));
            }
        }
        private BoardModel _selectedBoard;
        public BoardModel SelectedBoard { get => _selectedBoard; set { _selectedBoard = value; RaisePropertyChanged("SelectedBoard"); } }
        private string _newBoardName;
        public string NewBoardName { get => _newBoardName; set { _newBoardName = value; RaisePropertyChanged("NewBoardName"); } }
        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; RaisePropertyChanged("ErrorMessage"); } }

        public DashboardVM(UserModel user)
        {
            this.User = user;
            this.Title = "Kanban Boards for " + user.Email;
            LoadBoards();
        }

        public void LoadBoards()
        {
            ErrorMessage = "";
            try
            {
                Boards = new ObservableCollection<BoardModel>(Controller.BoardController.GetUserBoards(User.Email, this.Controller));
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                Boards = new ObservableCollection<BoardModel>();
            }
        }

        public void CreateBoard()
        {
            ErrorMessage = "";
            try
            {
                Controller.BoardController.CreateBoard(User.Email, NewBoardName);
                NewBoardName = "";
                Owner = User.Email;
                LoadBoards();
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                throw e;
            }
        }

        public void DeleteBoard()
        {
            ErrorMessage = "";
            try
            {
                if (SelectedBoard == null) throw new Exception("Please select a board to delete.");
                Controller.BoardController.DeleteBoard(User.Email, SelectedBoard.Name);
                LoadBoards();
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                throw e;
            }
        }

        public void Logout()
        {
            try
            {
                Controller.UserController.Logout(User.Email);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }
    }
}
