using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class EnrollmentStudentRepository(AppDbContext appDbContext) : IEnrollmentStudent
    {
        public async Task<ResponseList<EnrollmentStudentProfile>> GetEnrollmentDetails(int userId)
        {
            var query = from en in appDbContext.EnrollmentStudentsGroups
                        from u in appDbContext.Users
                        join stud in appDbContext.Students on u.UserId equals stud.UserId
                        join fin in appDbContext.FinanceFormS on en.FinanceFormId equals fin.FinanceFormId
                        join lang in appDbContext.Languages on en.LanguageId equals lang.LanguageId
                        join g in appDbContext.Groups on en.GroupId equals g.GroupId
                        join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                        join y in appDbContext.AcademicYears on s.AcademicYearId equals y.AcademicYearId
                        join sem in appDbContext.Semesters on y.AcademicYearId equals sem.AcademicYearId
                        join ed in appDbContext.EducationForms on y.EducationFormId equals ed.EducationFormId
                        join spec in appDbContext.Specializations on y.SpecializationId equals spec.SpecializationId
                        join cycle in appDbContext.StudyCycles on spec.StudyCycleId equals cycle.StudyCycleId
                        where u.UserId == userId
                        select new
                        {
                            StartYearP = y.AcademicStartYear,
                            EndYearP = y.AcademicEndYear,
                            Status = en.Status,
                            AcademicYear = y.Year,
                            Sem = sem.SemesterNumber,
                            Cycle = cycle.Name,
                            Group = g.GroupNumber,
                            Serie = s.SeriesName,
                            Finance = fin.FormFinance,
                            Lang = lang.TeachingLanguage,
                            Spec = spec.SpecializationName,
                            FormEducation = ed.EducationFormName

                        };

            var list = query.ToList();
            List<EnrollmentStudentProfile> result = new List<EnrollmentStudentProfile>();
            foreach (var item in list)
            {
                EnrollmentStudentProfile student = new EnrollmentStudentProfile()
                {
                    Semester = item.Sem,
                    StudyCycle = item.Cycle,
                    FinanceForm = item.Finance,
                    Language = item.Lang,
                    Specialization = item.Spec,
                    EducationForm = item.FormEducation,
                    AcademicYear = item.AcademicYear,
                    StartYear = item.StartYearP,
                    EndYear = item.EndYearP,
                    Group = item.Group,
                    Serie = item.Serie,
                    Status = item.Status
                };
                result.Add(student);
            }
            return new ResponseList<EnrollmentStudentProfile>(true, result);
        }
    }
}
