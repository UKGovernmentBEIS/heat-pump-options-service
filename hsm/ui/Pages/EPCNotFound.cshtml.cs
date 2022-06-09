using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OCC.HSM.Model.Interfaces;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the Home page.
    /// </summary>
    public class EPCNotFoundModel : HSMPage
	{
		private IConfiguration _configuration;

		/// <inheritdoc/>
		public EPCNotFoundModel(IApplicationConfiguration configuration, ILogger logger, IConfiguration iConfig)
			: base(logger, configuration)
		{
			_configuration = iConfig;
		}

		public string Postcode { get; set; }
		public string GovukGetEnergyCertificateUrl => _configuration.GetValue<string>("AppConfigSettings:GovukGetEnergyCertificateUrl"); 
		public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");
		public IActionResult OnGet()
		{
			RemoveRedirectPageSession(HttpContext.Request.Path.Value);

			Postcode = HttpContext.Session.GetString("postcode");
			return Page();
		}

		public IActionResult OnPost()
		{
			return NextQuestion("Question");
		}
	}
}