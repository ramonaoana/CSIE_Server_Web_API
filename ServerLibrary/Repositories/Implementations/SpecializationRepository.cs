using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class SpecializationRepository(AppDbContext appDbContext) : ISpecialization
    {
        public async Task<ResponseList<Specialization>> GetSpecializations()
        {
            var allRecords = await appDbContext.Specializations.ToListAsync();
            if (allRecords != null && allRecords.Any())
            {
                return new ResponseList<Specialization>(true, allRecords);
            }
            else
            {
                return new ResponseList<Specialization>(false, null);
            }
        }
    }
}
