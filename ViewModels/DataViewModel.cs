using SinglePageApplication.Models;

namespace SinglePageApplication.ViewModels
{
    public class DataViewModel
    {
        public int SelectedTeacherId { get; set; }
        public IEnumerable<Student> Students { get; set; } = default!;
        public IEnumerable<Subject> Subjects { get; set; } = default!;
        public IEnumerable<Teacher> Teachers { get; set; } = default!;
        public IEnumerable<TeacherStudent> TeacherStudents { get; set; } = default!;
    }
}
