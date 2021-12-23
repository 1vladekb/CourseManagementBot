using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class Course
    {
        public Course()
        {
            CourseAssignments = new HashSet<CourseAssignment>();
            CourseChanges = new HashSet<CourseChange>();
            CourseJoinHistories = new HashSet<CourseJoinHistory>();
            CourseUsers = new HashSet<CourseUser>();
            Courses = new HashSet<Course>();
            GlobalCourses = new HashSet<Course>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsGlobal { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ApprovedBy { get; set; }
        public string Curator { get; set; } = null!;
        public string? Requisites { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int? MembersCount { get; set; }
        public string? Token { get; set; }

        public virtual ChattedUser? ApprovedByNavigation { get; set; }
        public virtual ChattedUser CuratorNavigation { get; set; } = null!;
        public virtual ActiveToken? TokenNavigation { get; set; }
        public virtual ICollection<CourseAssignment> CourseAssignments { get; set; }
        public virtual ICollection<CourseChange> CourseChanges { get; set; }
        public virtual ICollection<CourseJoinHistory> CourseJoinHistories { get; set; }
        public virtual ICollection<CourseUser> CourseUsers { get; set; }

        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<Course> GlobalCourses { get; set; }
    }
}
