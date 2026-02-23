using CertStore.Infrastructure.Entities;
using MongoDB.Driver;

namespace CertStore.Infrastructure.Repository;

public class UserCertRepository : Repository<UserCertEntity>, IUserCertRepository
{
    public UserCertRepository(IMongoDatabase database)
        : base(database, "UserCertificates") { }

    public async Task<List<UserCertEntity>> GetByRootCertIdAsync(Guid rootCertId)
    {
        return await FindAsync(x => x.RootCertId == rootCertId);
    }
}
