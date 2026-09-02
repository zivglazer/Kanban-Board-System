using System.Collections.Generic;

namespace IntroSE.Kanban.Backend.ServiceLayer.Entities
{
    public class ColumnSL
    {
        public int ColumnIndex { get; set; }
        public Dictionary<int, TaskSL> Tasks { get; set; }
        public int Limit { get; set; }

        public ColumnSL(int columnIndex, int limit, Dictionary<int,TaskSL> tasks)
        {
            Limit = limit;
            ColumnIndex = columnIndex;
            Tasks = tasks;
        }

        public List<TaskSL> turnTaskToList()
        {
            List<TaskSL> taskSLs = new List<TaskSL>();
            foreach (TaskSL task in Tasks.Values)
            {
                taskSLs.Add(task);
            }
            return taskSLs;
        }
    }
}
