using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictScopeManager _scopeManager;
        private readonly IConfiguration _configuration;

        public SetupController(
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictScopeManager scopeManager,
            IConfiguration configuration)
        {
            _applicationManager = applicationManager;
            _scopeManager = scopeManager;
            _configuration = configuration;
        }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestDatabaseConnection([FromServices] ApplicationDbContext context)
        {
            try
            {
                // Zaman sayacı başlat
                var watch = System.Diagnostics.Stopwatch.StartNew();

                // Sadece bağlantıyı dene
                var canConnect = await context.Database.CanConnectAsync();

                watch.Stop();

                if (canConnect)
                    return Ok($"✅ BAĞLANTI BAŞARILI! Süre: {watch.ElapsedMilliseconds} ms");
                else
                    return BadRequest($"❌ Bağlantı başarısız oldu (Hata fırlatmadı ama false döndü). Süre: {watch.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "❌ PATLADI! Veritabanına erişilemiyor.",
                    Error = ex.Message,
                    Inner = ex.InnerException?.Message
                });
            }
        }
    }
}
