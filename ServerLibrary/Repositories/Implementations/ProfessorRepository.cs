using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class ProfessorRepository(AppDbContext appDbContext) : IProfessors
    {
        public async Task<ResponseRequest<string>> GetEmailAddress(string name)
        {
            var professor = await appDbContext.Professors.FirstOrDefaultAsync(p =>
        (p.FirstName + " " + p.LastName).Contains(name) ||
        (p.LastName + " " + p.FirstName).Contains(name) ||
        p.FirstName.Contains(name) ||
        p.LastName.Contains(name)
    );
            if (professor != null)
            {
                var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.UserId == professor.UserId);
                if (user != null)
                {
                    return new ResponseRequest<string>(true, user.UserEmail);
                }
            }
            return new ResponseRequest<string>(false, null);
        }

        public async Task<ResponseList<Professor>> GetProfessors()
        {
            var allRecords = await appDbContext.Professors.ToListAsync();
            if (allRecords != null && allRecords.Any())
            {
                return new ResponseList<Professor>(true, allRecords);
            }
            else
            {
                return new ResponseList<Professor>(false, null);
            }
        }
    }
}
