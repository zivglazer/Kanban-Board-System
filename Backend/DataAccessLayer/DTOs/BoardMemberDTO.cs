
using System.Collections.Generic;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class BoardMemberDTO : DTO
    {
        public const string EmailColumnName = "Email";
        public const string BoardIDColumnName = "BoardID";


        public override Dictionary<string, object> PrimaryKey => new Dictionary<string, object>
        {
            { BoardIDColumnName, BoardID },
            { EmailColumnName, Email }
        };

        public string Email;

        public int BoardID;

        public BoardMemberDTO(int id, string email)
        {
            Email = email;
            BoardID = id;
            controller = new BoardMembersController();
        }

        public override Dictionary<string, object> ToColumnValuePairs()
        {
            return new Dictionary<string, object>
            {
                { BoardIDColumnName, BoardID },
                { EmailColumnName, Email }
            };
        }
    }
}
