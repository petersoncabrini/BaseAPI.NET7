using System.Diagnostics;

namespace CustomBackend.Infra.Notifications
{
    public class Notification
    {
        public Notification()
        {

        }

        public Notification(Exception exception) : this(exception.Message, NotificationType.Error, exception.ToString()) { }

        public Notification(string message, NotificationType type = NotificationType.Validation, string? detail = null)
        {
            Message = message;
            Detail = detail;
            DateTime = DateTime.UtcNow;
            Type = type;

            Debug.WriteLine($"[{Type}] {message}");
        }

        public string Message { get; set; }
        public string? Detail { get; set; }
        public DateTime DateTime { get; set; }
        public NotificationType Type { get; set; }
    }
}
