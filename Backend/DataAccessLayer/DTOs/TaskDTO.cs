
using System;
using System.Collections.Generic;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class TaskDTO : DTO
    {
        public const string IdColumnName = "ID";
        public const string BoardIDColumnName = "BoardID";
        public const string AssigneeColumnName = "Assignee";
        public const string StateColumnName = "State";
        public const string TitleColumnName = "Title";
        public const string DescriptionColumnName = "Description";
        public const string CreationDateColumnName = "CreationDate";
        public const string DueDateColumnName = "DueDate";

        private string _assignee;
        private int _state;
        private string _description;
        private string _title;
        private DateTime _creationDate;
        private DateTime _dueDate;

        public int Id { get; private set; }
        public int BoardID { get; private set; }

        public string Assignee
        {
            get => _assignee;
            set { _assignee = value; controller.Update(this); }
        }
        public int State
        {
            get => _state;
            set { _state = value; controller.Update(this); }
        }
        public string Description
        {
            get => _description;
            set { _description = value; controller.Update(this); }
        }
        public string Title
        {
            get => _title;
            set { _title = value; controller.Update(this); }
        }
        public DateTime CreationDate
        {
            get => _creationDate;
            set { _creationDate = value; controller.Update(this); }
        }
        public DateTime DueDate
        {
            get => _dueDate;
            set { _dueDate = value; controller.Update(this); }
        }

        public override Dictionary<string, object> PrimaryKey => new Dictionary<string, object>
        {
            { BoardIDColumnName, BoardID },
            { IdColumnName , Id }
        };

        public TaskDTO(int id, int boardID, string assignee, int state, string title, string description, DateTime creationDate, DateTime dueDate)
        {
            Id = id;
            BoardID = boardID;
            _assignee = assignee;
            _state = state;
            _title = title;
            _description = description;
            _creationDate = creationDate;
            _dueDate = dueDate;

            controller = new TaskController();
        }

        public override Dictionary<string, object> ToColumnValuePairs()
        {
            return new Dictionary<string, object>
            {
                { IdColumnName, Id },
                { BoardIDColumnName, BoardID },
                { AssigneeColumnName, Assignee },
                { StateColumnName, State },
                { TitleColumnName, Title },
                { DescriptionColumnName, Description },
                { CreationDateColumnName, CreationDate.ToString("yyyy-MM-dd HH:mm:ss") },
                { DueDateColumnName, DueDate.ToString("yyyy-MM-dd HH:mm:ss") }
            };
        }
    }
}
