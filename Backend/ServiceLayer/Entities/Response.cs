using System.Text.Json;

namespace IntroSE.Kanban.Backend.ServiceLayer.Entities
{
    public class Response<T>
    {
        public string ErrorMessage { get; set; }
        public T? ReturnValue { get; set; }

        public bool ErrorOccured { get => !string.IsNullOrEmpty(ErrorMessage); }
        
        public Response() { }
        public Response(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public Response(T val, string errorMsg)
        {
            ReturnValue = val;
            ErrorMessage = errorMsg;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

    }
}
