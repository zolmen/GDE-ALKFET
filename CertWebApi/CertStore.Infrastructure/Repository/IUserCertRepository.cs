using CertStore.Infrastructure.Entities;

namespace CertStore.Infrastructure.Repository;

public interface IUserCertRepository : IRepository<UserCertEntity>
{
    Task<List<UserCertEntity>> GetByRootCertIdAsync(Guid rootCertId);
}
