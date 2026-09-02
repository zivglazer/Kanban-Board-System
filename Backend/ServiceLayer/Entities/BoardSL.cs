namespace IntroSE.Kanban.Backend.ServiceLayer.Entities
{
    public class BoardSL
    {
        public int BoardID {  get; set; }
        public string BoardName { get; set; }

        public string Owner { get; set; }
        public BoardSL(int boardID, string boardName, string owner)
        {
            BoardID = boardID;
            BoardName = boardName;
            Owner = owner;
        }

    }
}
