﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CourseManagementBot.Models
{
    public partial class CourseManagementDataContext : DbContext
    {
        public CourseManagementDataContext()
        {
        }

        public CourseManagementDataContext(DbContextOptions<CourseManagementDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ActiveToken> ActiveTokens { get; set; } = null!;
        public virtual DbSet<ChattedUser> ChattedUsers { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<CourseAssignment> CourseAssignments { get; set; } = null!;
        public virtual DbSet<CourseAssignmentChange> CourseAssignmentChanges { get; set; } = null!;
        public virtual DbSet<CourseAssignmentsResult> CourseAssignmentsResults { get; set; } = null!;
        public virtual DbSet<CourseChange> CourseChanges { get; set; } = null!;
        public virtual DbSet<CourseJoinHistory> CourseJoinHistories { get; set; } = null!;
        public virtual DbSet<CourseUser> CourseUsers { get; set; } = null!;
        public virtual DbSet<CourseUserRole> CourseUserRoles { get; set; } = null!;
        public virtual DbSet<GradeType> GradeTypes { get; set; } = null!;
        public virtual DbSet<LogType> LogTypes { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<NotificationType> NotificationTypes { get; set; } = null!;
        public virtual DbSet<TokenType> TokenTypes { get; set; } = null!;
        public virtual DbSet<UserRole> UserRoles { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=HOME-PC;Database=CourseManagementData;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActiveToken>(entity =>
            {
                entity.HasKey(e => e.Token);

                entity.Property(e => e.Token).HasMaxLength(50);

                entity.Property(e => e.TokenType).HasMaxLength(100);

                entity.HasOne(d => d.TokenTypeNavigation)
                    .WithMany(p => p.ActiveTokens)
                    .HasForeignKey(d => d.TokenType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActiveTokens_TokenTypes");
            });

            modelBuilder.Entity<ChattedUser>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(30)
                    .HasColumnName("ID");

                entity.Property(e => e.ChatId).HasColumnName("ChatID");

                entity.Property(e => e.Email).HasMaxLength(80);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.MiddleName).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Photo).HasColumnType("image");

                entity.Property(e => e.Role).HasMaxLength(30);

                entity.HasOne(d => d.RoleNavigation)
                    .WithMany(p => p.ChattedUsers)
                    .HasForeignKey(d => d.Role)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChattedUsers_UserRoles");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ApprovalDate).HasColumnType("date");

                entity.Property(e => e.ApprovedBy).HasMaxLength(30);

                entity.Property(e => e.Curator).HasMaxLength(30);

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.PublishDate).HasColumnType("date");

                entity.Property(e => e.Token).HasMaxLength(50);

                entity.HasOne(d => d.ApprovedByNavigation)
                    .WithMany(p => p.CourseApprovedByNavigations)
                    .HasForeignKey(d => d.ApprovedBy)
                    .HasConstraintName("FK_Courses_ChattedUsers");

                entity.HasOne(d => d.CuratorNavigation)
                    .WithMany(p => p.CourseCuratorNavigations)
                    .HasForeignKey(d => d.Curator)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Courses_ChattedUsers1");

                entity.HasOne(d => d.TokenNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.Token)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Courses_ActiveTokens");

                entity.HasMany(d => d.Courses)
                    .WithMany(p => p.GlobalCourses)
                    .UsingEntity<Dictionary<string, object>>(
                        "GlobalCoursesComposition",
                        l => l.HasOne<Course>().WithMany().HasForeignKey("Course").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_GlobalCoursesComposition_Courses1"),
                        r => r.HasOne<Course>().WithMany().HasForeignKey("GlobalCourse").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_GlobalCoursesComposition_Courses"),
                        j =>
                        {
                            j.HasKey("GlobalCourse", "Course");

                            j.ToTable("GlobalCoursesComposition");
                        });

                entity.HasMany(d => d.GlobalCourses)
                    .WithMany(p => p.Courses)
                    .UsingEntity<Dictionary<string, object>>(
                        "GlobalCoursesComposition",
                        l => l.HasOne<Course>().WithMany().HasForeignKey("GlobalCourse").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_GlobalCoursesComposition_Courses"),
                        r => r.HasOne<Course>().WithMany().HasForeignKey("Course").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_GlobalCoursesComposition_Courses1"),
                        j =>
                        {
                            j.HasKey("GlobalCourse", "Course");

                            j.ToTable("GlobalCoursesComposition");
                        });
            });

            modelBuilder.Entity<CourseAssignment>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.GradeType).HasMaxLength(20);

                entity.Property(e => e.Name).HasMaxLength(150);

                entity.Property(e => e.NeedToCompleteHw).HasColumnName("NeedToCompleteHW");

                entity.HasOne(d => d.GradeTypeNavigation)
                    .WithMany(p => p.CourseAssignments)
                    .HasForeignKey(d => d.GradeType)
                    .HasConstraintName("FK_CourseAssignments_GradeTypes");

                entity.HasOne(d => d.PinnedCourseNavigation)
                    .WithMany(p => p.CourseAssignments)
                    .HasForeignKey(d => d.PinnedCourse)
                    .HasConstraintName("FK_CourseAssignments_Courses");
            });

            modelBuilder.Entity<CourseAssignmentChange>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ChangedCheckedBy).HasMaxLength(30);

                entity.Property(e => e.ChangedEndDate).HasColumnType("date");

                entity.Property(e => e.ChangedGradeType).HasMaxLength(20);

                entity.Property(e => e.ChangedName).HasMaxLength(150);

                entity.Property(e => e.ChangesCheckDate).HasColumnType("date");

                entity.Property(e => e.NeedToCompleteHw).HasColumnName("NeedToCompleteHW");

                entity.Property(e => e.SendChangesDate).HasColumnType("date");

                entity.HasOne(d => d.ChangedCheckedByNavigation)
                    .WithMany(p => p.CourseAssignmentChanges)
                    .HasForeignKey(d => d.ChangedCheckedBy)
                    .HasConstraintName("FK_CourseAssignmentChanges_ChattedUsers");

                entity.HasOne(d => d.CourseAssignmentNavigation)
                    .WithMany(p => p.CourseAssignmentChanges)
                    .HasForeignKey(d => d.CourseAssignment)
                    .HasConstraintName("FK_CourseAssignmentChanges_CourseAssignments");
            });

            modelBuilder.Entity<CourseAssignmentsResult>(entity =>
            {
                entity.HasKey(e => new { e.CurrentUser, e.Assignment });

                entity.Property(e => e.CurrentUser).HasMaxLength(30);

                entity.Property(e => e.CompletionDate).HasColumnType("date");

                entity.HasOne(d => d.AssignmentNavigation)
                    .WithMany(p => p.CourseAssignmentsResults)
                    .HasForeignKey(d => d.Assignment)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CourseAssignmentsResults_CourseAssignments");

                entity.HasOne(d => d.CurrentUserNavigation)
                    .WithMany(p => p.CourseAssignmentsResults)
                    .HasForeignKey(d => d.CurrentUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CourseAssignmentsResults_ChattedUsers");
            });

            modelBuilder.Entity<CourseChange>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ChangedEndDate).HasColumnType("date");

                entity.Property(e => e.ChangedName).HasMaxLength(200);

                entity.Property(e => e.ChangesCheckDate).HasColumnType("date");

                entity.Property(e => e.ChangesCheckedBy).HasMaxLength(30);

                entity.Property(e => e.SendChangesDate).HasColumnType("date");

                entity.HasOne(d => d.ChangesCheckedByNavigation)
                    .WithMany(p => p.CourseChanges)
                    .HasForeignKey(d => d.ChangesCheckedBy)
                    .HasConstraintName("FK_CourseChanges_ChattedUsers1");

                entity.HasOne(d => d.CourseNavigation)
                    .WithMany(p => p.CourseChanges)
                    .HasForeignKey(d => d.Course)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_CourseChanges_Courses");
            });

            modelBuilder.Entity<CourseJoinHistory>(entity =>
            {
                entity.HasKey(e => new { e.Course, e.CurrentUser });

                entity.ToTable("CourseJoinHistory");

                entity.Property(e => e.CurrentUser).HasMaxLength(30);

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.LogType).HasMaxLength(100);

                entity.HasOne(d => d.CourseNavigation)
                    .WithMany(p => p.CourseJoinHistories)
                    .HasForeignKey(d => d.Course)
                    .HasConstraintName("FK_CourseJoinHistory_Courses");

                entity.HasOne(d => d.CurrentUserNavigation)
                    .WithMany(p => p.CourseJoinHistories)
                    .HasForeignKey(d => d.CurrentUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CourseJoinHistory_ChattedUsers");

                entity.HasOne(d => d.LogTypeNavigation)
                    .WithMany(p => p.CourseJoinHistories)
                    .HasForeignKey(d => d.LogType)
                    .HasConstraintName("FK_CourseJoinHistory_LogTypes");
            });

            modelBuilder.Entity<CourseUser>(entity =>
            {
                entity.HasKey(e => new { e.Course, e.PinnedUser });

                entity.Property(e => e.PinnedUser).HasMaxLength(30);

                entity.Property(e => e.CourseJoinDate).HasColumnType("date");

                entity.Property(e => e.CourseUserRole).HasMaxLength(50);

                entity.Property(e => e.LeavingDate).HasColumnType("date");

                entity.HasOne(d => d.CourseNavigation)
                    .WithMany(p => p.CourseUsers)
                    .HasForeignKey(d => d.Course)
                    .HasConstraintName("FK_CourseUsers_Courses");

                entity.HasOne(d => d.CourseUserRoleNavigation)
                    .WithMany(p => p.CourseUsers)
                    .HasForeignKey(d => d.CourseUserRole)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CourseUsers_CourseUserRoles");

                entity.HasOne(d => d.PinnedUserNavigation)
                    .WithMany(p => p.CourseUsers)
                    .HasForeignKey(d => d.PinnedUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CourseUsers_ChattedUsers");
            });

            modelBuilder.Entity<CourseUserRole>(entity =>
            {
                entity.HasKey(e => e.RoleName);

                entity.Property(e => e.RoleName).HasMaxLength(50);
            });

            modelBuilder.Entity<GradeType>(entity =>
            {
                entity.HasKey(e => e.GradeTypeName);

                entity.Property(e => e.GradeTypeName).HasMaxLength(20);
            });

            modelBuilder.Entity<LogType>(entity =>
            {
                entity.HasKey(e => e.LogTypeName);

                entity.Property(e => e.LogTypeName).HasMaxLength(100);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.FromUser).HasMaxLength(30);

                entity.Property(e => e.Type).HasMaxLength(100);

                entity.HasOne(d => d.FromUserNavigation)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.FromUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Notifications_ChattedUsers");

                entity.HasOne(d => d.TypeNavigation)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.Type)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Notifications_NotificationTypes");
            });

            modelBuilder.Entity<NotificationType>(entity =>
            {
                entity.HasKey(e => e.NotificationTypeName);

                entity.Property(e => e.NotificationTypeName).HasMaxLength(100);
            });

            modelBuilder.Entity<TokenType>(entity =>
            {
                entity.HasKey(e => e.TokenTypeName);

                entity.Property(e => e.TokenTypeName).HasMaxLength(100);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.RoleName);

                entity.Property(e => e.RoleName).HasMaxLength(30);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
