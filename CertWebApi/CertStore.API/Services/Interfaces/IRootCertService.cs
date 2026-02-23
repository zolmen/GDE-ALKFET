using CertStore.Domain.Dtos;

namespace CertStore.API.Services.Interfaces;

public interface IRootCertService
{
    Task<List<RootCertDto>> GetAllRootCertsAsync();
    Task<RootCertDto?> GetRootCertByIdAsync(Guid id);
    Task<RootCertDto> CreateRootCertAsync(CreateRootCertDto dto);
    Task<bool> DeleteRootCertAsync(Guid id);
}
