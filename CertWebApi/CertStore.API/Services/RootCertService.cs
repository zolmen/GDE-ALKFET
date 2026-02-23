using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertStore.API.Services.Interfaces;
using CertStore.Domain.Dtos;
using CertStore.Infrastructure.Entities;
using CertStore.Infrastructure.UnitOfWork;

namespace CertStore.API.Services;

public class RootCertService : IRootCertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RootCertService> _logger;

    public RootCertService(IUnitOfWork unitOfWork, ILogger<RootCertService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<RootCertDto>> GetAllRootCertsAsync()
    {
        var rootCerts = await _unitOfWork.RootCerts.GetAllAsync();
        var result = new List<RootCertDto>();

        foreach (var root in rootCerts)
        {
            var userCerts = await _unitOfWork.UserCerts.GetByRootCertIdAsync(root.Id);
            result.Add(MapToDto(root, userCerts.Count));
        }

        return result;
    }

    public async Task<RootCertDto?> GetRootCertByIdAsync(Guid id)
    {
        var root = await _unitOfWork.RootCerts.GetByIdAsync(id);
        if (root == null) return null;

        var userCerts = await _unitOfWork.UserCerts.GetByRootCertIdAsync(root.Id);
        return MapToDto(root, userCerts.Count);
    }

    public async Task<RootCertDto> CreateRootCertAsync(CreateRootCertDto dto)
    {
        using var rsa = RSA.Create(2048);

        var request = new CertificateRequest(
            $"CN={dto.SubjectName}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));

        var notBefore = DateTimeOffset.UtcNow;
        var notAfter = notBefore.AddDays(dto.ValidityDays);

        var cert = request.CreateSelfSigned(notBefore, notAfter);

        var entity = new RootCertEntity
        {
            SubjectName = dto.SubjectName,
            CertificateDataBase64 = Convert.ToBase64String(cert.Export(X509ContentType.Pfx)),
            PrivateKeyDataBase64 = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
            NotBefore = cert.NotBefore,
            NotAfter = cert.NotAfter,
            Thumbprint = cert.Thumbprint
        };

        await _unitOfWork.RootCerts.AddAsync(entity);

        _logger.LogInformation("Root tanúsítvány létrehozva: Subject={Subject}, Id={Id}",
            dto.SubjectName, entity.Id);

        return MapToDto(entity, 0);
    }

    public async Task<bool> DeleteRootCertAsync(Guid id)
    {
        var deletedUserCerts = await _unitOfWork.UserCerts
            .DeleteManyAsync(x => x.RootCertId == id);

        var deleted = await _unitOfWork.RootCerts.DeleteAsync(id);

        if (deleted)
        {
            _logger.LogInformation(
                "Root tanúsítvány törölve: Id={Id}, törölt user cert-ek: {Count}",
                id, deletedUserCerts);
        }

        return deleted;
    }

    private static RootCertDto MapToDto(RootCertEntity entity, int userCertCount)
    {
        return new RootCertDto
        {
            Id = entity.Id,
            SubjectName = entity.SubjectName,
            NotBefore = entity.NotBefore,
            NotAfter = entity.NotAfter,
            Thumbprint = entity.Thumbprint,
            CreatedAt = entity.CreatedAt,
            UserCertCount = userCertCount
        };
    }
}
