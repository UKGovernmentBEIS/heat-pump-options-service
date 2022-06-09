using Microsoft.AspNetCore.Mvc;
using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OCC.HSM.UI.ViewModels;
using Microsoft.Extensions.Configuration;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the summary page, supports all configured questions.
    /// </summary>
    public class ResultsComparisonModel : HSMPage
	{
		private IAnalysisService _analysisService { get; }
		private readonly IConfiguration _configuration;   

		/// <inheritdoc/>
		public ResultsComparisonModel(
			ILogger logger, 
			IApplicationConfiguration configuration,
			IConfiguration iConfig,
			IAnalysisService analysisService)
			: base(logger, configuration)
		{
			if (Configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			_analysisService = analysisService;
			_configuration = iConfig;

			ResultsComparisonViewModel = new ResultsComparisonViewModel(iConfig);			
		}

		/// <summary>
		/// This is set to give the user error messages.
		/// </summary>
		public string Message { get; private set; }
		public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");

		/// <summary>
		/// ResultsComparison page always needs to return to the Results page.
		/// </summary>
		override public string ReturnPage => "Results";      

        public ResultsComparisonViewModel ResultsComparisonViewModel { get; set; }

        public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
		{
			RemoveRedirectPageSession(HttpContext.Request.Path.Value);

			if (!OnAcceptablePage())
			{
				return NextQuestion();
			}

            try
            {
				var userAnswers = _analysisService.GetUserChoices();
				if (userAnswers == null)
				{
					Message = "Sorry, there is a problem with the service. Please try again.";
					TempData["ErrorMsg"] = Message;					
					Logger.Error("User answers cannot be captured.");
					return Page();
				}

				ResultsComparisonViewModel.EohResult = _analysisService.GetResult(userAnswers);
				ResultsComparisonViewModel.GetPotentialUpgradesHeatPump(ResultsComparisonViewModel.EohResult);
			}
			catch (Exception ex)
			{
				Message = "Sorry, there is a problem with the service. Please try again.";
				TempData["ErrorMsg"] = Message;
				Logger.Exception(ex);
			}
			
			return Page();
		}
	}
}