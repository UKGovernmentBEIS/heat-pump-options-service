using Microsoft.Extensions.Configuration;

namespace OCC.HSM.UI.ViewModels
{
    public class NextStepsViewModel
    {
        private readonly IConfiguration _configuration;

        public string SimpleEnergyAdviceGrantsUrl => _configuration.GetValue<string>("AppConfigSettings:SimpleEnergyAdviceGrantsUrl");        

        public string HomeEnergyScotlandGrantsUrl => _configuration.GetValue<string>("AppConfigSettings:HomeEnergyScotlandGrantsUrl");

        public string McsInstallerUrl => _configuration.GetValue<string>("AppConfigSettings:McsInstallerUrl");

        public string GovukBoilerUpgradeSchemeUrl => _configuration.GetValue<string>("AppConfigSettings:GovukBoilerUpgradeSchemeUrl");  

        /// <summary>
        /// Create a new instance with the service and logger.
        /// </summary>
        public NextStepsViewModel(IConfiguration iConfig)
        {
            _configuration = iConfig;
        }
    }
}
