using Microsoft.EntityFrameworkCore;
using WebADONET.Models;

namespace WebADONET.Data
{
    public class CollegeDbContext : DbContext
    {
        public CollegeDbContext(DbContextOptions<CollegeDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<Teacher> Teachers => Set<Teacher>();

        // Билдеры (Fluent API): таблицы, столбцы, ключи, ограничения и связи
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Groups (GroupId, GroupName)
            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("Groups");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Id).HasColumnName("GroupId");
                entity.Property(g => g.Name).HasColumnName("GroupName").IsRequired().HasMaxLength(100);
            });

            // Teachers (TeacherId, FirstName, LastName)
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("Teachers");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).HasColumnName("TeacherId");
                entity.Property(t => t.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(t => t.LastName).IsRequired().HasMaxLength(100);
            });

            // Subjects (SubjectId, SubjectName, TeacherId): у предмета один преподаватель
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.ToTable("Subjects");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).HasColumnName("SubjectId");
                entity.Property(s => s.Name).HasColumnName("SubjectName").IsRequired().HasMaxLength(100);

                entity.HasOne(s => s.Teacher)
                      .WithMany(t => t.Subjects)
                      .HasForeignKey(s => s.TeacherId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Students (StudentId, FirstName, LastName, Age, GroupId): у студента одна группа
            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).HasColumnName("StudentId");
                entity.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(s => s.LastName).IsRequired().HasMaxLength(100);

                entity.HasOne(s => s.Group)
                      .WithMany(g => g.Students)
                      .HasForeignKey(s => s.GroupId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Grades (GradeId, StudentId, SubjectId, Score, GradeDate)
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("Grades");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Id).HasColumnName("GradeId");
                entity.Property(g => g.Score).IsRequired();
                entity.Property(g => g.GradeDate).HasColumnType("date").IsRequired();

                entity.HasOne(g => g.Student)
                      .WithMany(s => s.Grades)
                      .HasForeignKey(g => g.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.Subject)
                      .WithMany(s => s.Grades)
                      .HasForeignKey(g => g.SubjectId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
