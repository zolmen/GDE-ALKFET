using CertStore.Infrastructure.Entities;
using MongoDB.Driver;

namespace CertStore.Infrastructure.Repository;

public class RootCertRepository : Repository<RootCertEntity>, IRootCertRepository
{
    public RootCertRepository(IMongoDatabase database)
        : base(database, "RootCertificates") { }
}
