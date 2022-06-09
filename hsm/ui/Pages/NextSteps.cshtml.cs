using Microsoft.AspNetCore.Mvc;
using OCC.HSM.Model.Interfaces;
using Microsoft.AspNetCore.Http;

namespace OCC.HSM.UI.Pages
{
	/// <summary>
	/// A page to display the options available to the user if they wish to find out more
	/// about installing a heat pump.
	/// </summary>
	public class NextStepsModel : HSMPage
	{
		/// <inheritdoc/>
		public NextStepsModel(ILogger logger, IApplicationConfiguration configuration)
			: base(logger, configuration)
		{
		}

		/// <summary>
		/// Cannot be on this page unless all of the questions have been answered.
		/// </summary>
		/// <returns>The first question needing an answer or the current page.</returns>
		public IActionResult OnGet()
		{
			RemoveRedirectPageSession(HttpContext.Request.Path.Value);

			return OnAcceptablePage() ? Page() : NextQuestion();
		}
	}
}