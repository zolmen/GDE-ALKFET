using System.ComponentModel;
using CertStore.API.Services.Interfaces;
using CertStore.Domain.Dtos;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;

namespace CertStore.API.McpTools;

[McpServerToolType]
public static class CertMcpTools
{
    [McpServerTool(Name = "GetRootCerts", Title = "Get all root certificates",
        Destructive = false, ReadOnly = true)]
    [Description("Lekérdezi az összes root (CA) tanúsítványt a tárból.")]
    public static async Task<List<RootCertDto>> GetRootCerts([FromServices] IRootCertService service)
    {
        return await service.GetAllRootCertsAsync();
    }

    [McpServerTool(Name = "GetUserCerts", Title = "Get user certificates",Destructive = false, ReadOnly = true)]
    [Description("Lekérdezi a user tanúsítványokat. Opcionálisan szűr root cert ID alapján.")]
    public static async Task<List<UserCertDto>> GetUserCerts(Guid? rootCertId,[FromServices] IUserCertService service)
    {
        return rootCertId.HasValue
            ? await service.GetUserCertsByRootIdAsync(rootCertId.Value)
            : await service.GetAllUserCertsAsync();
    }

    [McpServerTool(Name = "CreateRootCert", Title = "Create a new root certificate",Destructive = false, ReadOnly = false)]
    [Description("Létrehoz egy új self-signed root (CA) tanúsítványt a megadott subject névvel.")]
    public static async Task<RootCertDto> CreateRootCert(string subjectName,int validityDays,[FromServices] IRootCertService service)
    {
        var dto = new CreateRootCertDto
        {
            SubjectName = subjectName,
            ValidityDays = validityDays > 0 ? validityDays : 3650
        };
        return await service.CreateRootCertAsync(dto);
    }

    [McpServerTool(Name = "SignUserCert", Title = "Sign a user certificate (CSR)", Destructive = false, ReadOnly = false)]
    [Description("Aláír egy CSR-t a megadott root CA tanúsítvánnyal. A CSR-t Base64 formátumban kell megadni.")]
    public static async Task<UserCertDto> SignUserCert(Guid rootCertId,string csrBase64,int validityDays,[FromServices] IUserCertService service)
    {
        var dto = new SignUserCertDto
        {
            RootCertId = rootCertId,
            CsrBase64 = csrBase64,
            ValidityDays = validityDays > 0 ? validityDays : 365
        };
        return await service.SignUserCertAsync(dto);
    }

    [McpServerTool(Name = "DeleteRootCert", Title = "Delete root certificate and its user certs",Destructive = true, ReadOnly = false)]
    [Description("Töröl egy root (CA) tanúsítványt az összes hozzá tartozó user tanúsítvánnyal együtt.")]
    public static async Task<bool> DeleteRootCert(Guid id,[FromServices] IRootCertService service)
    {
        return await service.DeleteRootCertAsync(id);
    }

    [McpServerTool(Name = "DeleteUserCert", Title = "Delete a user certificate",Destructive = true, ReadOnly = false)]
    [Description("Töröl egy user tanúsítványt az ID alapján.")]
    public static async Task<bool> DeleteUserCert(Guid id,[FromServices] IUserCertService service)
    {
        return await service.DeleteUserCertAsync(id);
    }
}
