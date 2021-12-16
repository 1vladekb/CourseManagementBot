using System;
using System.Collections.Generic;

namespace CourseManagementBot
{
    public partial class CourseAssignmentsResult
    {
        public string CurrentUser { get; set; } = null!;
        public int Assignment { get; set; }
        public DateTime CompletionDate { get; set; }
        public int? Grade { get; set; }
        public int Attempt { get; set; }

        public virtual CourseAssignment AssignmentNavigation { get; set; } = null!;
        public virtual ChattedUser CurrentUserNavigation { get; set; } = null!;
    }
}
