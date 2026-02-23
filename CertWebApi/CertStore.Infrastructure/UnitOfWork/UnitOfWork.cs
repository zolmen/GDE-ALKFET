using CertStore.Infrastructure.Repository;
using MongoDB.Driver;

namespace CertStore.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    public IRootCertRepository RootCerts { get; }
    public IUserCertRepository UserCerts { get; }

    public UnitOfWork(IMongoDatabase database)
    {
        RootCerts = new RootCertRepository(database);
        UserCerts = new UserCertRepository(database);
    }
}
