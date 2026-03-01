using Elo_fotbalek.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Elo_fotbalek.Controllers.Api
{
    [Route("api/background-images")]
    public class BackgroundImagesApiController : BaseApiController
    {
        private readonly IOptions<AppConfigurationOptions> appConfig;

        public BackgroundImagesApiController(IOptions<AppConfigurationOptions> appConfig)
        {
            this.appConfig = appConfig;
        }

        [HttpGet]
        public IActionResult GetBackgroundImages()
        {
            var config = appConfig.Value;
            
            var response = new
            {
                images = config.BackgroundImages ?? new string[0],
                rotationInterval = 30 // seconds - configurable if needed
            };

            return Ok(response);
        }
    }
}
