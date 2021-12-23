using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class NotificationType
    {
        public NotificationType()
        {
            Notifications = new HashSet<Notification>();
        }

        public string NotificationTypeName { get; set; } = null!;

        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
