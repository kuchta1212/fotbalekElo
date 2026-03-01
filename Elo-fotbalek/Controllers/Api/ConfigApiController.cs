using Elo_fotbalek.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Elo_fotbalek.Controllers.Api
{
    [Route("api/config")]
    public class ConfigApiController : BaseApiController
    {
        private readonly IOptions<AppConfigurationOptions> appConfig;

        public ConfigApiController(IOptions<AppConfigurationOptions> appConfig)
        {
            this.appConfig = appConfig;
        }

        [HttpGet]
        public IActionResult GetConfig()
        {
            var config = appConfig.Value;
            
            var response = new
            {
                appName = config.AppName,
                isSeasoningSupported = config.IsSeasoningSupported,
                nonRegularsTitle = config.NonRegularsTitle,
                playerLimit = config.PlayerLimit,
                overLimitMessage = config.OverLimitMessage,
                isSmallMatchesEnabled = config.IsSmallMatchesEnabled,
                isJirkaLunakEnabled = config.IsJirkaLunakEnabled,
                isDoodleEnabled = config.IsDoodleEnabled
            };

            return Ok(response);
        }
    }
}
