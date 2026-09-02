using System.Collections.ObjectModel;

namespace Frontend.Model
{
    public class BoardModel : NotifiableModelObject
    {
        public int Id { get; }
        private string _name;
        public string Name { get => _name; set { _name = value; RaisePropertyChanged("Name"); } }
        public string OwnerEmail { get; }
        public ObservableCollection<TaskModel> BacklogTasks { get; } = new ObservableCollection<TaskModel>();
        public ObservableCollection<TaskModel> InProgressTasks { get; } = new ObservableCollection<TaskModel>();
        public ObservableCollection<TaskModel> DoneTasks { get; } = new ObservableCollection<TaskModel>();

        public BoardModel(BackendController controller, int id, string name, string ownerEmail) : base(controller)
        {
            this.Id = id;
            this._name = name;
            this.OwnerEmail = ownerEmail;
        }
    }
}