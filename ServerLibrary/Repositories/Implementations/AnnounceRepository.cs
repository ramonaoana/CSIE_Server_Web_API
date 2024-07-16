using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class AnnounceRepository(AppDbContext appDbContext) : IAnnounce
    {
        public async Task<ResponseRequest<AnnouncementInfo>> GetAnnounce(string word)
        {
            var wordString = "%" + word.ToLower() + "%";
            var query = await (from announcement in appDbContext.Announcements
                               where EF.Functions.Like(announcement.Title.ToLower(), wordString)
                               select new
                               {
                                   Title = announcement.Title,
                                   Content = announcement.AnnouncementContent,
                                   PostedDate = announcement.PostedDate,
                                   Type = announcement.Type
                               }).FirstOrDefaultAsync();
            if (query != null)
            {
                AnnouncementInfo info = new AnnouncementInfo()
                {
                    Title = query.Title,
                    Content = query.Content,
                    PostedDate = query.PostedDate,
                    Type = query.Type,
                    DocumentId = null,
                    DocumentContent = null
                };
                return new ResponseRequest<AnnouncementInfo>(true, info);
            }
            return new ResponseRequest<AnnouncementInfo>(false, null);
        }
    }
}
