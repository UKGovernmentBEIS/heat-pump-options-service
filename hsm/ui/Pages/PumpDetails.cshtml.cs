using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OCC.HSM.Model.Interfaces;
using OCC.HSM.UI.Pages.Enums;
using OCC.HSM.UI.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the summary page, supports all configured questions.
    /// </summary>
    public class PumpDetailsModel : HSMPage
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private IAnalysisService _analysisService { get; }

        private IConfiguration _configuration;

        public PumpDetailsViewModel PumpDetailsViewModel { get; set; }

        /// <summary>
        /// This is set to give the user error messages.
        /// </summary>
        public string Message { get; private set; }
        public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");
        public string HeatPumpName { get; set; }

        /// <summary>
        /// Results page always needs to return to the Summary page.
        /// </summary>
        override public string ReturnPage => "Results";

        /// <inheritdoc/>
        public PumpDetailsModel(
            ILogger logger,
            IApplicationConfiguration configuration,
            IAnalysisService analysisService,
            IConfiguration iConfig,
            IHttpContextAccessor httpContextAccessor)
            : base(logger, configuration)
        {
            if (Configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _httpContextAccessor = httpContextAccessor;
            _analysisService = analysisService;
            _configuration = iConfig;

            PumpDetailsViewModel = new PumpDetailsViewModel(logger, _configuration);            
        }

        public async Task<IActionResult> OnGet(string heatpump, CancellationToken cancellationToken)
        {
            heatpump = heatpump ?? HttpContext.Session.GetString("HeatPump");

            RemoveRedirectPageSession(HttpContext.Request.Path.Value);

            if (heatpump == null)
            {
                return RedirectToPage("/Results");
            }            

            if (!OnAcceptablePage())
            {
                return NextQuestion();
            }

            _httpContextAccessor.HttpContext.Session.SetString("PumpDetails-HeatPump", heatpump);

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

                PumpDetailsViewModel.EohResult = _analysisService.GetResult(userAnswers);

                EnumHeatPumpType heatPumpType;
                if (Enum.TryParse(heatpump, out heatPumpType))
                {
                    PumpDetailsViewModel.HeatPump = heatPumpType;
                    PumpDetailsViewModel.SetModelProperties(heatPumpType);
                    HeatPumpName = heatPumpType.ToString();
                    return Page();
                }
                else
                {
                    Message = "Sorry, there is a problem with the service. Please try again later.";
                    TempData["ErrorMsg"] = Message;
                    Logger.Error("Heat pump details cannot be found. Wrong enum string value.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Message = "Sorry, there is a problem with the service. Please try again.";
                TempData["ErrorMsg"] = Message;
                Logger.Exception(ex);
                return Page();
            }
        }

        public async Task<IActionResult> OnPost(EnumHeatPumpType heatpump, CancellationToken cancellationToken)
        {
            RemoveRedirectPageSession(HttpContext.Request.Path.Value);

            if (!OnAcceptablePage())
            {
                return NextQuestion();
            }

            _httpContextAccessor.HttpContext.Session.SetString("PumpDetails-HeatPump", heatpump.ToString());

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

                PumpDetailsViewModel.EohResult = _analysisService.GetResult(userAnswers);
                PumpDetailsViewModel.HeatPump = heatpump;
                PumpDetailsViewModel.SetModelProperties(heatpump);
                HeatPumpName = heatpump.ToString();                
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