using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class LogType
    {
        public LogType()
        {
            CourseJoinHistories = new HashSet<CourseJoinHistory>();
        }

        public string LogTypeName { get; set; } = null!;

        public virtual ICollection<CourseJoinHistory> CourseJoinHistories { get; set; }
    }
}
