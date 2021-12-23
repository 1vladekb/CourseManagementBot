using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class GradeType
    {
        public GradeType()
        {
            CourseAssignments = new HashSet<CourseAssignment>();
        }

        public string GradeTypeName { get; set; } = null!;

        public virtual ICollection<CourseAssignment> CourseAssignments { get; set; }
    }
}
