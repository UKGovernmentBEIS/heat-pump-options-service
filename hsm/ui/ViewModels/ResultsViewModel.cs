using Microsoft.Extensions.Configuration;
using OCC.HSM.Model.Entities;
using OCC.HSM.UI.Pages.Enums;

namespace OCC.HSM.UI.ViewModels
{

    public class ResultsViewModel
    {
        private readonly IConfiguration _configuration;
        private int _numberOfSuitableHeatPumps;        

        #region Public properties

        public Eoh EohResult { get; set; }

        public NextStepsViewModel NextStepsViewModel { get; set; }        

        public string HeatPumpAssociationUrl => _configuration.GetValue<string>("AppConfigSettings:HeatPumpAssociationUrl");

        public string GroundSourcePumpAssociationUrl => _configuration.GetValue<string>("AppConfigSettings:GroundSourcePumpAssociationUrl");

        public string HeatPumpFederationUrl => _configuration.GetValue<string>("AppConfigSettings:HeatPumpFederationUrl");

        public string ESCatapultHeatPumpCaseStudyUrl => _configuration.GetValue<string>("AppConfigSettings:ESCatapultHeatPumpCaseStudyUrl");

        public string SimpleEnergyAdviceSolarPhotovoltaicUrl => _configuration.GetValue<string>("AppConfigSettings:SimpleEnergyAdviceSolarPhotovoltaicUrl");

        public string SimpleEnergyAdviceGrantsUrl => _configuration.GetValue<string>("AppConfigSettings:SimpleEnergyAdviceGrantsUrl");

        public string HomeEnergyScotlandGrantsUrl => _configuration.GetValue<string>("AppConfigSettings:HomeEnergyScotlandGrantsUrl");

        public string McsInstallerUrl => _configuration.GetValue<string>("AppConfigSettings:McsInstallerUrl");

        public int NumberOfSuitableHeatPumps
        {
            get
            {
                if (EohResult.LtAshpSuitable == (ushort)EnumHeatPumpSuitability.Suitable)
                _numberOfSuitableHeatPumps++;
                if (EohResult.HtAshpSuitable == (ushort)EnumHeatPumpSuitability.Suitable)
                _numberOfSuitableHeatPumps++;
                if (EohResult.GshpSuitable == (ushort)EnumHeatPumpSuitability.Suitable)
                _numberOfSuitableHeatPumps++;
                if (EohResult.HhpSuitable == (ushort)EnumHeatPumpSuitability.Suitable)
                _numberOfSuitableHeatPumps++;

                return _numberOfSuitableHeatPumps;
            }
        }

        #endregion

        /// <summary>
        /// Create a new instance with the service and logger.
        /// </summary>
        public ResultsViewModel(IConfiguration iConfig)            
        {
            _configuration = iConfig;
            NextStepsViewModel = new NextStepsViewModel(iConfig);            
        }

    }
}
