using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SinglePageApplication.Models
{
    public enum TeacherType { Senior = 1, Junior, Assistent }
    public class Subject
    {
        public int SubjectId { get; set; }
        [Required, StringLength(50), Display(Name = "Subject Name")]
        public string SubjectName { get; set; } = default!;
        [Required, StringLength(50), Display(Name = "Title")]
        public string Title { get; set; } = default!;
        public int TotalHour { get; set; }
        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
    public class Teacher
    {
        public int TeacherId { get; set; }

        [Required, StringLength(50), Display(Name = "Teacher Name")]
        public string TeacherName { get; set; } = default!;
        [Required, Column(TypeName = "date"), Display(Name = "Date of Birth"), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateOfBirth { get; set; } = default!;
        [Required, EnumDataType(typeof(TeacherType))]
        public TeacherType TeacherType { get; set; }
        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; } = default!;
        public virtual ICollection<TeacherStudent> TeacherStudents { get; set; } = new List<TeacherStudent>();

    }
    public class TeacherStudent
    {
        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }
        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public int Fee { get; set; } = default!;
        public Teacher Teacher { get; set; } = default!;
        public Student Student { get; set; } = default!;
    }

    public class Student
    {
        public int StudentId { get; set; }

        [Required, StringLength(50), Display(Name = "Student Name")]
        public string StudentName { get; set; } = default!;
        [Required, Column(TypeName = "money"), DisplayFormat(DataFormatString = "{0:0.00}")]
        public decimal AnnualCost { get; set; }
        [Required, StringLength(150)]
        public string Picture { get; set; } = default!;
        [Display(Name = "Continuing?")]
        public bool Continuing { get; set; }
        public virtual ICollection<TeacherStudent> TeacherStudents { get; set; } = new List<TeacherStudent>();
    }
    
   
    
    
    public class TeacherDbContext : DbContext
    {
        public TeacherDbContext(DbContextOptions<TeacherDbContext> options) : base(options) { }
        public DbSet<Student> Students { get; set; } = default!;
        public DbSet<Teacher> Teachers { get; set; } = default!;
        public DbSet<Subject> Subjects { get; set; } = default!;
        public DbSet<TeacherStudent> TeacherStudents { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeacherStudent>().HasKey(t => new { t.TeacherId, t.StudentId });
        }
    }
}
