using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class UserProfileRepository(AppDbContext appDbContext) : IUserProfile
    {
        public async Task<ResponseRequest<string>> CheckUserRole(int userId)
        {
            var query = await (from user in appDbContext.Users
                               join userRole in appDbContext.UserSystemRole on user.UserId equals userRole.UserId
                               join role in appDbContext.SystemRoles on userRole.RoleId equals role.RoleId
                               where user.UserId == userId
                               select new
                               {
                                   RoleName = role.Name
                               }).FirstOrDefaultAsync();
            if (query != null)
            {
                return new ResponseRequest<string>(true, query.RoleName);
            }
            return new ResponseRequest<string>(false, null);
        }
        public async Task<UserResponse> GetProfile(int userId)
        {
            User user = await FindUser(userId);
            UserProfile userProfile = null;

            if (user.UserEmail.Contains("@stud"))
            {
                Student student = await FindStudent(userId);
                userProfile = await MapUserProfile(student);
            }
            else
            {
                Professor professor = await FindProfessor(userId);
                userProfile = await MapUserProfile(professor);
            }

            return new UserResponse(userProfile != null, userProfile);
        }

        private async Task<UserProfile> MapUserProfile(Student student)
        {
            if (student == null)
                return null;

            Address address = await FindAddress(student.AddressId);
            return new UserProfile()
            {
                Address = address,
                FirstName = student.FirstName,
                LastName = student.LastName,
                PersonalEmailAddress = student.PersonalEmailAddress,
                CNP = student.CNP,
                DateOfBirth = student.DateOfBirth,
                PlaceOfBirth = student.PlaceOfBirth,
                PhoneNumber = student.PhoneNumber
            };
        }

        private async Task<UserProfile> MapUserProfile(Professor professor)
        {
            if (professor == null)
                return null;

            Address addressProfessor = await FindAddress(professor.AddressId);
            return new UserProfile()
            {
                Address = addressProfessor,
                FirstName = professor.FirstName,
                LastName = professor.LastName,
                PersonalEmailAddress = professor.PersonalEmailAddress,
                CNP = professor.CNP,
                DateOfBirth = professor.DateOfBirth,
                PlaceOfBirth = professor.PlaceOfBirth,
                PhoneNumber = professor.PhoneNumber,
                Title = professor.AcademicTitle
            };
        }

        public async Task<User> FindUser(int userId) => await appDbContext.Users.FirstOrDefaultAsync(_ => _.UserId == userId);
        private async Task<Student> FindStudent(int userId) => await appDbContext.Students.FirstOrDefaultAsync(_ => _.UserId == userId);
        private async Task<Professor> FindProfessor(int userId) => await appDbContext.Professors.FirstOrDefaultAsync(_ => _.UserId == userId);
        private async Task<Address> FindAddress(int address) => await appDbContext.Addresses.FirstOrDefaultAsync(_ => _.AddressId == address);

        public async Task<ResponseRequest<UserNameAndEmail>> GetName(int userId)
        {
            var user = await FindUser(userId);
            var student = await FindStudent(userId);
            UserNameAndEmail userDetails = new UserNameAndEmail();
            if (user != null && student != null)
            {
                userDetails.Email = user.UserEmail;
                userDetails.Name = student.LastName + " " + student.FirstName;
                userDetails.Password = user.Password;
                return new ResponseRequest<UserNameAndEmail>(true, userDetails);
            }
            else return new ResponseRequest<UserNameAndEmail>(false, null);

        }

        public async Task<ResponseList<CourseProfessor>> GetCoursesForProfessor(int userId)
        {
            int currentYear = DateTime.Now.Year;
            var query = await (from user in appDbContext.Users
                               join professor in appDbContext.Professors on user.UserId equals professor.UserId
                               join schedule in appDbContext.Schedules on professor.ProfessorId equals schedule.ProfessorId
                               join course in appDbContext.Courses on schedule.CourseId equals course.CourseID
                               join semester in appDbContext.Semesters on schedule.SemesterId equals semester.SemesterId
                               join academicYear in appDbContext.AcademicYears on semester.AcademicYearId equals academicYear.AcademicYearId
                               where user.UserId == userId && (academicYear.AcademicStartYear == currentYear || academicYear.AcademicEndYear == currentYear)
                               select new
                               {
                                   Course = course.CourseName,
                                   CourseType = schedule.Type
                               }).ToListAsync();
            List<CourseProfessor> courses = new List<CourseProfessor>();
            if (query != null)
            {
                foreach (var item in query)
                {
                    CourseProfessor course = new CourseProfessor()
                    {
                        Course = item.Course,
                        Type = item.CourseType
                    };
                    courses.Add(course);
                }
                return new ResponseList<CourseProfessor>(true, courses);
            }
            else return new ResponseList<CourseProfessor>(false, null);
        }
    }
}
