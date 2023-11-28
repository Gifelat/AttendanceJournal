using AttendanceJournalLibrary;

namespace ASP_Server
{
    public class AddPostsDB
    {
        private readonly RequestDelegate _next;
        private Random _random;
        private AttendanceJournalContext _db;
        private int _smallTable = 100;
        private int _largeTable = 10000;

        public AddPostsDB(RequestDelegate next, AttendanceJournalContext db)
        {
            _next = next;
            _random = new Random(Guid.NewGuid().GetHashCode());
            _db = db;
        }

        public async Task Invoke(HttpContext context)
        {
            AddRoles();
            AddSubjects();
            AddFaculties();
            AddTeachers();
            AddClasses();
            AddStudents();

            await _next.Invoke(context);
        }

        private void AddRoles()
        {
            if (!_db.Roles.Any())
            {
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0)
                        _db.Roles.Add(new Role() { Name = "Admin" });
                    if (i == 1)
                        _db.Roles.Add(new Role() { Name = "User" });
                    _db.SaveChanges();
                }
            }
        }

        private void AddSubjects()
        {
            if (!_db.Subjects.Any())
            {
                for (int i = 0; i < _smallTable; i++)
                {
                    _db.Subjects.Add(new Subject() { SubjectName = $"Subject{i + 1}" });
                    _db.SaveChanges();
                }
            }
        }

        private void AddFaculties()
        {
            if (!_db.Faculties.Any())
            {
                for (int i = 0; i < _smallTable; i++)
                {
                    _db.Faculties.Add(new Faculty() { FacultyName = $"FacultyName{i + 1}" });
					_db.Faculties.Add(new Faculty() { DepartmentName = $"DepartmentName{i + 1}" });
					_db.Faculties.Add(new Faculty() { GroupName = $"GroupName{i + 1}" });
					_db.SaveChanges();
                }
            }
        }

        private void AddTeachers()
        {
            if (!_db.Teachers.Any())
            {
                for (int i = 0; i < _largeTable; i++)
                {
                    _db.Teachers.Add(new Teacher()
                    {
                        Login = (i == 0) ? "Admin" : $"Login{i + 1}",
                        Password = _random.Next(0, 9) + "" + _random.Next(0, 9) + "" + _random.Next(0, 9) + "" + _random.Next(0, 10000),
                        Email = $"Email{i + 1}@gmail.com",
                        FirstName = $"Name{i + 1}",
                        LastName = $"Surname{i + 1}",
						MiddleName = $"Surname{i + 1}",
						Position = $"Position{i + 1}",
						RoleID = (i == 0) ? 1 : 2,
						FacultyID = _random.Next(_smallTable) + 1
					});
                    _db.SaveChanges();
                }
            }
        }

        private void AddClasses()
        {
            if (!_db.Classes.Any())
            {
                for (int i = 0; i < _largeTable; i++)
                {
                    _db.Classes.Add(new Class()
                    {
                        Theme = $"Theme{i + 1}",
                        SubjectID = _random.Next(_smallTable) + 1,
						FacultyID = _random.Next(_smallTable) + 1,
       					ClassDate = GetRandomDate().Date,
						StartTime = new TimeSpan(_random.Next(0, 12), _random.Next(0, 59), _random.Next(0, 59)),
						EndTime = new TimeSpan(_random.Next(0, 12), _random.Next(0, 59), _random.Next(0, 59))
					});
                    _db.SaveChanges();
                }
            }
        }

		private void AddStudents()
		{
			if (!_db.Students.Any())
			{
				for (int i = 0; i < _largeTable; i++)
				{
					_db.Students.Add(new Student()
					{
						FirstName = $"FirstName{i + 1}",
                        LastName = $"LastName{i + 1}",
						FacultyID = _random.Next(_smallTable) + 1
					});
					_db.SaveChanges();
				}
			}
		}

		private void AddAttendances()
        {
            if (!_db.Attendances.Any())
            {
                for (int i = 0; i < _largeTable; i++)
                {
                    _db.Attendances.Add(new Attendance()
                    {
                        ClassID = _random.Next(_largeTable) + 1,
						StudentID = _random.Next(_largeTable) + 1,
						TeacherID = _random.Next(_largeTable) + 1,
                        /*IsPresent = (i == 0) ? 1 : 2*/
					});
                    _db.SaveChanges();
                }
            }
        }

        private DateTime GetRandomDate()
        {
            DateTime start = new DateTime(1995, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(_random.Next(range));
        }
    }
}
