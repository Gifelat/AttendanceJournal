using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AttendanceJournalLibrary;

public partial class AttendanceJournalContext : DbContext
{
    public AttendanceJournalContext()
    {
    }

    public AttendanceJournalContext(DbContextOptions<AttendanceJournalContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Faculty> Faculties { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build().GetConnectionString("DefaultConnection"));


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceID).HasName("PK__Attendances__CB27876075AE4971");

            entity.Property(e => e.IsPresent).HasColumnType("Present");

            entity.HasOne(d => d.Class).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ClassID)
                .HasConstraintName("FK_Attendance_Classes");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.StudentID)
                .HasConstraintName("FK_Attendances_Students");

			entity.HasOne(d => d.Teacher).WithMany(p => p.Attendances)
				.HasForeignKey(d => d.TeacherID)
				.HasConstraintName("FK_Attendances_Teachers");
		});

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassID).HasName("PK__Class__1B8CEC205205AD53");

            entity.HasIndex(e => e.Theme, "UQ__Class__D4E7DFA8B134C153").IsUnique();

            entity.HasOne(d => d.Subject).WithMany(p => p.Classes)
                .HasForeignKey(d => d.SubjectID)
                .HasConstraintName("FK_Attendances_Students");
        });


        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentID).HasName("PK__Students__1004BEC9E68C4815");

            entity.HasIndex(e => e.FirstName, "UQ__Students__D4E7DFA893F960ED").IsUnique();
            entity.HasIndex(e => e.LastName, "UQ__Students__D4E7DFA893F960EY").IsUnique();

            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);

        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.FacultyID).HasName("PK__Faculty__1004BEC9E68C4815");

            entity.HasIndex(e => e.FacultyName, "UQ__Students__D4E7DFA893F960ED").IsUnique();
            entity.HasIndex(e => e.DepartmentName, "UQ__Students__D4E7DFA893F960EY").IsUnique();
            entity.HasIndex(e => e.GroupName, "UQ__Students__D4E7DFA893F960EZ").IsUnique();

            entity.Property(e => e.FacultyName).HasMaxLength(50);
            entity.Property(e => e.DepartmentName).HasMaxLength(50);
            entity.Property(e => e.GroupName).HasMaxLength(50);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectID).HasName("PK__Teachers__A349B0625CEBB34D");
            entity.HasIndex(e => e.SubjectName, "UQ__Students__D4E7DFA893F960EY").IsUnique();

            entity.Property(e => e.SubjectName).HasMaxLength(50);

        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleID).HasName("PK__Roles__8AFACE1A371F63F0");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F6A988291B").IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(15)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherID).HasName("PK__Teachers__A349B0625CEBB34D");
            entity.HasIndex(e => e.FirstName, "UQ__Students__D4E7DFA893F960ED").IsUnique();
            entity.HasIndex(e => e.LastName, "UQ__Students__D4E7DFA893F960EY").IsUnique();
            entity.HasIndex(e => e.MiddleName, "UQ__Students__D4E7DFA893F960EG").IsUnique();

            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.MiddleName).HasMaxLength(50);

            entity.HasOne(d => d.Faculty).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.FacultyID)
                .HasConstraintName("FK_Attendances_Students");

            entity.HasOne(d => d.Role).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.RoleID)
                .HasConstraintName("FK_Teachers_Roles");

        });
    }




    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
