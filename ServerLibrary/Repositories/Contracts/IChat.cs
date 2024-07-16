using BaseLibrary.DTOs;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IChat
    {
        Task<GeneralResponse> TrainModelML();
        Task<ResponseRequest<ChatResponse>> ClassifyMessage(ChatMessage chatMessage);
        Task<ResponseRequest<string>> TestClassifyMessage(ChatMessage chatMessage);
        Task<Dictionary<string, string>> GetKeywordsNER(ChatMessage chatMessage);
        Task<ResponseRequest<string>> QueryPdf(ChatMessage chatMessage, int typeDocument);
        Task<ChatResponse> getStudentCertificate(ChatMessage message);
        Task<ChatResponse> getEmailProfessor(ChatMessage message);
        Task<ChatResponse> getInfoScholarship(ChatMessage message);
        Task<ChatResponse> getAmountScholarship(ChatMessage message);
        Task<ChatResponse> getInfoCollegeAdmission(ChatMessage message);
        Task<ChatResponse> getInfoMaster(ChatMessage message);
        Task<ChatResponse> getInfoStudentCalendar(ChatMessage message);
        Task<ChatResponse> getInfoBachelorDegree(ChatMessage message);
        Task<ChatResponse> getInfoMasterDegree(ChatMessage message);
        Task<ChatResponse> getInfoBachelorDegreeInternship(ChatMessage message);
        Task<ChatResponse> getInfoMasterInternship(ChatMessage message);
        Task<ChatResponse> getInfoPlagiarism(ChatMessage message);
        Task<ChatResponse> getSchedule(ChatMessage message);
        Task<ChatResponse> getInfoCourse(ChatMessage message);
        Task<ChatResponse> getInfoExam(ChatMessage message);
        Task<ChatResponse> getExamGrade(ChatMessage message);
        Task<ChatResponse> getScheduleProfessor(ChatMessage message);
        Task<ChatResponse> getScheduleDay(ChatMessage message);
        Task<ChatResponse> getScheduleDayAndHour(ChatMessage message);
        Task<ChatResponse> getCoursesYear(ChatMessage message);
        Task<ChatResponse> getInfoScheduleCourse(ChatMessage message);
        Task<ChatResponse> getJobs(ChatMessage message);
        Task<ChatResponse> getGroupsAndSeries(ChatMessage message);
        Task<ChatResponse> getGroupsForSeries(ChatMessage message);
        Task<ChatResponse> getGroupsBySpecializationAndYear(ChatMessage message);
        Task<ChatResponse> getSeriesForSpecialization(ChatMessage message);
        Task<ChatResponse> getGroupsBySpecCycleAndYear(ChatMessage message);
        Task<ChatResponse> getSeriesForGroup(ChatMessage message);
        Task<ResponseRequest<string>> FindAnnounce(string message);

    }
}
