using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories
{
    public class UserReportRepository : BaseRepository<UserReport>, IUserReportRepository
    {
        public UserReportRepository(ApplicationDbContext context) : base(context) { }
    }
}
