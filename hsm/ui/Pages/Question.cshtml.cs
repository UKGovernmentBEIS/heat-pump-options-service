using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;


namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// The model for the question page, supports all configured questions.
    /// </summary>
    public class QuestionModel : HSMPage
    {
        private IConfiguration _configuration;
        /// <summary>
        /// The unique key for this question.
        /// </summary>
        public string Key { get; private set; }

        public string BreadCrumbsText { get; set; }

        /// <summary>
        /// The prompt to be displayed for the question.
        /// </summary>
        public string Prompt => Configuration.Questions[Key].Prompt;

        /// <summary>
        /// The set of permitted answers.,
        /// </summary>
        public ReadOnlyCollection<AnswerChoice> Answers => Configuration.Questions[Key].AnswerChoices;

        /// <summary>
        /// The optional explanation text for this question.
        /// </summary>
        public string Explanation => Configuration.Questions[Key].Explanation;

        /// <summary>
        /// Flag indicating if images are to be shown above the answer choices.
        /// </summary>
        public bool HasImages => Configuration.Questions[Key].HasChoiceImages;

        /// <summary>
        /// The single image path to be shown above the answer choices if there are no separate images for each choice.
        /// </summary>
        public string SingleImage => Configuration.Questions[Key].SingleImage;

        /// <summary>
        /// Collects the user's selection from the radio buttons.
        /// </summary>
        [BindProperty]
        public string CurrentChoice { get; set; }

        /// <summary>
        /// An error message.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Returns the total number of questions in the list.
        /// </summary>
        public int TotalQuestions => Configuration.Questions.Count;

        /// <summary>
        /// Returns the current question index in the list.
        /// </summary>
        public int CurrentQuestionIndex
        {
            get
            {
                var question = Configuration.Questions[Key];
                if (question != null)
                {
                    return Configuration.Questions.IndexOf(question);
                }
                return -1;
            }
        }

        override public string ReturnPage
        {
            get
            {
                Question currentQuestion = Configuration.Questions[Key];
                int idx = Configuration.Questions.IndexOf(currentQuestion);
                if (idx == 0)
                {
                    return "Postcode";
                }
                return "Home";
            }
        }

        /// <summary>
        /// Create a new page model for the questions contained in the <paramref name="configuration"/>
        /// </summary>
        public QuestionModel(IApplicationConfiguration configuration, IConfiguration iConfig, ILogger logger)
            : base(logger, configuration)
        {
            _configuration = iConfig;
        }
        public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");

        /// <summary>
        /// The question is being loaded, get the key to the current question from the 
        /// "q" query string, if not present go to the first question and use its key.
        /// </summary>
        public IActionResult OnGet()
        {
            RemoveRedirectPageSession(HttpContext.Request.Path.Value);

            if (!OnAcceptablePage())
                return NextQuestion();

            Key = KeyFromQuery();

            if (string.IsNullOrEmpty(Key))
            {
                Key = HttpContext.Session.GetString("QuestionKey");                
            }

            //If the Key is not present for any reason return to the last answered question
            if (string.IsNullOrEmpty(Key))
            {
                return NextQuestion();
            }

            var question = Configuration.Questions[Key];
            BreadCrumbsText = question.BreadcrumbText;

            CurrentChoice = GetStoredString(Key);

            //We don't want to ask the user this question
            bool isEpcPresent = Convert.ToBoolean(HttpContext.Session.GetString("IsEpcPresent"));
            if (!isEpcPresent && Key.Equals("housesizeoption1") && CurrentChoice == null)
            {
                SetQuestionHidden("housesizeoption1", true, true);
                StoreString("housesizeoption1", "hidden");
                return NextQuestion();
            }

            if (Key.Equals("currentheatingsystem") && CurrentChoice != null && CurrentChoice.Equals($"{ Key}-gas"))
            {
                //Answer the Gas supply question and mark it as hidden
                StoreString("gassupply", "gassupply-yes");
                SetQuestionHidden("gassupply", false, true);
            }
            return Page();
        }

        /// <summary>
        /// The current page has been submitted
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPost()
        {
            Key = KeyFromQuery();

            if (string.IsNullOrEmpty(Key))
            {
                Key = HttpContext.Session.GetString("QuestionKey");
                HttpContext.Session.Remove("QuestionKey");
            }

            var question = Configuration.Questions[Key];
            BreadCrumbsText = question.BreadcrumbText;

            if (question != null)
            {
                if (Request.Headers["Referer"].ToString().EndsWith("Summary"))
                {
                    CurrentChoice = GetStoredString(Key);
                    return Page();
                }

                if (CurrentChoice == null)
                {
                    Error = "Please select an option before moving on";
                    BreadCrumbsText = $"Error - {BreadCrumbsText}";
                    return Page();
                }
                StoreString(Key, CurrentChoice ?? String.Empty);

                //Skip question according to the result
                //If the answer to the Current heating system is Gas, then auto-answer the Gas supply question
                if (Key.Equals("currentheatingsystem"))
                {
                    if (CurrentChoice.Equals($"{ Key}-gas"))
                    {
                        SetQuestionHidden("gassupply", false, true);
                        StoreString("gassupply", "gassupply-yes");
                    }
                    else
                    {
                        SetQuestionHidden("gassupply", false, false);
                        ClearValue("gassupply");
                    }
                }
            }
            return NextQuestion("Summary");
        }

        /// <summary>
        /// This handler gets called when the Back button is clicked. Finds the previous question and redirects back to it.
        /// </summary>
        /// <param name="key">The current's page question key</param>
        /// <returns></returns>
        public IActionResult OnPostPreviousQuestion(string key)
        {
            Key = key;
            var question = Configuration.Questions[Key];
            if (question != null)
            {
                int idx = Configuration.Questions.IndexOf(question);
                if (idx > 0)
                {
                    bool isEpcPresent = Convert.ToBoolean(HttpContext.Session.GetString("IsEpcPresent"));
                    if (isEpcPresent && Configuration.Questions[idx - 1].Key.Equals("housesizeoption2"))
                    {
                        idx--;
                    }
                    else if (!isEpcPresent && Configuration.Questions[idx - 1].Key.Equals("housesizeoption1"))
                    {
                        idx--;
                    }
                    return RedirectToPage("Question", new { q = Configuration.Questions[--idx].Key });
                }
            }

            return NextQuestion(key);
        }
    }
}