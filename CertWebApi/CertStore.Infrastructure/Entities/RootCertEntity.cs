namespace CertStore.Infrastructure.Entities;

public class RootCertEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SubjectName { get; set; } = null!;
    public string CertificateDataBase64 { get; set; } = null!;
    public string PrivateKeyDataBase64 { get; set; } = null!;
    public DateTime NotBefore { get; set; }
    public DateTime NotAfter { get; set; }
    public string Thumbprint { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
