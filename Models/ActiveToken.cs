using System;
using System.Collections.Generic;

namespace CourseManagementBot
{
    public partial class ActiveToken
    {
        public ActiveToken()
        {
            Courses = new HashSet<Course>();
        }

        public string Token { get; set; } = null!;
        public string TokenType { get; set; } = null!;
        public int MaxUsesNumber { get; set; }
        public int UsedAttempts { get; set; }

        public virtual TokenType TokenTypeNavigation { get; set; } = null!;
        public virtual ICollection<Course> Courses { get; set; }
    }
}
