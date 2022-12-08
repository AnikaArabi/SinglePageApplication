using SinglePageApplication.Models;

namespace SinglePageApplication.HostedService
{
    public class DbSeederHostedService : IHostedService
    {

        IServiceProvider serviceProvider;
        public DbSeederHostedService(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {

                var db = scope.ServiceProvider.GetRequiredService<TeacherDbContext>();

                await SeedDbAsync(db);

            }
        }
        public async Task SeedDbAsync(TeacherDbContext db)
        {
            await db.Database.EnsureCreatedAsync();
            if (!db.Students.Any())
            {
                
                var sb1 = new Subject { SubjectName = "CSharp", Title="Besic to Medium", TotalHour=980 };
                await db.Subjects.AddAsync(sb1);
                var s1 = new Student { StudentName = "Anika Arabi", AnnualCost = 20000.00M, Picture = "1.jpg", Continuing =true };
                await db.Students.AddAsync(s1);
                var t1 = new Teacher { TeacherName = "Habibul Haq", DateOfBirth= DateTime.Today.AddDays(-365 * 45), TeacherType= TeacherType.Senior, Subject= sb1 };
                t1.TeacherStudents.Add(new TeacherStudent { Teacher = t1, Student = s1, Fee = 200 });
                await db.Teachers.AddAsync(t1);
                await db.SaveChangesAsync();
            }

        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }
}
