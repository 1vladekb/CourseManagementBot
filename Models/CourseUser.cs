using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class CourseUser
    {
        public int Course { get; set; }
        public string PinnedUser { get; set; } = null!;
        public string CourseUserRole { get; set; } = null!;
        public bool Admitted { get; set; }
        public DateTime? CourseJoinDate { get; set; }
        public DateTime LeavingDate { get; set; }

        public virtual Course CourseNavigation { get; set; } = null!;
        public virtual CourseUserRole CourseUserRoleNavigation { get; set; } = null!;
        public virtual ChattedUser PinnedUserNavigation { get; set; } = null!;
    }
}
