using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// A base class for the pages to share some functionality across pages.
    /// </summary>
    public abstract class HSMPage : PageModel
    {

        private string _returnPage = "/Postcode";
        
        /// <summary>
        /// For logging to a local logfile.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// The questions being asked are contained within the configuration, these are
        /// used to identify items in the EPC which can provide answers to some of the
        /// subsequent questions.
        /// </summary>
        protected IApplicationConfiguration Configuration { get; }

        /// <summary>
        /// The key to the postcode value that may have been saved in the state.
        /// </summary>
        protected const string POSTCODE_KEY = "postcode";

        /// <summary>
        /// The key to the EPC rating that may have been saved in the state.  This is the
        /// A-G rating.
        /// </summary>
        protected const string EPC_RATING_KEY = "epc-rating";

        /// <summary>
        /// Key into the state of the flag ('Y'/'N') for the skip answered questions flag.
        /// </summary>
        private const string SKIP_ANSWERED_QUESTIONS_KEY = "hsm-skip-answered";

        /// <summary>
        /// The key to the date when the rating was lodged.
        /// </summary>
        protected const string EPC_RATING_DATE_KEY = "epc-rating-date";

        /// <summary>
        /// The key to the wall type.
        /// </summary>
        protected const string WALLS_DESCRIPTION_KEY = "wall-type";

        /// <summary>
        /// The key to the roof type.
        /// </summary>
        protected const string ROOF_DESCRIPTION_KEY = "roof-type";

        /// <summary>
        /// The key to the windows type.
        /// </summary>
        protected const string WINDOWS_DESCRIPTION_KEY = "windows-type";

        /// <summary>
        /// The key to the main heat type.
        /// </summary>
        protected const string MAINHEAT_DESCRIPTION_KEY = "mainheat-type";

        /// <summary>
        /// The key to the floor area.
        /// </summary>
        protected const string FLOOR_AREA_KEY = "floor-area";

        /// <summary>
        /// The key to the property type.
        /// </summary>
        protected const string PROPERTY_TYPE_KEY = "property-type";

        /// <summary>
        /// Values in the EPC to save in the state.
        /// </summary>
        protected static readonly (string key, string epcKey)[] STORED_EPC_VALUES = {
            (EPC_RATING_KEY, "current-energy-rating"),
            (EPC_RATING_DATE_KEY, "lodgement-datetime"),
            (WALLS_DESCRIPTION_KEY, "walls-description"),
            (ROOF_DESCRIPTION_KEY, "roof-description"),
            (WINDOWS_DESCRIPTION_KEY, "windows-description"),
            (MAINHEAT_DESCRIPTION_KEY, "mainheat-description"),
            (FLOOR_AREA_KEY, "total-floor-area"),
            (PROPERTY_TYPE_KEY, "property-type")
        };

        public bool IsEpcPresent { get; set; }
       
        public bool ContinueWithEpc { get; set; }
        
        public virtual string ReturnPage
        {
            get
            {
                //Set it first to the page it came from and then set it in a session and always use the session
                if (HttpContext.Session.GetString("ReturnPage") != null)
                {
                    _returnPage = HttpContext.Session.GetString("ReturnPage");
                }
                else
                {
                    _returnPage = $"/{Request.Headers["Referer"].ToString().Split('/').Last()}";

                    //We don't want to go back to the EPC Summary if it's not present
                    if (HttpContext.Session.GetString("ContinueWithEpc") != null)
                    {
                        bool isContinueWithEpc = Convert.ToBoolean(HttpContext.Session.GetString("ContinueWithEpc"));
                        if (!isContinueWithEpc && _returnPage.StartsWith("/EPCSummary"))
                        {
                            return "/Postcode";
                        }
                    }

                    if (_returnPage.Equals("/") || _returnPage.Equals("/?"))
                    {
                        _returnPage = "/Postcode";
                    }

                    if (_returnPage.Contains('?'))
                    {                        
                        string[] strUrl = _returnPage.Split('?');
                        if (strUrl.Length > 1)
                        {
                            _returnPage = strUrl[0];
                            
                            string[] strHeatPumpType = strUrl[1].Split('=');
                            if (strHeatPumpType.Length > 1)
                            {
                                HttpContext.Session.SetString("HeatPump", strHeatPumpType[1]);

                                //Put the question key in a session to keep a track of the question page that it needs to return to
                                string[] strQuestionKey = strHeatPumpType[1].Split('&');
                                if(strQuestionKey.Length > 0)
                                {
                                    HttpContext.Session.SetString("QuestionKey", strQuestionKey[0]);
                                }                                
                            }
                        }
                    }

                    HttpContext.Session.SetString("ReturnPage", _returnPage);
                }
                return _returnPage;
            }
        }
        /// <summary>
        /// Create a new instance with the service and logger.
        /// </summary>
        public HSMPage(ILogger logger, IApplicationConfiguration configuration)
        {
            Logger = logger;
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Store the <paramref name="value"/> keyed with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to associate with the value</param>
        /// <param name="value">The value to store</param>
        protected void StoreString(string key, string value)
        {
            if (!(value is null))
                HttpContext.Session.SetString(key, value);
            else
                ClearValue(key);
        }

        /// <summary>
        /// Removes the value for <paramref name="key"/> from the session.
        /// </summary>
        /// <param name="key">Identifies the value to remove</param>
        protected void ClearValue(string key)
        {
            HttpContext.Session.Remove(key);
        }

        /// <summary>
        /// Get a previously stored value identified by <paramref name="key"/>
        /// </summary>
        /// <param name="key">The key to lookup the value with</param>
        /// <returns>The stored value or </returns>
        public string GetStoredString(string key)
        {
            string value = HttpContext.Session.GetString(key);

            value = value?.Replace("m-¦", "m²");

            return value;
        }

        public void SetQuestionHidden(string key, bool isHidden, bool isAutoAnswered)
        {
            var quest = Configuration?.Questions[key];
            quest.IsHidden = isHidden;
            quest.IsAutoAnswered = isAutoAnswered;
        }

        /// <summary>
        ///  If it's a return page for some other page, remove the session variable.
        /// </summary>
        /// <param name="requestPath"></param>
        public void RemoveRedirectPageSession(string requestPath)
        {
            if (HttpContext.Session.GetString("ReturnPage") != null
                && HttpContext.Session.GetString("ReturnPage").Equals(requestPath))
            {
                HttpContext.Session.Remove("ReturnPage");
                HttpContext.Session.Remove("HeatPump");
            }
        }

        /// <summary>
        /// Attempt to pull the question key out of the query string.
        /// </summary>
        /// <returns>The value for the "q" query string or the empty string.</returns>
        protected string KeyFromQuery()
        {
            return QueryFromQuery("q");
        }

        /// <summary>
        /// Attempt to pull a value from the query string.
        /// </summary>
        /// <returns>The value for the "q" query string or the empty string.</returns>
        protected string QueryFromQuery(string key)
        {
            if (Request.Query.TryGetValue(key, out StringValues values) && values.Count == 1)
                return values[0];

            return String.Empty;
        }

        /// <summary>
        /// Will find the next question to be asked paying attention to the 
        /// <see cref="SkipAnsweredQuestions"/> property when encountering already answered
        /// questions.
        /// </summary>
        /// <param name="nextPage">Where to go when all questions have been answered.</param>
        /// <returns>A <see cref="RedirectToPageResult"/> to navigate to the next page</returns>
        protected IActionResult NextQuestion(string nextPage = "Summary")
        {
            IQuestionCollection questions = Configuration.Questions;

            if (questions.Count == 0)
            {
                Logger.Error($"There are no questions");
                return RedirectToPage("Index");
            }
            int idx;
            string key = KeyFromQuery();

            if (String.IsNullOrEmpty(key))
            {
                idx = 0;
            }
            else
            {
                Question currentQuestion = questions[key];
                idx = questions.IndexOf(currentQuestion);

                // Step back to the first or first answered question.
                while (!IsAnswered(currentQuestion) && idx > 0)
                    currentQuestion = questions[--idx];

                if (IsAnswered(currentQuestion))
                    ++idx;
            }
            if (SkipAnsweredQuestions || nextPage.Equals("Summary"))
            {
                while (idx < questions.Count)
                {
                    if (!IsAnswered(questions[idx]))
                        break;
                    ++idx;
                }
            }
            return idx < questions.Count
                ? RedirectToPage("Question", new { q = questions[idx].Key })
                : RedirectToPage(nextPage);
        }

        /// <summary>
        /// Test if the question has an answer stored in the state.
        /// </summary>
        /// <param name="question">The question to test.</param>
        /// <returns>True if the question has a stored answer otherwise false.</returns>
        private bool IsAnswered(Question question)
        {
            return !String.IsNullOrEmpty(GetStoredString(question.Key));
        }

        /// <summary>
        /// Tests to see if the current page is acceptable in the sense that it is not
        /// ahead of any unanswered questions by more than one step.
        /// </summary>
        /// <remarks>Will return false if called when on pages which come before the
        /// questions.
        /// Does not check for existing gaps in the answered questions, i.e. if the question
        /// at index n has been answered then it is assumed all questions from 0 to n have
        /// been answered.
        /// </remarks>
        /// <returns>True if the current page contains an answered question or is only one
        /// step ahead of an answered question.</returns>
        protected bool OnAcceptablePage()
        {
            string key = KeyFromQuery();
            IQuestionCollection questions = Configuration.Questions;

            int idx = questions.FirstOrDefault(q => q.Key == key) == null
                ? questions.Count
                : questions.IndexOf(questions[key]);

            while (idx > 0)
            {
                if (!IsAnswered(questions[--idx]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Remove all the stored answers and values which have been derived from the energy
        /// performance certificate.
        /// </summary>
        protected void ClearAnswers()
        {
            foreach ((string key, _) in STORED_EPC_VALUES)
            {
                ClearValue(key);
            }
            if (Configuration.Questions is IQuestionCollection questions)
            {
                foreach (Question question in questions)
                {
                    question.IsHidden = false;
                    ClearValue(question.Key);
                }
            }
        }

        /// <summary>
        /// Get a value from an energy performance certificate checking for non-value values.
        /// </summary>
        /// <param name="key">The lookup key</param>
        /// <param name="cert">The dictionary containing the values.</param>
        /// <returns>The extracted value or null</returns>
        protected static string GetEPCValue(string key, IDictionary<string, string> cert)
        {
            if (cert != null && cert.TryGetValue(key, out string value))
            {
                if (String.IsNullOrWhiteSpace(value)
                    || value.StartsWith("INVALID!", StringComparison.OrdinalIgnoreCase)
                    || value.StartsWith("NO DATA", StringComparison.OrdinalIgnoreCase)
                    || value.StartsWith("N/A", StringComparison.OrdinalIgnoreCase))
                {
                    value = null;
                }
            }
            else
            {
                value = null;
            }
            return value;
        }

        /// <summary>
        /// Read the skip answered questions flag from the state.
        /// </summary>
        protected bool SkipAnsweredQuestions
        {
            get
            {
                return GetStoredString(SKIP_ANSWERED_QUESTIONS_KEY) == "Y";
            }
        }

        /// <summary>
        /// Set the flag to skip answered questions.
        /// </summary>
        /// <param name="enable">If true sets the flag to "Y" otherwise "N"</param>
        protected void SetSkipAnsweredQuestions(bool enable)
        {
            StoreString(SKIP_ANSWERED_QUESTIONS_KEY, enable ? "Y" : "N");
        }

        /// <summary>
        /// Log the exception and return some text to report via the user interface.
        /// </summary>
        /// <param name="ex">The exception to record.</param>
        /// <returns></returns>
        protected string LogException(Exception ex)
        {
            Logger.Exception(ex);
#if DEBUG
            return ex != null
                ? $"Computer says: {ex.Message} ({ex.GetType().Name})"
                : "Really?!";
#else
			return "An internal error was encountered";
#endif
        }
    }
}
