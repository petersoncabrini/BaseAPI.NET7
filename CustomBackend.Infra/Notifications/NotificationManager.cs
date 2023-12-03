using CustomBackend.Infra.Utils;

namespace CustomBackend.Infra.Notifications
{
    public class NotificationManager
    {
        private List<Notification> items = new List<Notification>();

        public void Add(params Notification[] notifications)
        {
            notifications = notifications?.Where(e => e != null)?.ToArray();

            if (notifications.IsNullOrEmpty())
                return;

            items.AddRange(notifications);
        }

        public void Add(Exception ex)
        {
            if (ex == null)
                return;

            items.Add(new Notification(ex));
        }

        public void Add(string message, NotificationType type = NotificationType.Validation, string? detail = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            items.Add(new Notification(message, type, detail));
        }

        public bool IsValid() => !Any(NotificationType.Error, NotificationType.Validation, NotificationType.Authentication, NotificationType.Authorization, NotificationType.NotFound);

        public bool Any() => items.Any();

        public bool Any(params NotificationType[] types) => items.Any(e => types.Contains(e.Type));

        public void Clear() => items.Clear();

        public List<Notification> List() => items;
    }
}
