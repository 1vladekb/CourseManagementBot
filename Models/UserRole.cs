using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class UserRole
    {
        public UserRole()
        {
            ChattedUsers = new HashSet<ChattedUser>();
        }

        public string RoleName { get; set; } = null!;

        public virtual ICollection<ChattedUser> ChattedUsers { get; set; }
    }
}
