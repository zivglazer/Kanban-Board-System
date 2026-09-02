using System;

namespace IntroSE.Kanban.Backend.ServiceLayer.Entities
{
    public class TaskSL
    {
        public int TaskID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreationDate { get; set; }
        public string Assignee { get; internal set; }

        public TaskSL(int taskID, string title, string description, DateTime dueDate, DateTime creationDate, string assignee)
        {
            TaskID = taskID;
            Title = title;
            Description = description;
            DueDate = dueDate;
            CreationDate = creationDate;
            Assignee = assignee;
        }
    }
}
