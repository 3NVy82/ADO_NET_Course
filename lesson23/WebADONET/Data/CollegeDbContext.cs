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

        // Билдеры (Fluent API): таблицы, ключи, ограничения и связи
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Groups
            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("Groups");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
            });

            // Teachers
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("Teachers");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(t => t.LastName).IsRequired().HasMaxLength(100);
            });

            // Subjects: у предмета один преподаватель, у преподавателя много предметов
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.ToTable("Subjects");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(100);

                entity.HasOne(s => s.Teacher)
                      .WithMany(t => t.Subjects)
                      .HasForeignKey(s => s.TeacherId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Students: у студента одна группа, в группе много студентов
            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(s => s.LastName).IsRequired().HasMaxLength(100);

                entity.HasOne(s => s.Group)
                      .WithMany(g => g.Students)
                      .HasForeignKey(s => s.GroupId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Grades: оценка принадлежит студенту и предмету
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("Grades");
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Value).IsRequired();

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
