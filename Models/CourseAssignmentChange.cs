using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class CourseAssignmentChange
    {
        public int Id { get; set; }
        public int? CourseAssignment { get; set; }
        public string? ChangedName { get; set; }
        public DateTime? ChangedEndDate { get; set; }
        public string? ChangedDescription { get; set; }
        public string? ChangedGradeType { get; set; }
        public int? ChangedMaxGradeCount { get; set; }
        public bool? NeedToCompleteHw { get; set; }
        public DateTime SendChangesDate { get; set; }
        public DateTime? ChangesCheckDate { get; set; }
        public string? ChangedCheckedBy { get; set; }
        public bool? Approved { get; set; }
        public string? ChangeDescription { get; set; }

        public virtual ChattedUser? ChangedCheckedByNavigation { get; set; }
        public virtual CourseAssignment? CourseAssignmentNavigation { get; set; }
    }
}
