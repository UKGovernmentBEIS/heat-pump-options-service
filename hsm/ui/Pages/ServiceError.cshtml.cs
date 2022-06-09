using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using OCC.HSM.Model.Interfaces;

namespace OCC.HSM.UI.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ServiceErrorModel : HSMPage
    {
        private IConfiguration _configuration;
        public ServiceErrorModel(IApplicationConfiguration configuration, ILogger logger, IConfiguration iConfig)
        : base(logger, configuration)
        {
            _configuration = iConfig;
        }
        public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ExceptionMessage { get; set; }
        public bool DisabledCookies { get; set; }

        public void OnGet(string code)
        {
            switch (code)
            {
                case "400":
                    DisabledCookies = true;
                    break;
                default:
                    break;
            }
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var exceptionHandlerPathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
            {
                ExceptionMessage = "File error thrown";
            }
            if (exceptionHandlerPathFeature?.Path == "/postcode")
            {
                ExceptionMessage += " from home page";
            }
        }
    }
    
}