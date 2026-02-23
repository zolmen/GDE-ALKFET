using CertStore.Infrastructure.Repository;

namespace CertStore.Infrastructure.UnitOfWork;

public interface IUnitOfWork
{
    IRootCertRepository RootCerts { get; }
    IUserCertRepository UserCerts { get; }
}
