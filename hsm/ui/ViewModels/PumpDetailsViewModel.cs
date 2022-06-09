
using Microsoft.Extensions.Configuration;
using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using OCC.HSM.UI.Pages.Enums;
using OCC.HSM.UI.Pages.Helpers;
using System;

namespace OCC.HSM.UI.ViewModels
{
    public class PumpDetailsViewModel : PotentialUpgradesViewModel
    {
        private readonly IConfiguration _configuration;
        public NextStepsViewModel NextStepsViewModel { get; set; }

        /// <summary>
        /// For logging to a local logfile.
        /// </summary>
        private ILogger _logger { get; }

        /// <summary>
        /// Create a new instance with the service and logger.
        /// </summary>
        public PumpDetailsViewModel(ILogger logger, IConfiguration iConfig)
        {
            _configuration = iConfig;
            _logger = logger;
            NextStepsViewModel = new NextStepsViewModel(iConfig);            
        }

        #region Public Properties


        public EnumHeatPumpType HeatPump { get; set; }

        /// <summary>
        /// Heat pump type.
        /// </summary>
        public string HeatPumpType { get; set; }

        /// <summary>
        /// Heat pump suitability.
        /// </summary>
        public ushort HeatPumpSuitable { get; set; }

        /// <summary>
        /// Carbon Emissions / Savings.
        /// </summary>
        public double? CarbonEmissionSavings { get; set; }

        /// <summary>
        /// Min Heat Pump System Running Costs (%).
        /// </summary>
        public double? HeatPumpRunCostMin { get; set; }

        /// <summary>
        /// Max Heat Pump System Running Costs (%).
        /// </summary>
        public double? HeatPumpRunCostMax { get; set; }

        /// <summary>
        /// Min Heat Pump System Equipment and Installation Costs.
        /// </summary>
        public double? HeatPumpEquipCostMin { get; set; }

        /// <summary>
        /// Max Heat Pump System Equipment and Installation Costs.
        /// </summary>
        public double? HeatPumpEquipCostMax { get; set; }

        public string SimpleEnergyAdviceToolUrl => _configuration.GetValue<string>("AppConfigSettings:SimpleEnergyAdviceToolUrl");
        public string SimpleEnergyAdviceToolScotlandUrl => _configuration.GetValue<string>("AppConfigSettings:SimpleEnergyAdviceToolScotlandUrl");

        public string SeaAirSourceHeatPumpsUrl => _configuration.GetValue<string>("AppConfigSettings:SeaAirSourceHeatPumpsUrl");

        public string SeaHighTempAirSourceHeatPumpsUrl => _configuration.GetValue<string>("AppConfigSettings:SeaHighTempAirSourceHeatPumpsUrl");

        public string SeaGroundSourceHeatPumpsUrl => _configuration.GetValue<string>("AppConfigSettings:SeaGroundSourceHeatPumpsUrl");

        public string SeaHybridHeatPumpsUrl => _configuration.GetValue<string>("AppConfigSettings:SeaHybridHeatPumpsUrl");

        public string SeaEnergyEfficiencyCalculatorUrl => _configuration.GetValue<string>("AppConfigSettings:SeaEnergyEfficiencyCalculatorUrl");

        public string TrustMarkUrl => _configuration.GetValue<string>("AppConfigSettings:TrustMarkUrl");

        public string McsCertifiedUrl => _configuration.GetValue<string>("AppConfigSettings:McsCertifiedUrl");

        public string SimpleEnergyAdviceGrantsUrl => _configuration.GetValue<string>("AppConfigSettings:SimpleEnergyAdviceGrantsUrl");

        public string HomeEnergyScotlandGrantsUrl => _configuration.GetValue<string>("AppConfigSettings:HomeEnergyScotlandGrantsUrl");

        public string McsInstallerUrl => _configuration.GetValue<string>("AppConfigSettings:McsInstallerUrl");

        public string GovukNetZeroStrategyUrl => _configuration.GetValue<string>("AppConfigSettings:GovukNetZeroStrategyUrl");

        public Eoh EohResult { get; set; }

        public bool IsAirSourceVisible { get; set; }

        public bool IsHighTempAirSourceVisible { get; set; }

        public bool IsGroundSourceVisible { get; set; }

        public bool IsHybridVisible { get; set; }

        #endregion
                
        /// <summary>
        /// Map the results properties to the page model ones
        /// </summary>
        public void SetModelProperties(EnumHeatPumpType heatpump)
        {
            try
            {
                switch (heatpump)
                {
                    case EnumHeatPumpType.AirSource:
                        IsAirSourceVisible = true;

                        HeatPumpType = Extensions.GetDescription(EnumHeatPumpType.AirSource);
                        HeatPumpSuitable = EohResult.LtAshpSuitable;

                        HeatPumpRunCostMin = EohResult.LtAshpRunCostMin;
                        HeatPumpRunCostMax = EohResult.LtAshpRunCostMax;

                        HeatPumpEquipCostMin = EohResult.LtAshpEquipCostMin;
                        HeatPumpEquipCostMax = EohResult.LtAshpEquipCostMax;

                        CarbonEmissionSavings = EohResult.LtAshpEmissionSavings * 100;

                        GetPotentialUpgradesHeatPump(EohResult);

                        break;

                    case EnumHeatPumpType.HighTempAirSource:
                        IsHighTempAirSourceVisible = true;

                        HeatPumpType = Extensions.GetDescription(EnumHeatPumpType.HighTempAirSource);
                        HeatPumpSuitable = EohResult.HtAshpSuitable;

                        HeatPumpRunCostMin = EohResult.HtAshpRunCostMin;
                        HeatPumpRunCostMax = EohResult.HtAshpRunCostMax;

                        HeatPumpEquipCostMin = EohResult.HtAshpEquipCostMin;
                        HeatPumpEquipCostMax = EohResult.HtAshpEquipCostMax;

                        CarbonEmissionSavings = EohResult.HtAshpEmissionSavings * 100;

                        GetPotentialUpgradesHeatPump(EohResult);

                        break;

                    case EnumHeatPumpType.GroundSource:
                        IsGroundSourceVisible = true;

                        HeatPumpType = Extensions.GetDescription(EnumHeatPumpType.GroundSource);
                        HeatPumpSuitable = EohResult.GshpSuitable;

                        HeatPumpEquipCostMin = EohResult.GshpEquipCostMin;
                        HeatPumpEquipCostMax = EohResult.GshpEquipCostMax;

                        HeatPumpRunCostMin = EohResult.GshpRunCostMin;
                        HeatPumpRunCostMax = EohResult.GshpRunCostMax;

                        CarbonEmissionSavings = EohResult.GshpEmissionSavings * 100;

                        GetPotentialUpgradesHeatPump(EohResult);

                        break;

                    case EnumHeatPumpType.Hybrid:
                        IsHybridVisible = true;

                        HeatPumpType = Extensions.GetDescription(EnumHeatPumpType.Hybrid);
                        HeatPumpSuitable = EohResult.HhpSuitable;

                        HeatPumpRunCostMin = EohResult.HhpRunCostMin;
                        HeatPumpRunCostMax = EohResult.HhpRunCostMax;

                        HeatPumpEquipCostMin = EohResult.HhpEquipCostMin;
                        HeatPumpEquipCostMax = EohResult.HhpEquipCostMax;

                        CarbonEmissionSavings = EohResult.HhpEmissionSavings * 100;

                        GetPotentialUpgradesHybrid(EohResult);

                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }
        }
    }
}
