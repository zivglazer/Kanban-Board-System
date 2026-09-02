using System;

namespace Frontend.Model
{
    public class TaskModel : NotifiableModelObject
    {
        public int Id { get; }
        public DateTime CreationTime { get; }
        private DateTime _dueDate;
        public DateTime DueDate { get => _dueDate; set { _dueDate = value; RaisePropertyChanged("DueDate"); } }
        private string _title;
        public string Title { get => _title; set { _title = value; RaisePropertyChanged("Title"); } }
        private string _description;
        public string Description { get => _description; set { _description = value; RaisePropertyChanged("Description"); } }
        private string _assigneeEmail;
        public string AssigneeEmail { get => _assigneeEmail; set { _assigneeEmail = value; RaisePropertyChanged("AssigneeEmail"); } }
        private int _columnIndex;
        public int ColumnIndex { get => _columnIndex; set { _columnIndex = value; RaisePropertyChanged("ColumnIndex"); } }

        public TaskModel(BackendController controller, int id, DateTime creationTime, DateTime dueDate, string title, string description, string assigneeEmail, int columnIndex) : base(controller)
        {
            this.Id = id;
            this.CreationTime = creationTime;
            this._dueDate = dueDate;
            this._title = title;
            this._description = description;
            this._assigneeEmail = assigneeEmail;
            this._columnIndex = columnIndex;
        }
    }
}