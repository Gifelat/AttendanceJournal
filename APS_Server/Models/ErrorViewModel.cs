namespace ASP_Server.Models
{
    public class ErrorViewModel
    {
        public string? RequestID { get; set; }

        public bool ShowRequestID => !string.IsNullOrEmpty(RequestID);
    }
}