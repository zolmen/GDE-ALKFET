using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CertStore.Infrastructure.Entities;

public class UserCertEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [BsonRepresentation(BsonType.String)]
    public Guid RootCertId { get; set; }
    public string SubjectName { get; set; } = null!;
    public string CertificateDataBase64 { get; set; } = null!;
    public DateTime NotBefore { get; set; }
    public DateTime NotAfter { get; set; }
    public string Thumbprint { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
