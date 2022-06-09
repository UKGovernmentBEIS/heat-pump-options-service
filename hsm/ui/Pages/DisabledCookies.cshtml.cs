using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OCC.HSM.Model.Interfaces;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the Home page.
    /// </summary>
    public class DisabledCookiesModel : HSMPage
	{
		private IConfiguration _configuration;

		/// <inheritdoc/>
		public DisabledCookiesModel(IApplicationConfiguration configuration, ILogger logger, IConfiguration iConfig)
			: base(logger, configuration)
		{
			_configuration = iConfig;
		}

		public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");
    }
}