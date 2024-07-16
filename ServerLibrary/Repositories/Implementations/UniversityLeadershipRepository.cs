using BaseLibrary.Responses;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class UniversityLeadershipRepository(AppDbContext appDbContext) : ILeadership
    {
        public async Task<ResponseList<MemberLeadershipInfo>> GetUniveristyLeadership()
        {
            var query = from l in appDbContext.UniversityLeaders
                        join p in appDbContext.Professors on l.ProfessorId equals p.ProfessorId
                        join u in appDbContext.Users on p.UserId equals u.UserId
                        select new
                        {
                            Name = l.Name,
                            Email = u.UserEmail,
                            Title = p.AcademicTitle,
                            Function = l.Function,
                            Department = l.Department,
                            Image = l.Image
                        };
            var list = query.ToList();
            if (list != null)
            {
                List<MemberLeadershipInfo> result = new List<MemberLeadershipInfo>();
                foreach (var item in list)
                {
                    MemberLeadershipInfo member = new MemberLeadershipInfo()
                    {
                        Name = item.Name,
                        Email = item.Email,
                        Title = item.Title,
                        Function = item.Function,
                        Department = item.Department,
                        Image = Convert.ToBase64String(item.Image)
                    };
                    result.Add(member);
                }
                return new ResponseList<MemberLeadershipInfo>(true, result);
            }
            else return new ResponseList<MemberLeadershipInfo>(false, null);

        }
    }
}
