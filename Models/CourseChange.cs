using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class CourseChange
    {
        public int Id { get; set; }
        public int? Course { get; set; }
        public string? ChangedName { get; set; }
        public string? ChangedDescription { get; set; }
        public bool? IsGlobal { get; set; }
        public DateTime? ChangedEndDate { get; set; }
        public string? ChangedRequisites { get; set; }
        public bool? IsPrivate { get; set; }
        public int? ChangedMembersCount { get; set; }
        public DateTime SendChangesDate { get; set; }
        public DateTime? ChangesCheckDate { get; set; }
        public string? ChangesCheckedBy { get; set; }
        public bool? Approved { get; set; }
        public string? ChangeDescription { get; set; }

        public virtual ChattedUser? ChangesCheckedByNavigation { get; set; }
        public virtual Course? CourseNavigation { get; set; }
    }
}
