using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCC.HSM.Model.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the summary page, supports all configured questions.
    /// </summary>
    public class AccessibilityStatementModel : HSMPage
    {
        private IConfiguration _configuration;
        
        /// <inheritdoc/>
        public AccessibilityStatementModel(ILogger logger, IApplicationConfiguration configuration, IConfiguration iConfig)
            : base(logger, configuration)
        {
            if (Configuration == null)
                throw new ArgumentNullException(nameof(configuration));

                _configuration = iConfig;
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

        public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");

    }
}