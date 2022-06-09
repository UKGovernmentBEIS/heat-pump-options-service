using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OCC.HSM.Model.Interfaces;
using OCC.HSM.UI.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// A page showing the results returned to this application when the answers which
    /// have been provided by the user are processed by the analysis engine.
    /// </summary>
    public class ResultsModel : HSMPage
    {
        private readonly IAnalysisService _analysisService;
        private readonly IConfiguration _configuration;        

        #region Public properties

        public ResultsViewModel ResultsViewModel { get; set; }
        public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");
        
        /// <summary>
        /// Results page always needs to return to the Summary page.
        /// </summary>
        override public string ReturnPage => "Summary";

        /// <summary>
        /// This is set to give the user error messages.
        /// </summary>
        [ViewData]
        public string Message { get; private set; }

        #endregion

        /// <summary>
        /// Create a new instance with the service and logger.
        /// </summary>
        public ResultsModel(
            ILogger logger,
            IApplicationConfiguration configuration,
            IAnalysisService analysisService,           
            IConfiguration iConfig)
            : base(logger, configuration)
        {
            _configuration = iConfig;
            _analysisService = analysisService;            

            ResultsViewModel = new ResultsViewModel(_configuration);            
        }

        /// <summary>
        /// Cannot be on this page unless all of the questions have been answered.
        /// </summary>
        /// <returns>The first question needing an answer or the current page.</returns>
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
                if(userAnswers == null)
                {
                    Message = "Sorry, there is a problem with the service. Please try again.";
                    TempData["ErrorMsg"] = Message;
                    Logger.Error("An error occurred during loading the results.");
                    return Page();
                }
                
                ResultsViewModel.EohResult = _analysisService.GetResult(userAnswers);
            }
            catch (Exception ex)
            {
                Message = "Sorry, there is a problem with the service. Please try again.";
                TempData["ErrorMsg"] = Message;                
                Logger.Exception(ex);
            }            

            return Page();
        }

        public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
        {
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
                    Logger.Error("An error occurred during loading the results.");
                    return Page();
                }

                ResultsViewModel.EohResult = _analysisService.GetResult(userAnswers);
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