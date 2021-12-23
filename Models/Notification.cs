using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class Notification
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string FromUser { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string NotificationValue { get; set; } = null!;

        public virtual ChattedUser FromUserNavigation { get; set; } = null!;
        public virtual NotificationType TypeNavigation { get; set; } = null!;
    }
}
