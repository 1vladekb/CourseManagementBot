using System;
using System.Collections.Generic;

namespace CourseManagementBot
{
    public partial class CourseJoinHistory
    {
        public int Course { get; set; }
        public string CurrentUser { get; set; } = null!;
        public DateTime? Date { get; set; }
        public string? LogType { get; set; }
        public string? Description { get; set; }

        public virtual Course CourseNavigation { get; set; } = null!;
        public virtual ChattedUser CurrentUserNavigation { get; set; } = null!;
        public virtual LogType? LogTypeNavigation { get; set; }
    }
}
