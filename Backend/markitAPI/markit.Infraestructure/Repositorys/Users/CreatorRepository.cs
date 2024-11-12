using markit.Application.Contracts.Persistence.Users;
using markit.Domain.Entities;
using markit.Infraestructure.Persistence.EF;

namespace markit.Infraestructure.Repositorys.Users
{
    public class CreatorRepository : BaseRepository<Creator>, ICreatorRepository
    {
        public CreatorRepository(MarkitDbContext mirefDbContext) : base(mirefDbContext)
        {
        }
    }
}
