namespace Elo_fotbalek.Configuration
{
    using System.Linq;

    public class AppConfigurationOptions
    {
        public const string AppConfiguration = "AppConfiguration";

        public string AppName { get; set;} = "Elo-fotbalek";

        public bool IsSeasoningSupported { get; set; }

        public string NonRegularsTitle { get; set; } = "Non-Regulars";

        public int PlayerLimit { get; set; }

        public string OverLimitMessage { get; set; } = string.Empty;

        public bool IsSmallMatchesEnabled { get; set; }

        public bool IsJirkaLunakEnabled { get; set; }

        public string[] BackgroundImages { get; set; } = Array.Empty<string>();

        public bool IsDoodleEnabled { get; set; }

        public string BackgroundImagesForJs 
        { 
            get 
            {
                return this.BackgroundImages != null && this.BackgroundImages.Count() > 0
                    ? string.Join(";", this.BackgroundImages)
                    : string.Empty;
            } 
        }

        public int AmountOfMonthsToBeCounted { get; set; }
    }
}
