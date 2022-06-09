using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OCC.HSM.UI.Pages
{
    /// <summary>
    /// Display a summary of the energy performance certificate.
    /// </summary>
    public class EPCSummaryModel : HSMPage
	{
		private IConfiguration _configuration;
		/// <inheritdoc/>
		public EPCSummaryModel(IApplicationConfiguration configuration, IConfiguration iConfig, ILogger logger)	
			: base(logger, configuration)
		{
			_configuration = iConfig;
		}
		public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");

		/// <summary>
        /// Results page always needs to return to the Summary page.
        /// </summary>
        override public string ReturnPage => "PostCode";

		/// <summary>
		/// The energy performance rating as read from the state.
		/// </summary>
		public string Rating => GetStoredString(EPC_RATING_KEY);

		public string WallType => GetStoredString(WALLS_DESCRIPTION_KEY);

		public string RoofType => GetStoredString(ROOF_DESCRIPTION_KEY);

		public string WindowsType => GetStoredString(WINDOWS_DESCRIPTION_KEY);

		public string MainheatType => GetStoredString(MAINHEAT_DESCRIPTION_KEY);

		public string FloorArea => GetStoredString(FLOOR_AREA_KEY);

		public string PropertyType => GetStoredString(PROPERTY_TYPE_KEY);
			
		/// <summary>
		/// The stored value for date the rating was lodged the as derived from 
		/// the certificate.
		/// </summary>
		public string RatingDate
		{
			get
			{
				string str = GetStoredString(EPC_RATING_DATE_KEY);

				if (!string.IsNullOrEmpty(str))
				{
					if (DateTime.TryParse(str, out DateTime date))
					{
						return $"{date:MMMM yyyy}";
					}
				}
				return null;
			}
		}

		public async Task<IActionResult> OnGet()
        {
			bool isEpcPresent = Convert.ToBoolean(HttpContext.Session.GetString("IsEpcPresent"));
			if(!isEpcPresent)
            {
				return RedirectToPage("/Postcode");
            }

			RemoveRedirectPageSession(HttpContext.Request.Path.Value);

			//After clicking Continue Without EPC button, all the EPC answers will be cleared. If the user then clicks the browser's back button 
			//they shouldn't return to this page, because it won't contain any EPC data, for this reaseon redirecting to the Postcode page is more meaningfull
			if (string.IsNullOrEmpty(Rating) &&
				string.IsNullOrEmpty(RatingDate) &&
				string.IsNullOrEmpty(WallType) &&
				string.IsNullOrEmpty(RoofType) &&
				string.IsNullOrEmpty(WindowsType) &&
				string.IsNullOrEmpty(MainheatType) &&
				string.IsNullOrEmpty(FloorArea) &&
				string.IsNullOrEmpty(PropertyType))
            {
				return RedirectToPage("Postcode");
			}
			return Page();			
		}
		
		/// <summary>
		/// Called when the form is posted back by the Continue button click.  Sets the
		/// "skip 
		/// </summary>
		/// <returns>Redirects to the question page</returns>
		public IActionResult OnPost()
		{
			//After clicking Continue Without EPC button, all the EPC answers will be cleared. If the user then clicks the browser's back button 
			//they shouldn't return to this page, because it won't contain any EPC data, for this reaseon redirecting to the Postcode page is more meaningfull
			if (string.IsNullOrEmpty(Rating) &&
				string.IsNullOrEmpty(RatingDate) &&
				string.IsNullOrEmpty(WallType) &&
				string.IsNullOrEmpty(RoofType) &&
				string.IsNullOrEmpty(WindowsType) &&
				string.IsNullOrEmpty(MainheatType) &&
				string.IsNullOrEmpty(FloorArea) &&
				string.IsNullOrEmpty(PropertyType))
			{
				return RedirectToPage("Postcode");
			}

			RemoveRedirectPageSession(HttpContext.Request.Path.Value);
			return Page();
		}

		public IActionResult OnPostContinueWithEPC()
        {			
			HttpContext.Session.SetString("ContinueWithEpc", "true");
			return RedirectToPage("QuestionIntro");
		}

		public IActionResult OnPostContinueWithoutEPC()
        {
			RemoveRedirectPageSession(HttpContext.Request.Path.Value);

			HttpContext.Session.SetString("ContinueWithEpc", "false");
			HttpContext.Session.SetString("IsEpcPresent", "false");
			SetSkipAnsweredQuestions(false);
			ClearAnswers();
			return RedirectToPage("QuestionIntro");
		}
		
		/// <summary>
		/// Gets all the answered questions for displaying on the page.
		/// </summary>
		/// <returns>An enumerable of strings each prefixed with the breadcrumb text (to 
		/// keep the prompt short) followed by the answer text.</returns>
		public IEnumerable<string> GetAnswers()
		{
			foreach(Question question in Configuration.Questions) {
				string answerKey = GetStoredString(question.Key);

				if(!String.IsNullOrEmpty(answerKey)) {
					AnswerChoice answer = question.AnswerChoices.FirstOrDefault(ans => ans.Key == answerKey);

					if(answer != null)
						yield return $"{question.BreadcrumbText}: {answer.Text}";
				}
			}
		}
	}
}