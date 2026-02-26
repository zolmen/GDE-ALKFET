using CertStore.API.Services.Interfaces;
using CertStore.Domain.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CertStore.API.Controllers;

[ApiController]
[Route("api/user-cert")]
public class UserCertController : ControllerBase
{
    private readonly IUserCertService _userCertService;
    private readonly ILogger<UserCertController> _logger;

    public UserCertController(IUserCertService userCertService,
        ILogger<UserCertController> logger)
    {
        _userCertService = userCertService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUserCerts([FromQuery] Guid? rootCertId)
    {
        try
        {
            var result = rootCertId.HasValue
                ? await _userCertService.GetUserCertsByRootIdAsync(rootCertId.Value)
                : await _userCertService.GetAllUserCertsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hiba a user tanúsítványok lekérdezésekor");
            return Problem(ex.Message);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserCert(Guid id)
    {
        try
        {
            var result = await _userCertService.GetUserCertByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hiba a user tanúsítvány lekérdezésekor: Id={Id}", id);
            return Problem(ex.Message);
        }
    }

    [HttpPost("sign")]
    public async Task<IActionResult> SignUserCert([FromBody] SignUserCertDto dto)
    {
        try
        {
            var result = await _userCertService.SignUserCertAsync(dto);
            return CreatedAtAction(nameof(GetUserCert), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "InvalidOperationException a user tanúsítvány aláírásakor: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            _logger.LogError(ex, "Kriptográfiai hiba az aláíráskor: {Message}", ex.Message);
            return BadRequest($"Kriptográfiai hiba: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hiba a user tanúsítvány aláírásakor. Típus: {Type}, {Message}", ex.GetType().Name, ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUserCert(Guid id)
    {
        try
        {
            var result = await _userCertService.DeleteUserCertAsync(id);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hiba a user tanúsítvány törlésekor: Id={Id}", id);
            return Problem(ex.Message);
        }
    }
}
