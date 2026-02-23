namespace CertStore.Domain.Dtos;

public class SignUserCertDto
{
    public Guid RootCertId { get; set; }
    public string CsrBase64 { get; set; } = null!;
    public int ValidityDays { get; set; } = 365;
}
