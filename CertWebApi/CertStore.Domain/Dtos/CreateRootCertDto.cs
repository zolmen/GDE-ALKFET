namespace CertStore.Domain.Dtos;

public class CreateRootCertDto
{
    public string SubjectName { get; set; } = null!;
    public int ValidityDays { get; set; } = 3650;
}
