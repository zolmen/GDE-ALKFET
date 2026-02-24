using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertStore.API.Services.Interfaces;
using CertStore.Domain.Dtos;
using CertStore.Infrastructure.Entities;
using CertStore.Infrastructure.UnitOfWork;

namespace CertStore.API.Services;

public class UserCertService : IUserCertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserCertService> _logger;

    public UserCertService(IUnitOfWork unitOfWork, ILogger<UserCertService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<UserCertDto>> GetAllUserCertsAsync()
    {
        var userCerts = await _unitOfWork.UserCerts.GetAllAsync();
        return userCerts.Select(MapToDto).ToList();
    }

    public async Task<List<UserCertDto>> GetUserCertsByRootIdAsync(Guid rootCertId)
    {
        var userCerts = await _unitOfWork.UserCerts.GetByRootCertIdAsync(rootCertId);
        return userCerts.Select(MapToDto).ToList();
    }

    public async Task<UserCertDto?> GetUserCertByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.UserCerts.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<UserCertDto> SignUserCertAsync(SignUserCertDto dto)
    {
        var rootCert = await _unitOfWork.RootCerts.GetByIdAsync(dto.RootCertId)
            ?? throw new InvalidOperationException("A megadott root tanúsítvány nem található.");

        var rootCertBytes = Convert.FromBase64String(rootCert.CertificateDataBase64);
        using var caCert = X509CertificateLoader.LoadCertificate(rootCertBytes);

        var privateKeyBytes = Convert.FromBase64String(rootCert.PrivateKeyDataBase64);
        using var caRsa = RSA.Create();
        caRsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        using var caWithKey = caCert.CopyWithPrivateKey(caRsa);

        var csrBytes = Convert.FromBase64String(dto.CsrBase64);
        var csr = CertificateRequest.LoadSigningRequestPem(
            System.Text.Encoding.UTF8.GetString(csrBytes),
            HashAlgorithmName.SHA256,
            CertificateRequestLoadOptions.UnsafeLoadCertificateExtensions);

        var serialNumber = new byte[16];
        Random.Shared.NextBytes(serialNumber);

        var notBefore = DateTimeOffset.UtcNow;
        var notAfter = notBefore.AddDays(dto.ValidityDays);

        var generator = X509SignatureGenerator.CreateForRSA(caRsa, RSASignaturePadding.Pkcs1);

        var signedCert = csr.Create(
            caCert.SubjectName,
            generator,
            notBefore,
            notAfter,
            serialNumber);

        var entity = new UserCertEntity
        {
            RootCertId = dto.RootCertId,
            SubjectName = signedCert.Subject,
            CertificateDataBase64 = Convert.ToBase64String(signedCert.Export(X509ContentType.Cert)),
            NotBefore = signedCert.NotBefore,
            NotAfter = signedCert.NotAfter,
            Thumbprint = signedCert.Thumbprint
        };

        await _unitOfWork.UserCerts.AddAsync(entity);

        _logger.LogInformation("User tanúsítvány aláírva: Subject={Subject}, RootCertId={RootCertId}",
            signedCert.Subject, dto.RootCertId);

        return MapToDto(entity);
    }

    public async Task<bool> DeleteUserCertAsync(Guid id)
    {
        var deleted = await _unitOfWork.UserCerts.DeleteAsync(id);

        if (deleted)
        {
            _logger.LogInformation("User tanúsítvány törölve: Id={Id}", id);
        }

        return deleted;
    }

    private static UserCertDto MapToDto(UserCertEntity entity)
    {
        return new UserCertDto
        {
            Id = entity.Id,
            RootCertId = entity.RootCertId,
            SubjectName = entity.SubjectName,
            NotBefore = entity.NotBefore,
            NotAfter = entity.NotAfter,
            Thumbprint = entity.Thumbprint,
            CreatedAt = entity.CreatedAt
        };
    }
}
