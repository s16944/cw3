using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace cw3.Models
{
    public partial class StudiesDbContext : DbContext
    {
        public StudiesDbContext()
        {
        }

        public StudiesDbContext(DbContextOptions<StudiesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Enrollment> Enrollment { get; set; }
        public virtual DbSet<RefreshTokens> RefreshTokens { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Student> Student { get; set; }
        public virtual DbSet<StudentRoles> StudentRoles { get; set; }
        public virtual DbSet<Studies> Studies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=db-mssql;Initial Catalog=s16944;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.IdEnrollment)
                    .HasName("Enrollment_pk");

                entity.Property(e => e.IdEnrollment).ValueGeneratedNever();

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.HasOne(d => d.IdStudyNavigation)
                    .WithMany(p => p.Enrollment)
                    .HasForeignKey(d => d.IdStudy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollment_Studies");
            });

            modelBuilder.Entity<RefreshTokens>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.IndexNumber)
                    .HasColumnName("indexNumber")
                    .HasMaxLength(100);

                entity.Property(e => e.Token)
                    .HasColumnName("token")
                    .HasMaxLength(36)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Validity)
                    .HasColumnName("validity")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.IndexNumberNavigation)
                    .WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.IndexNumber)
                    .HasConstraintName("FK__RefreshTo__index__32767D0B");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("UQ__ROLES__72E12F1BA65AEF53")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.IndexNumber)
                    .HasName("Student_pk");

                entity.Property(e => e.IndexNumber).HasMaxLength(100);

                entity.Property(e => e.BirthDate).HasColumnType("date");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Password).HasMaxLength(100);

                entity.Property(e => e.Salt)
                    .HasMaxLength(24)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.IdEnrollmentNavigation)
                    .WithMany(p => p.Student)
                    .HasForeignKey(d => d.IdEnrollment)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Student_Enrollment");
            });

            modelBuilder.Entity<StudentRoles>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.IndexNumber)
                    .HasColumnName("indexNumber")
                    .HasMaxLength(100);

                entity.Property(e => e.RoleId).HasColumnName("roleId");

                entity.HasOne(d => d.IndexNumberNavigation)
                    .WithMany(p => p.StudentRoles)
                    .HasForeignKey(d => d.IndexNumber)
                    .HasConstraintName("FK__StudentRo__index__793DFFAF");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.StudentRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__StudentRo__roleI__7849DB76");
            });

            modelBuilder.Entity<Studies>(entity =>
            {
                entity.HasKey(e => e.IdStudy)
                    .HasName("Studies_pk");

                entity.Property(e => e.IdStudy).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
