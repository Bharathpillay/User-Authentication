using Microsoft.EntityFrameworkCore;
using SimpleSite.Models;

namespace SimpleSite.Data
{
    public class SimpleSiteDbContext : DbContext
    {
        public SimpleSiteDbContext(DbContextOptions<SimpleSiteDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<StaffSubject> StaffSubjects { get; set; }
        public DbSet<DepartmentSubject> DepartmentSubjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<StaffSubject>()
                .HasKey(ss => new { ss.UserId, ss.SubjectId });

            modelBuilder.Entity<StaffSubject>()
                .HasOne(ss => ss.User)
                .WithMany(u => u.StaffSubjects)
                .HasForeignKey(ss => ss.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffSubject>()
                .HasOne(ss => ss.Subject)
                .WithMany(s => s.StaffSubjects)
                .HasForeignKey(ss => ss.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

           
            modelBuilder.Entity<DepartmentSubject>()
                .HasKey(ds => new { ds.DepartmentId, ds.SubjectId });

            modelBuilder.Entity<DepartmentSubject>()
                .HasOne(ds => ds.Department)
                .WithMany(d => d.DepartmentSubjects)
                .HasForeignKey(ds => ds.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DepartmentSubject>()
                .HasOne(ds => ds.Subject)
                .WithMany(s => s.DepartmentSubjects)
                .HasForeignKey(ds => ds.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}