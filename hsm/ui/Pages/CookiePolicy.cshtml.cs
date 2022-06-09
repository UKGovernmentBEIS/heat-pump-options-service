using Microsoft.AspNetCore.Mvc;
using OCC.HSM.Model.Interfaces;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the summary page, supports all configured questions.
    /// </summary>
    public class CookiePolicyModel : HSMPage
    {
        /// <inheritdoc/>
        public CookiePolicyModel(ILogger logger, IApplicationConfiguration configuration
            )
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

        public IActionResult OnPostSetFunctionalCookies()
        {
            return Page();
        }
    }
}