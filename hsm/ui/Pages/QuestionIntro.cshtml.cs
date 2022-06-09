using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCC.HSM.Model.Interfaces;
using System;
using Microsoft.Extensions.Configuration;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the summary page, supports all configured questions.
    /// </summary>
    public class QuestionIntroModel : HSMPage
    {
        private IConfiguration _configuration;
        public QuestionIntroModel(
           ILogger logger,
           IApplicationConfiguration configuration, IConfiguration iConfig)
           : base(logger, configuration)
        {
            if (Configuration == null)
                throw new ArgumentNullException(nameof(configuration));
                
            _configuration = iConfig;
        }

        public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");

        /// <summary>
        /// Page always needs to return to the EPCSummary page.
        /// </summary>
        override public string ReturnPage => "EPCSummary";

        public IActionResult OnGet()
        {            
            RemoveRedirectPageSession(HttpContext.Request.Path.Value);

            //If the session doesn't exist, we get false as a result, which is what we need
            IsEpcPresent = Convert.ToBoolean(HttpContext.Session.GetString("IsEpcPresent"));
            ContinueWithEpc = Convert.ToBoolean(HttpContext.Session.GetString("ContinueWithEpc"));

            return Page();
        }

        public IActionResult OnPost()
        {
            //If the session doesn't exist, we get false as a result, which is what we need
            IsEpcPresent = Convert.ToBoolean(HttpContext.Session.GetString("IsEpcPresent"));
            ContinueWithEpc = Convert.ToBoolean(HttpContext.Session.GetString("ContinueWithEpc"));

            if (ContinueWithEpc && IsEpcPresent)
            {
                SetSkipAnsweredQuestions(true);
                return NextQuestion("Question");
            }
            else
            {
                SetSkipAnsweredQuestions(false);
                ClearAnswers();
                return NextQuestion("Question");
            }            
        }
    }
}