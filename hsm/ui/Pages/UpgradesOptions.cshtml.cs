using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OCC.HSM.Model.Interfaces;
using OCC.HSM.UI.ViewModels;
using System;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the summary page, supports all configured questions.
    /// </summary>
    public class UpgradesOptionsModel : HSMPage
    {
        private IConfiguration _configuration;

        override public string ReturnPage => "Results";                

        public NextStepsViewModel NextStepsViewModel { get; set; }

        /// <inheritdoc/>
        public UpgradesOptionsModel(
            ILogger logger,
            IApplicationConfiguration configuration,
            IConfiguration iConfig)
            : base(logger, configuration)
        {
            if (Configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _configuration = iConfig;

            NextStepsViewModel = new NextStepsViewModel(iConfig);            
        }

        public IActionResult OnGet()
        {
            RemoveRedirectPageSession(HttpContext.Request.Path.Value);            

            return Page();
        }

        public IActionResult OnPost()
        {
            return Page();
        }

        #region Public properties
        public string SimpleEnergyAdviceToolUrl => _configuration.GetValue<string>("AppConfigSettings:SimpleEnergyAdviceToolUrl");
        public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");

        #endregion
    }
}