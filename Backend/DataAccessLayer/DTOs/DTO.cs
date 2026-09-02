
using System.Collections.Generic;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal abstract class DTO
    {

        public abstract Dictionary<string, object> PrimaryKey { get; }

        protected Controller controller;
        protected DTO() { }
        
        public abstract Dictionary<string, object> ToColumnValuePairs();

        //Insert DTO to the DB.
        public bool Insert()
        {
            return controller.Insert(this);
        }
        //Delete DTO from the DB.
        public bool Delete()
        {
            return controller.Delete(this);
        }
        //Update DTO in the DB.
        public bool Update()
        {
            
            return controller.Update(this);
        }
    }
}
