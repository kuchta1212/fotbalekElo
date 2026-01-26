using Microsoft.AspNetCore.Mvc;

namespace Elo_fotbalek.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult Ok<T>(T data)
        {
            return base.Ok(new { success = true, data });
        }

        protected IActionResult BadRequest(string message)
        {
            return base.BadRequest(new { success = false, error = message });
        }

        protected IActionResult NotFound(string message)
        {
            return base.NotFound(new { success = false, error = message });
        }

        protected IActionResult ServerError(string message)
        {
            return StatusCode(500, new { success = false, error = message });
        }
    }
}
