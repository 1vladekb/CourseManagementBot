using System;
using System.Collections.Generic;

namespace CourseManagementBot
{
    public partial class CourseAssignment
    {
        public CourseAssignment()
        {
            CourseAssignmentsResults = new HashSet<CourseAssignmentsResult>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int PinnedCourse { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }

        public virtual Course PinnedCourseNavigation { get; set; } = null!;
        public virtual ICollection<CourseAssignmentsResult> CourseAssignmentsResults { get; set; }
    }
}
