namespace Elo_fotbalek.Configuration
{
    public class AppConfigurationOptions
    {
        public const string AppConfiguration = "AppConfiguration";

        public string AppName { get; set;}

        public bool IsSeasoningSupported { get; set; }

        public string NonRegularsTitle { get; set; }

        public int PlayerLimit { get; set; }

        public string OverLimitMessage { get; set; }

        public bool IsSmallMatchesEnabled { get; set; }

        public bool IsJirkaLunakEnabled { get; set; }

        public string[] BackgroundImages { get; set; }

        public string BackgroundImagesForJs 
        { 
            get 
            {
                return string.Join(";", this.BackgroundImages);
            } 
        }
    }
}
