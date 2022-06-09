using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OCC.HSM.Model.Interfaces;
using System;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the summary page, supports all configured questions.
    /// </summary>
    public class AboutServiceModel : HSMPage
    {
        private IConfiguration _configuration;
                
        public string ReturnHeatPump { get; set; }

        /// <inheritdoc/>
        public AboutServiceModel(
            ILogger logger,
            IApplicationConfiguration configuration,
            IConfiguration iConfig)
            : base(logger, configuration)
        {
            if (Configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _configuration = iConfig;
        }

        #region Public Properties        

        public string EnergyRatingOfDwellings2012Url => _configuration.GetValue<string>("AppConfigSettings:EnergyRatingOfDwellings2012Url");
        public string BEISGreenhouseGasReportingUrl => _configuration.GetValue<string>("AppConfigSettings:BEISGreenhouseGasReportingUrl");

        public string PriceCapUnitRatesUrl => _configuration.GetValue<string>("AppConfigSettings:PriceCapUnitRatesUrl");

        public string BEISQuarterlyEnergyPricesUrl => _configuration.GetValue<string>("AppConfigSettings:BEISQuarterlyEnergyPricesUrl");        
        public string McsCertifiedUrl => _configuration.GetValue<string>("AppConfigSettings:McsCertifiedUrl");        
        public string OfgemPriceCapUrl => _configuration.GetValue<string>("AppConfigSettings:OfgemPriceCapUrl");
        public string BEISCostDomesticElectrificationUrl => _configuration.GetValue<string>("AppConfigSettings:BEISCostDomesticElectrificationUrl"); 
        public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");       

        #endregion

        public IActionResult OnGet()
        {
            RemoveRedirectPageSession(HttpContext.Request.Path.Value);

            if(HttpContext.Session.GetString("HeatPump") != null)
            {
                ReturnHeatPump = HttpContext.Session.GetString("HeatPump").ToString(); 
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            return Page();
        }
    }
}