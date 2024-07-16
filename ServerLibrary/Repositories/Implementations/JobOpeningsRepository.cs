using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class JobOpeningsRepository(AppDbContext appDbContext) : IJobOpenings
    {
        public async Task<ResponseList<AnnouncementInfo>> GetJobOpenings()
        {
            var query = await (from announcement in appDbContext.Announcements
                               join doc in appDbContext.Documents on announcement.DocumentId equals doc.DocumentId
                               where announcement.Type == "Job"
                               select new
                               {
                                   Title = announcement.Title,
                                   Content = announcement.AnnouncementContent,
                                   PostedDate = announcement.PostedDate,
                                   Type = announcement.Type,
                                   DocumentId = announcement.DocumentId,
                                   DocumentContent = doc.Content
                               }).ToListAsync();
            List<AnnouncementInfo> result = new List<AnnouncementInfo>();
            if (query != null)
            {
                foreach (var item in query)
                {
                    AnnouncementInfo info = new AnnouncementInfo()
                    {
                        Title = item.Title,
                        Content = item.Content,
                        PostedDate = item.PostedDate,
                        Type = item.Type,
                        DocumentId = item.DocumentId,
                        DocumentContent = item.DocumentContent
                    };
                    result.Add(info);
                }
                return new ResponseList<AnnouncementInfo>(true, result);
            }
            return new ResponseList<AnnouncementInfo>(false, null);
        }
    }
}
