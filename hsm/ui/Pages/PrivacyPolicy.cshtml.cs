using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCC.HSM.Model.Interfaces;
using System;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the summary page, supports all configured questions.
    /// </summary>
    public class PrivacyPolicyModel : HSMPage
    {
        /// <inheritdoc/>
        public PrivacyPolicyModel(ILogger logger, IApplicationConfiguration configuration)
            : base(logger, configuration)
        {
            if (Configuration == null)
                throw new ArgumentNullException(nameof(configuration));
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
    }
}