using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class CourseAssignment
    {
        public CourseAssignment()
        {
            CourseAssignmentChanges = new HashSet<CourseAssignmentChange>();
            CourseAssignmentsResults = new HashSet<CourseAssignmentsResult>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int PinnedCourse { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public string? GradeType { get; set; }
        public int? MaxGradeCount { get; set; }
        public bool? NeedToCompleteHw { get; set; }

        public virtual GradeType? GradeTypeNavigation { get; set; }
        public virtual Course PinnedCourseNavigation { get; set; } = null!;
        public virtual ICollection<CourseAssignmentChange> CourseAssignmentChanges { get; set; }
        public virtual ICollection<CourseAssignmentsResult> CourseAssignmentsResults { get; set; }
    }
}
