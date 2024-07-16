using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class StudentCertificateRepository(AppDbContext appDbContext) : IStudentCertificate
    {

        public async Task<GeneralResponse> CheckStatusUser(int userId)
        {
            var query = await FindStudent(userId);
            var result = await CheckStatus(query.StudentId);
            if (result.Flag)
            {
                return new GeneralResponse(true, "Student activ");
            }
            return new GeneralResponse(false, "Student inactiv");
        }

        public async Task<ResponseRequest<bool>> IsStudent(int userId)
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
                if (query.RoleName == "Student")
                {
                    return new ResponseRequest<bool>(true, true);
                }
                else return new ResponseRequest<bool>(true, false);
            }
            return new ResponseRequest<bool>(false, false);
        }
        public async Task<GeneralResponse> CheckStatus(int studentId)
        {
            var query = from en in appDbContext.EnrollmentStudentsGroups
                        join g in appDbContext.Groups on en.GroupId equals g.GroupId
                        join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                        join y in appDbContext.AcademicYears on s.AcademicYearId equals y.AcademicYearId
                        join p in appDbContext.Promotions on y.PromotionId equals p.Id
                        where en.StudentId == studentId && en.Status == 2
                        select new
                        {
                            StartYearP = y.AcademicStartYear,
                            EndYearP = y.AcademicEndYear,
                            Status = en.Status
                        };

            var result = await query.FirstOrDefaultAsync();
            bool checkStudent = false;
            string message = "";
            if (result != null)
            {
                int startYear = result.StartYearP;
                int endYear = result.EndYearP;
                int currentYear = DateTime.Now.Year;
                bool isActive = (currentYear >= startYear && currentYear <= endYear);
                if (isActive)
                {
                    checkStudent = true;
                    message = "The student is active in the current academic year";
                }
                else message = "The student is not active in the current academic year";
                return new GeneralResponse(checkStudent, message);
            }
            return new GeneralResponse(checkStudent, message);


        }

        public async Task<DocumentResponse> GenerateDocument(int user, string reason)
        {
            StudentInfoResponse en = await GetStudentInfo(user);
            if (en.Flag)
            {
                InfoStudent userDetails = en.info;
                var requestDocumentId = await GetDocumentId();
                int id = Convert.ToInt32(requestDocumentId.Message);
                byte[] pdfBytes = CreatePdfDocument(userDetails, reason, id);
                try
                {
                    var result = await SaveDocument(en.info.Id, pdfBytes);
                }
                catch (Exception ex)
                {
                    return new DocumentResponse(false, null);
                }
                return new DocumentResponse(true, pdfBytes);

            }
            return new DocumentResponse(false, null);
        }

        static byte[] CreatePdfDocument(InfoStudent userDetails, string reason, int documentId)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (PdfWriter writer = new PdfWriter(memoryStream))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    pdf.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4);
                    iText.Layout.Document document = new iText.Layout.Document(pdf);
                    document.SetMargins(60, 60, 60, 60);
                    document.SetBorder(new SolidBorder(2f));


                    Paragraph header = new Paragraph()
                        .Add(new Text("Ministerul Educatiei").SetFontSize(7)).SetBold()
                        .Add(new Text("\nAcademia de Studii Economice din Bucuresti").SetFontSize(7)).SetBold();

                    document.Add(header);
                    int id = 1000 + documentId;
                    Paragraph rightHeader = new Paragraph()
                    .Add(new Text("Nr. A" + id + "/ " + DateTime.Now.Date.ToString("dd.MM.yyyy")).SetFontSize(7))
                        .SetTextAlignment(TextAlignment.RIGHT);
                    document.Add(rightHeader);

                    Paragraph paragraph = new Paragraph()
                      .Add(new Text("\nFacultatea:CIBERNETICA, STATISTICA SI INFORMATICA ECONOMICA").SetFontSize(7))
                      .Add(new Text("\nSpecializarea: " + userDetails.EnrollmentDetails.Specialization.ToUpper()).SetFontSize(7))
                      .Add(new Text("\nLimba Predata: " + userDetails.EnrollmentDetails.Language.ToUpper() + ";" + " Locatie Geografica: Bucuresti").SetFontSize(7)).SetBold();
                    document.Add(paragraph);



                    Paragraph title = new Paragraph("ADEVERINTA").SetFontSize(15).SetTextAlignment(TextAlignment.CENTER).SetBold();
                    document.Add(title);
                    string typeOfFinance = (userDetails.EnrollmentDetails.Language.ToUpper() == "ROMANA") ? "RO" : "EN";
                    Paragraph paragraph2 = new Paragraph()
                      .Add(new Text("\nStudentul/a: " + userDetails.LastName.ToUpper() + " " + userDetails.FirstName.ToUpper()).SetFontSize(10))
                      .Add(new Text("\n este inscris(a) in anul universitar " + userDetails.EnrollmentDetails.StartYear + "-" + userDetails.EnrollmentDetails.EndYear
                      + ", in anul " + userDetails.EnrollmentDetails.AcademicYear + " de studii universitare de " + userDetails.EnrollmentDetails.StudyCycle + ", " +
                      "la forma de invatamant cu " + userDetails.EnrollmentDetails.EducationForm + " la forma de finantare " + userDetails.EnrollmentDetails.FinanceForm.ToUpper() + " " + typeOfFinance + ".").SetFontSize(10));
                    document.Add(paragraph2);

                    Paragraph paragraph3 = new Paragraph()
                      .Add(new Text("\nAdeverinta se elibereaza pentru a-i servi la:").SetFontSize(10)).SetTextAlignment(TextAlignment.CENTER)
                      .Add(new Text("\n" + reason.ToUpper()).SetFontSize(10)).SetTextAlignment(TextAlignment.CENTER).SetItalic();
                    document.Add(paragraph3);

                    Table table = new Table(2);
                    table.SetBorder(Border.NO_BORDER);
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    Cell leftCell = new Cell().SetTextAlignment(TextAlignment.LEFT);
                    leftCell.SetBorder(Border.NO_BORDER);
                    Paragraph signatureProfessor = new Paragraph()
                        .Add(new Text("Decan,").SetFontSize(10).SetBold())
                        .Add(new Text("\nProfesor univ. dr. MARIAN DARDALA").SetFontSize(10).SetBold());
                    Image signatureProfessorImage = new Image(ImageDataFactory.Create(@"C:\\Users\\aramo\\Desktop\\CHATBOT_SECRETARIAT\\CSIE\\BaseLibrary\\Images\\stamp.png"));
                    signatureProfessorImage.SetWidth(70);
                    signatureProfessorImage.SetHeight(70);
                    signatureProfessorImage.SetMarginLeft(50);

                    leftCell.Add(signatureProfessor);
                    leftCell.Add(signatureProfessorImage);


                    Cell rightCell = new Cell().SetTextAlignment(TextAlignment.RIGHT);
                    rightCell.SetBorder(Border.NO_BORDER);
                    Paragraph signatureSecretariat = new Paragraph()
                        .Add(new Text("Secretar sef facultate").SetFontSize(10).SetBold())
                        .Add(new Text("\nClaudia COSTACHE").SetFontSize(10).SetBold());
                    Image signatureImage = new Image(ImageDataFactory.Create(@"C:\\Users\\aramo\\Desktop\\CHATBOT_SECRETARIAT\\CSIE\\BaseLibrary\\Images\\signature.png"));
                    signatureImage.SetWidth(50);
                    signatureImage.SetHeight(30);
                    signatureImage.SetMarginLeft(120);

                    rightCell.Add(signatureSecretariat);
                    rightCell.Add(signatureImage);
                    table.AddCell(leftCell);
                    table.AddCell(rightCell);


                    document.Add(table);

                    document.Close();
                }
            }
            return memoryStream.ToArray();
        }
        public async Task<StudentInfoResponse> GetStudentInfo(int userId)
        {
            Student stud = await FindStudent(userId);

            if (stud != null)
            {
                InfoStudent infoStudent = new InfoStudent
                {
                    Id = stud.StudentId,
                    FirstName = stud.FirstName,
                    LastName = stud.LastName
                };
                EnrollmentStudentGroup enrollmentDetails = await FindEnrollmentDetails(stud.StudentId);
                if (enrollmentDetails != null)
                {
                    GeneralResponse res = await CheckStatus(stud.StudentId);
                    if (res.Flag)
                    {
                        EnrollmentInfoResponse enrollment = await GetEnrollementDetails(enrollmentDetails.GroupId);
                        if (enrollment != null)
                        {
                            infoStudent.EnrollmentDetails = enrollment.info;
                        }
                        else return new StudentInfoResponse(false, null);
                    }
                    else return new StudentInfoResponse(false, null);
                }
                return new StudentInfoResponse(true, infoStudent);
            }
            return new StudentInfoResponse(false, null);
        }

        private async Task<Student> FindStudent(int userId) => await appDbContext.Students.FirstOrDefaultAsync(_ => _.UserId == userId);
        private async Task<EnrollmentStudentGroup> FindEnrollmentDetails(int studentId) =>
            await appDbContext.EnrollmentStudentsGroups.FirstOrDefaultAsync(_ => _.StudentId == studentId
            && _.Status == 2);

        public async Task<EnrollmentInfoResponse> GetEnrollementDetails(int groupId)
        {
            var query = from g in appDbContext.Groups
                        join en in appDbContext.EnrollmentStudentsGroups on g.GroupId equals en.GroupId
                        join f in appDbContext.FinanceFormS on en.FinanceFormId equals f.FinanceFormId
                        join l in appDbContext.Languages on en.LanguageId equals l.LanguageId
                        join s in appDbContext.Series on g.SeriesId equals s.SeriesId
                        join y in appDbContext.AcademicYears on s.AcademicYearId equals y.AcademicYearId
                        join sem in appDbContext.Semesters on y.AcademicYearId equals sem.AcademicYearId
                        join p in appDbContext.Promotions on y.PromotionId equals p.Id
                        join spec in appDbContext.Specializations on y.SpecializationId equals spec.SpecializationId
                        join cycle in appDbContext.StudyCycles on spec.StudyCycleId equals cycle.StudyCycleId
                        join edForm in appDbContext.EducationForms on y.EducationFormId equals edForm.EducationFormId
                        where g.GroupId == groupId
                        select new
                        {
                            AcademicY = y.Year,
                            StartYearP = y.AcademicStartYear,
                            EndYearP = y.AcademicEndYear,
                            FormEd = edForm.EducationFormName,
                            Spec = spec.SpecializationName,
                            Cycle = cycle.Name,
                            Finance = f.FormFinance,
                            Language = l.TeachingLanguage

                        };

            var result = await query.FirstOrDefaultAsync();
            EnrollmentDetails enrollement = new EnrollmentDetails();
            enrollement.AcademicYear = result.AcademicY;
            enrollement.StartYear = result.StartYearP;
            enrollement.EndYear = result.EndYearP;
            enrollement.EducationForm = result.FormEd;
            enrollement.Specialization = result.Spec;
            enrollement.StudyCycle = result.Cycle;
            enrollement.FinanceForm = result.Finance;
            enrollement.Language = result.Language;
            return new EnrollmentInfoResponse(true, enrollement);
        }

        public async Task<GeneralResponse> GetDocumentId()
        {
            var lastRecord = await appDbContext.StudentDocuments.OrderByDescending(x => x.StudentDocumentId).FirstOrDefaultAsync();
            int id = lastRecord != null ? lastRecord.StudentDocumentId + 1 : 1;
            return new GeneralResponse(true, id.ToString());
        }

        public async Task<GeneralResponse> SaveDocument(int id, byte[] content)
        {
            StudentDocument doc = new StudentDocument();
            doc.StudentId = id;
            doc.Content = content;
            doc.Date = DateTime.Now;
            doc.Name = "ADV/" + id + "/" + DateTime.Now.Date.ToString("dd/MM/yyyy");
            appDbContext.StudentDocuments.Add(doc);
            await appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, "the document was saved successfully");

        }
    }
}
