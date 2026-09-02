
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using System.Collections.Generic;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class ColumnDTO : DTO
    {
        public const string ColumnIndexColumnName = "ColumnIndex";
        public const string BoardIDColumnName = "BoardID";
        public const string LimitColumnName = "ColumnLimit";

        private int _limit;

        

        public int BoardID { get; private set; }
        public int ColumnIndex { get; private set; }

        public int Limit
        {
            get => _limit;
            set { _limit = value; ((ColumnController)controller).Update(this); }
        }

        public ColumnDTO(int boardID, int columnIndex, int limit)
        {
            BoardID = boardID;
            ColumnIndex = columnIndex;
            _limit = limit;
            controller = new ColumnController();
        }

        public override Dictionary<string, object> ToColumnValuePairs()
        {
            return new Dictionary<string, object>
            {
                { BoardIDColumnName, BoardID },
                { ColumnIndexColumnName, ColumnIndex },
                { LimitColumnName, Limit }
            };
        }

        public override Dictionary<string, object> PrimaryKey => new Dictionary<string, object>
        {
            { BoardIDColumnName, BoardID },
            { ColumnIndexColumnName, ColumnIndex }
        };
    }
}
