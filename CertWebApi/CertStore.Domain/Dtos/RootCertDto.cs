namespace CertStore.Domain.Dtos;

public class RootCertDto
{
    public Guid Id { get; set; }
    public string SubjectName { get; set; } = null!;
    public DateTime NotBefore { get; set; }
    public DateTime NotAfter { get; set; }
    public string Thumbprint { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int UserCertCount { get; set; }
}
