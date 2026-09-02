
using System.Collections.Generic;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class BoardDTO : DTO
    {
        public const string IDColumnName = "BoardID";
        public const string NameColumnName = "Name";
        public const string OwnerColumnName = "Owner";
        
        private string _owner;
        
        public int ID { get; private set; }
        
        public string Name { get; private set; }

        public string Owner
        {
            get => _owner;
            set { _owner = value; controller.Update(this); }
        }

        public BoardDTO(int id, string name, string owner)
        {
            ID = id;
            _owner = owner;
            Name = name;
            controller = new BoardController();
        }

        public override Dictionary<string, object> PrimaryKey => new Dictionary<string, object>
        {
            { IDColumnName, ID }
        };

        public override Dictionary<string, object> ToColumnValuePairs()
        {
            return new Dictionary<string, object>
            {
                { IDColumnName, ID },
                { NameColumnName, Name },
                { OwnerColumnName, Owner }
            };
        }
    }
}
