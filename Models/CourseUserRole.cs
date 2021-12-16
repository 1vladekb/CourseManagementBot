using System;
using System.Collections.Generic;

namespace CourseManagementBot
{
    public partial class CourseUserRole
    {
        public CourseUserRole()
        {
            CourseUsers = new HashSet<CourseUser>();
        }

        public string RoleName { get; set; } = null!;

        public virtual ICollection<CourseUser> CourseUsers { get; set; }
    }
}
