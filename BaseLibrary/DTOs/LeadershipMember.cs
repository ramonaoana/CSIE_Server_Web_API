using BaseLibrary.Entities;

namespace BaseLibrary.DTOs
{
    public class LeadershipMember
    {
        public Professor? Professor { get; set; }
        public UniversityLeader Leader { get; set; }
    }
}
