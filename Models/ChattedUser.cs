using System;
using System.Collections.Generic;

namespace CourseManagementBot.Models
{
    public partial class ChattedUser
    {
        public ChattedUser()
        {
            CourseApprovedByNavigations = new HashSet<Course>();
            CourseAssignmentChanges = new HashSet<CourseAssignmentChange>();
            CourseAssignmentsResults = new HashSet<CourseAssignmentsResult>();
            CourseChanges = new HashSet<CourseChange>();
            CourseCuratorNavigations = new HashSet<Course>();
            CourseJoinHistories = new HashSet<CourseJoinHistory>();
            CourseUsers = new HashSet<CourseUser>();
            Notifications = new HashSet<Notification>();
        }

        public string Id { get; set; } = null!;
        public string ChatId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Email { get; set; }
        public byte[]? Photo { get; set; }

        public virtual UserRole RoleNavigation { get; set; } = null!;
        public virtual ICollection<Course> CourseApprovedByNavigations { get; set; }
        public virtual ICollection<CourseAssignmentChange> CourseAssignmentChanges { get; set; }
        public virtual ICollection<CourseAssignmentsResult> CourseAssignmentsResults { get; set; }
        public virtual ICollection<CourseChange> CourseChanges { get; set; }
        public virtual ICollection<Course> CourseCuratorNavigations { get; set; }
        public virtual ICollection<CourseJoinHistory> CourseJoinHistories { get; set; }
        public virtual ICollection<CourseUser> CourseUsers { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
