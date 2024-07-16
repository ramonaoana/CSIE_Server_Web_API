using BaseLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace ServerLibrary.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<SystemRole> SystemRoles { get; set; }
        public DbSet<UserSystemRole> UserSystemRole { get; set; }
        public DbSet<RefreshTokenInfo> RefreshTokenInfos { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Professor> Professors { get; set; }
        public DbSet<StudyCycle> StudyCycles { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<EducationForm> EducationForms { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<BaseLibrary.Entities.Series> Series { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<ScheduleGroup> ScheduleGroups { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<CourseSemester> CoursesSemesters { get; set; }
        public DbSet<BaseLibrary.Entities.Document> Documents { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<EnrollmentStudentGroup> EnrollmentStudentsGroups { get; set; }
        public DbSet<EnrollmentSemester> EnrollmentSemesters { get; set; }
        public DbSet<StudentDocument> StudentDocuments { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<ExamGrade> ExamGrades { get; set; }
        public DbSet<BaseLibrary.Entities.Language> Languages { get; set; }
        public DbSet<FinanceForm> FinanceFormS { get; set; }
        public DbSet<UniversityLeader> UniversityLeaders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //USER SYSTEM ROLE
#pragma warning restore
            modelBuilder.Entity<UserSystemRole>()
                 .ToTable("UserSystemRoles");
            modelBuilder.Entity<UserSystemRole>()
              .HasKey(l => new { l.UserId, l.RoleId });

            modelBuilder.Entity<UserSystemRole>()
              .HasOne<User>()
              .WithMany()
              .HasForeignKey(u => u.UserId)
              .OnDelete(DeleteBehavior.ClientNoAction)
              .HasConstraintName("FK_UserSystemRole_User");

            modelBuilder.Entity<UserSystemRole>()
               .HasOne<SystemRole>()
               .WithMany()
               .HasForeignKey(u => u.RoleId)
               .OnDelete(DeleteBehavior.ClientNoAction)
               .HasConstraintName("FK_UserSystemRole_SystemRole");


            // RefreshToken 
            modelBuilder.Entity<RefreshTokenInfo>()
             .HasOne<User>()
             .WithMany()
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.ClientNoAction)
             .HasConstraintName("FK_RefreshTokenInfo_User");

            //STUDENT
            modelBuilder.Entity<Professor>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.ClientNoAction)
            .HasConstraintName("FK_Student_User");

            modelBuilder.Entity<Professor>()
            .HasOne<Address>()
            .WithMany()
            .HasForeignKey(p => p.AddressId)
            .OnDelete(DeleteBehavior.ClientNoAction)
            .HasConstraintName("FK_Student_Address");

            //PROFESSOR
            modelBuilder.Entity<Professor>()
           .HasOne<User>()
           .WithMany()
           .HasForeignKey(p => p.UserId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Professor_User");

            modelBuilder.Entity<Professor>()
           .HasOne<Address>()
           .WithMany()
           .HasForeignKey(p => p.AddressId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Professor_Address");

            //SPECIALIZATION
            modelBuilder.Entity<Specialization>()
           .HasOne<StudyCycle>()
           .WithMany()
           .HasForeignKey(p => p.StudyCycleId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Specialization_StudyCycle");

            //ACADEMIC YEAR
            modelBuilder.Entity<AcademicYear>()
           .HasOne<Specialization>()
           .WithMany()
           .HasForeignKey(p => p.SpecializationId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_AcademicYear_Specialization");

            modelBuilder.Entity<AcademicYear>()
           .HasOne<EducationForm>()
           .WithMany()
           .HasForeignKey(p => p.EducationFormId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_AcademicYear_EducationForm");

            modelBuilder.Entity<AcademicYear>()
           .HasOne<Promotion>()
           .WithMany()
           .HasForeignKey(p => p.PromotionId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_AcademicYear_Promotion");

            //SEMESTER

            modelBuilder.Entity<Semester>()
           .HasOne<AcademicYear>()
           .WithMany()
           .HasForeignKey(p => p.AcademicYearId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Semester_AcademicYear");

            //SERIES

            modelBuilder.Entity<Series>()
           .HasOne<AcademicYear>()
           .WithMany()
           .HasForeignKey(p => p.AcademicYearId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Series_AcademicYear");

            //GROUP

            modelBuilder.Entity<Group>()
           .HasOne<Series>()
           .WithMany()
           .HasForeignKey(p => p.SeriesId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Group_Series");

            //SCHEDULE
            modelBuilder.Entity<Schedule>()
            .HasOne<Professor>()
            .WithMany()
            .HasForeignKey(p => p.ProfessorId)
            .OnDelete(DeleteBehavior.ClientNoAction)
            .HasConstraintName("FK_Schedule_Professor");

            modelBuilder.Entity<Schedule>()
            .HasOne<Course>()
            .WithMany()
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.ClientNoAction)
            .HasConstraintName("FK_Schedule_Course");

            modelBuilder.Entity<Schedule>()
           .HasOne<Semester>()
           .WithMany()
           .HasForeignKey(p => p.SemesterId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Schedule_Semester");

            //EXAM
            modelBuilder.Entity<Exam>()
           .HasOne<Schedule>()
           .WithMany()
           .HasForeignKey(p => p.ScheduleId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Schedule_Exam");

            //SCHEDULE GROUP
            //USER SYSTEM ROLE

            modelBuilder.Entity<ScheduleGroup>()
                 .ToTable("ScheduleGroups");
            modelBuilder.Entity<ScheduleGroup>()
              .HasKey(l => new { l.ScheduleId, l.GroupId });

            modelBuilder.Entity<ScheduleGroup>()
              .HasOne<Schedule>()
              .WithMany()
              .HasForeignKey(u => u.ScheduleId)
              .OnDelete(DeleteBehavior.ClientNoAction)
              .HasConstraintName("FK_ScheduleGroup_Schedule");

            modelBuilder.Entity<ScheduleGroup>()
               .HasOne<Group>()
               .WithMany()
               .HasForeignKey(u => u.GroupId)
               .OnDelete(DeleteBehavior.ClientNoAction)
               .HasConstraintName("FK_ScheduleGroup_Group");

            //ENROLLMENT STUDENT - SEMESTER
            //SEMESTER

            modelBuilder.Entity<EnrollmentSemester>()
                 .ToTable("EnrollmentSemesters");
            modelBuilder.Entity<EnrollmentSemester>()
              .HasKey(l => new { l.SemesterId, l.EnrollmentStudentGroupId });

            modelBuilder.Entity<EnrollmentSemester>()
              .HasOne<EnrollmentStudentGroup>()
              .WithMany()
              .HasForeignKey(u => u.EnrollmentStudentGroupId)
              .OnDelete(DeleteBehavior.ClientNoAction)
              .HasConstraintName("FK_EnrollmentSemester_EnrollmentStudentGroupId");

            modelBuilder.Entity<EnrollmentSemester>()
              .HasOne<Semester>()
              .WithMany()
              .HasForeignKey(u => u.SemesterId)
              .OnDelete(DeleteBehavior.ClientNoAction)
              .HasConstraintName("FK_EnrollmentSemester_SemesterId");

            //COURSE
            modelBuilder.Entity<Course>()
           .HasOne<Document>()
           .WithMany()
           .HasForeignKey(p => p.DocumentId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Course_Document");

            //ANNOUNCEMENT

            modelBuilder.Entity<Announcement>()
           .HasOne<Document>()
           .WithMany()
           .HasForeignKey(p => p.DocumentId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Announcement_Document");

            //COURSE SEMESTER

            modelBuilder.Entity<CourseSemester>()
                 .ToTable("CourseSemesters");
            modelBuilder.Entity<CourseSemester>()
              .HasKey(l => new { l.CourseId, l.SemesterId });

            modelBuilder.Entity<CourseSemester>()
              .HasOne<Course>()
              .WithMany()
              .HasForeignKey(u => u.CourseId)
              .OnDelete(DeleteBehavior.ClientNoAction)
              .HasConstraintName("FK_CourseSemester_Course");

            modelBuilder.Entity<CourseSemester>()
                 .HasOne<Semester>()
                 .WithMany()
                 .HasForeignKey(u => u.SemesterId)
                 .OnDelete(DeleteBehavior.ClientNoAction)
                 .HasConstraintName("FK_CourseSemester_Semester");

            //ENROLLMENT STUDENT

            modelBuilder.Entity<EnrollmentStudentGroup>()
               .HasOne<Student>()
               .WithMany()
               .HasForeignKey(u => u.StudentId)
               .OnDelete(DeleteBehavior.ClientNoAction)
               .HasConstraintName("FK_EnrollmentStudentGroup_Student");

            modelBuilder.Entity<EnrollmentStudentGroup>()
               .HasOne<Group>()
               .WithMany()
               .HasForeignKey(u => u.GroupId)
               .OnDelete(DeleteBehavior.ClientNoAction)
               .HasConstraintName("FK_EnrollmentStudentGroup_Group");

            modelBuilder.Entity<EnrollmentStudentGroup>()
               .HasOne<Language>()
               .WithMany()
               .HasForeignKey(u => u.LanguageId)
               .OnDelete(DeleteBehavior.ClientNoAction)
               .HasConstraintName("FK_EnrollmentStudentGroup_Language");

            modelBuilder.Entity<EnrollmentStudentGroup>()
               .HasOne<FinanceForm>()
               .WithMany()
               .HasForeignKey(u => u.FinanceFormId)
               .OnDelete(DeleteBehavior.ClientNoAction)
               .HasConstraintName("FK_EnrollmentStudentGroup_FinanceForm");


            //ENROLLMENT STUDENT SCHEDULE

            modelBuilder.Entity<Enrollment>()
                 .ToTable("Enrollments");
            modelBuilder.Entity<Enrollment>()
              .HasKey(l => new { l.ScheduleId, l.StudentId });

            modelBuilder.Entity<Enrollment>()
              .HasOne<Schedule>()
              .WithMany()
              .HasForeignKey(u => u.ScheduleId)
              .OnDelete(DeleteBehavior.ClientNoAction)
              .HasConstraintName("FK_Enrollment_Schedule");

            modelBuilder.Entity<Enrollment>()
              .HasOne<Student>()
              .WithMany()
              .HasForeignKey(u => u.StudentId)
              .OnDelete(DeleteBehavior.ClientNoAction)
              .HasConstraintName("FK_Enrollment_Student");

            //STUDENT DOCUMENTS

            modelBuilder.Entity<StudentDocument>()
           .HasOne<Professor>()
           .WithMany()
           .HasForeignKey(p => p.StudentId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_StudentDocument_Student");

            //EXAM GRADES

            modelBuilder.Entity<ExamGrade>()
           .HasOne<Exam>()
           .WithMany()
           .HasForeignKey(p => p.ExamId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Grade_Exam");

            modelBuilder.Entity<ExamGrade>()
           .HasOne<Student>()
           .WithMany()
           .HasForeignKey(p => p.StudentId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Grade_Student");

            //LEADERS
            modelBuilder.Entity<UniversityLeader>()
           .HasOne<Professor>()
           .WithMany()
           .IsRequired(false)
           .HasForeignKey(p => p.ProfessorId)
           .OnDelete(DeleteBehavior.ClientNoAction)
           .HasConstraintName("FK_Leader_Professor");
        }
    }
}
