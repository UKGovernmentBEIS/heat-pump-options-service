using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace OCC.HSM.UI.Pages
{
	/// <summary>
	/// The model for the summary page, supports all configured questions.
	/// Anti Forgery token is ignored in this page as it has only a single post method, which we want to be able to be called from Simple Energy Advise website.
	/// </summary>
	[IgnoreAntiforgeryToken]
	public class SummaryModel : HSMPage
	{
		private IConfiguration _configuration;
		/// <inheritdoc/>
		public SummaryModel(ILogger logger, IApplicationConfiguration configuration, IConfiguration iConfig)
			: base(logger, configuration)
		{
			if(Configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			Questions = configuration.Questions;
			_configuration = iConfig;
		}
		public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");

        public string ReturnQuestion { get; set; }

		/// <summary>
		/// Summary page always needs to return to the last question.
		/// </summary>
        override public string ReturnPage
		{
			get
			{
				Question lastQuestion = Configuration?.Questions?[Configuration.Questions.Count - 1];
				ReturnQuestion = lastQuestion?.Key;
				return $"Question";
			}
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

		/// <summary>
		/// Receive all the answers.
		/// </summary>
		/// <returns>The first question needing an answer or the current page.</returns>
		public IActionResult OnPost()
		{
			//Answer the questions with the incoming answers
			Questions?.ToList().ForEach(x =>
			{
				if (!string.IsNullOrEmpty(Request.Form[x.Key]))
					StoreString(x.Key, Request.Form[x.Key]);
			});

			return OnAcceptablePage() ? Page() : NextQuestion();
		}

		/// <summary>
		/// The collection of all questions
		/// </summary>
		public IList<Question> Questions { get; private set; }
	}
}