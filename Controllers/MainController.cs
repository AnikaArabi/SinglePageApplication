using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinglePageApplication.Models;
using SinglePageApplication.ViewModels;

namespace SinglePageApplication.Controllers
{
    public class MainController : Controller
    {
        TeacherDbContext db;
        IWebHostEnvironment henv;
        public MainController(TeacherDbContext db, IWebHostEnvironment henv)
        {
            this.db = db;
            this.henv = henv;
        }

        public async Task<IActionResult> Index()
        {
            var id = 0;
            if (db.Teachers.Any())
            {
                id = db.Teachers.ToList()[0].TeacherId;
            }

            DataViewModel data = new DataViewModel();
            data.SelectedTeacherId = id;
            data.Subjects = await db.Subjects.ToListAsync();
            data.Students = await db.Students.ToListAsync();
            data.Teachers = await db.Teachers.ToListAsync();
            data.TeacherStudents = await db.TeacherStudents.Where(oi => oi.TeacherId == id).ToListAsync();


            return View(data);
        }
        public async Task<IActionResult> GetSelectedTeacherStudents(int id)
        {

            var TeacherStudents = await db.TeacherStudents.Include(x => x.Student).Where(oi => oi.StudentId == id).ToListAsync();
            return PartialView("_TeacherStudentTable", TeacherStudents);
        }
        public IActionResult CreateSubject()
        {
            return PartialView("_CreateSubject");
        }
        [HttpPost]
        public async Task<IActionResult> CreateSubject(Subject s)
        {
            if (ModelState.IsValid)
            {
                await db.Subjects.AddAsync(s);
                await db.SaveChangesAsync();
                return Json(s);
            }
            return BadRequest("Unexpected error");
        }
        public async Task<IActionResult> EditSubject(int id)
        {
            var data = await db.Subjects.FirstOrDefaultAsync(c => c.SubjectId == id);
            return PartialView("_EditSubject", data);
        }
        [HttpPost]
        public async Task<IActionResult> EditSubject(Subject s)
        {
            if (ModelState.IsValid)
            {
                db.Entry(s).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Json(s);
            }
            return BadRequest("Unexpected error");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            if (!await db.Teachers.AnyAsync(o => o.SubjectId == id))
            {
                var o = new Subject { SubjectId = id };
                db.Entry(o).State = EntityState.Deleted;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                return Json(new { success = true, message = "Data deleted" });
            }
            return Json(new { success = false, message = "Cannot delete, item has related child." });
        }
        public IActionResult CreateStudent()
        {
            return PartialView("_CreateStudent");
        }
        [HttpPost]
        public async Task<IActionResult> CreateStudent(StudentInputModel s)
        {
            if (ModelState.IsValid)
            {
                var student = new Student { StudentName = s.StudentName, AnnualCost = s.AnnualCost, Continuing=s.Continuing};
                string fileName = Guid.NewGuid() + Path.GetExtension(s.Picture.FileName);
                string savePath = Path.Combine(this.henv.WebRootPath, "Pictures", fileName);
                var fs = new FileStream(savePath, FileMode.Create);
                s.Picture.CopyTo(fs);
                fs.Close();
                student.Picture = fileName;
                await db.Students.AddAsync(student);
                await db.SaveChangesAsync();
                return Json(student);


            }
            return BadRequest("Falied to insert student");
        }
        public async Task<IActionResult> EditStudent(int id)
        {
            var data = await db.Students.FirstAsync(x => x.StudentId == id);
            ViewData["CurrentPic"] = data.Picture;
            return PartialView("_EditStudent", new StudentEditModel { StudentId = data.StudentId, StudentName = data.StudentName, AnnualCost = data.AnnualCost, Continuing = data.Continuing });
        }
        [HttpPost]
        public async Task<IActionResult> EditStudent(StudentEditModel s)
        {
            if (ModelState.IsValid)
            {
                var student = await db.Students.FirstAsync(x => x.StudentId == s.StudentId);
                student.StudentName = s.StudentName;
                student.AnnualCost = s.AnnualCost;
                student.Continuing = s.Continuing;
                if (s.Picture != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(s.Picture.FileName);
                    string savePath = Path.Combine(this.henv.WebRootPath, "Pictures", fileName);
                    var fs = new FileStream(savePath, FileMode.Create);
                    s.Picture.CopyTo(fs);
                    fs.Close();
                    student.Picture = fileName;
                }


                await db.SaveChangesAsync();
                return Json(student);


            }
            return BadRequest();
        }
        public async Task<IActionResult> DeleteStudent(int id)
        {
            if (!await db.TeacherStudents.AnyAsync(o => o.StudentId == id))
            {
                var o = new Student { StudentId = id };
                db.Entry(o).State = EntityState.Deleted;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                return Json(new { success = true, message = "Data deleted" });
            }
            return Json(new { success = false, message = "Cannot delete, item has related child." });
        }
        public async Task<IActionResult> CreateTeacher()
        {
            ViewData["Students"] = await db.Students.ToListAsync();
            ViewData["Subjects"] = await db.Subjects.ToListAsync();
            return PartialView("_CreateTeacher");
        }
        [HttpPost]
        public async Task<IActionResult> CreateTeacher(Teacher o, int[] StudentID, int[] Fee)
        {
            if (ModelState.IsValid)
            {
                for (var i = 0; i < StudentID.Length; i++)
                {
                    o.TeacherStudents.Add(new TeacherStudent { StudentId = StudentID[i], Fee = Fee[i] });
                }
                await db.Teachers.AddAsync(o);

                await db.SaveChangesAsync();


                var tr = await GetTeacher(o.TeacherId);
                return Json(tr);
            }
            return BadRequest();
        }
        public async Task<IActionResult> EditTeacher(int id)
        {
            ViewData["Students"] = await db.Students.ToListAsync();
            ViewData["Subjects"] = await db.Subjects.ToListAsync();
            var data = await db.Teachers
                .Include(x => x.TeacherStudents).ThenInclude(x => x.Student)
                .FirstOrDefaultAsync(x => x.TeacherId == id);
            return PartialView("_EditTeacher", data);

        }
        [HttpPost]
        public async Task<IActionResult> EditTeacher(Teacher o)
        {
            if (ModelState.IsValid)
            {
                var existing = await db.Teachers.Include(x => x.Subject).FirstAsync(x => x.TeacherId == o.TeacherId);
                existing.TeacherName = o.TeacherName;
                existing.DateOfBirth = o.DateOfBirth;
                existing.SubjectId = o.SubjectId;

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                return Json(existing);
            }

            return BadRequest();
        }
        private async Task<Teacher?> GetTeacher(int id)
        {
            var o = await db.Teachers.Include(x => x.Subject).FirstOrDefaultAsync(x => x.TeacherId == id);
            return o;
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var o = new Teacher { TeacherId = id };
            db.Entry(o).State = EntityState.Deleted;
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Data deleted" });
        }
        public async Task<IActionResult> CreateItem()
        {
            ViewData["Students"] = await db.Students.ToListAsync();
            return PartialView("_CreateItem");
        }
        public async Task<IActionResult> CreateTeacherStudent(int id)
        {
            ViewData["TeacherId"] = id;
            ViewData["Students"] = await db.Students.ToListAsync();
            return PartialView("_CreateTeacherStudent");
        }
        [HttpPost]
        public async Task<IActionResult> CreateTeacherStudent(TeacherStudent ts)
        {
            if (ModelState.IsValid)
            {
                await db.TeacherStudents.AddAsync(ts);
                await db.SaveChangesAsync();
                var o = await GetTeacherStudent(ts.TeacherId, ts.StudentId);
                return Json(o);
            }
            return BadRequest();
        }
        public async Task<IActionResult> EditTeacherStudent(int tid, int sid)
        {
            ViewData["Students"] = await db.Students.ToListAsync();
            var ts = await db.TeacherStudents.FirstAsync(x => x.TeacherId == tid && x.StudentId == sid);
            return PartialView("_EditTeacherStudent", ts);
        }
        [HttpPost]
        public async Task<IActionResult> EditTeacherStudent(TeacherStudent ts)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ts).State = EntityState.Modified;
                await db.SaveChangesAsync();
                var o = await GetTeacherStudent(ts.TeacherId, ts.StudentId);
                return Json(o);
            }
            return BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTeacherStudent([FromQuery] int tid, [FromQuery] int sid)
        {

            var o = new TeacherStudent { StudentId = sid, TeacherId = tid };
            db.Entry(o).State = EntityState.Deleted;

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Data deleted" });

        }
        private async Task<TeacherStudent> GetTeacherStudent(int tid, int sid)
        {
            var ts = await db.TeacherStudents
                .Include(o => o.Teacher)
                .Include(o => o.Student)
                .FirstAsync(x => x.TeacherId == tid && x.StudentId == sid);
            return ts;
        }
    }
}
