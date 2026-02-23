using CertStore.Domain.Dtos;

namespace CertStore.API.Services.Interfaces;

public interface IUserCertService
{
    Task<List<UserCertDto>> GetAllUserCertsAsync();
    Task<List<UserCertDto>> GetUserCertsByRootIdAsync(Guid rootCertId);
    Task<UserCertDto?> GetUserCertByIdAsync(Guid id);
    Task<UserCertDto> SignUserCertAsync(SignUserCertDto dto);
    Task<bool> DeleteUserCertAsync(Guid id);
}
