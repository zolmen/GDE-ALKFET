using CertStore.API.Services.Interfaces;
using CertStore.Domain.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CertStore.API.Controllers;

[ApiController]
[Route("api/root-cert")]
public class RootCertController : ControllerBase
{
    private readonly IRootCertService _rootCertService;
    private readonly ILogger<RootCertController> _logger;

    public RootCertController(IRootCertService rootCertService,
        ILogger<RootCertController> logger)
    {
        _rootCertService = rootCertService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRootCerts()
    {
        try
        {
            var result = await _rootCertService.GetAllRootCertsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hiba a root tanúsítványok lekérdezésekor");
            return Problem(ex.Message);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRootCert(Guid id)
    {
        try
        {
            var result = await _rootCertService.GetRootCertByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hiba a root tanúsítvány lekérdezésekor: Id={Id}", id);
            return Problem(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRootCert([FromBody] CreateRootCertDto dto)
    {
        try
        {
            var result = await _rootCertService.CreateRootCertAsync(dto);
            return CreatedAtAction(nameof(GetRootCert), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hiba a root tanúsítvány létrehozásakor");
            return Problem(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRootCert(Guid id)
    {
        try
        {
            var result = await _rootCertService.DeleteRootCertAsync(id);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hiba a root tanúsítvány törlésekor: Id={Id}", id);
            return Problem(ex.Message);
        }
    }
}
