using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using OCC.HSM.UI.Pages.Enums;
using static System.Net.WebUtility;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace OCC.HSM.UI.Pages
{
	/// <summary>
	/// The model behind the post code page.  This page takes the user through the process
	/// of entering a postcode, selecting one of the matching addresses for which a
	/// certificate is available or allowing the user to continue without information
	/// taken from a certificate.  If a certificate is found this page will pass on the
	/// information to the EPC summary page, if no certificate the site will navigate
	/// directly to the first question.
	/// </summary>	
	public class PostCodeModel : HSMPage
	{
		private IConfiguration _configuration;
		/// <summary>
		/// The connection to the energy performance certificate source.
		/// </summary>
		private IEPCService EPC { get; }

		/// <summary>
		/// A regular expression to validate postcodes before submitting to the EPC, this
		/// pattern seems adequate for addresses in England.
		/// </summary>
		private readonly Regex rePostcode = new Regex(
			@"^([A-Z]{1,2}\d[A-Z\d]?\s*\d[A-Z]{2}|GIR ?0A{2})$", RegexOptions.Compiled);

		/// <summary>
		/// Used to extract the leading digits from an address string.
		/// </summary>
		private static readonly Regex reDoorNumber_ = new Regex(@"^\d+", RegexOptions.Compiled);

		/// <summary>
		/// Bound to the postcode input field on the page.
		/// </summary>
		[BindProperty]
		public string PostcodeText { get; set; }

		/// <summary>
		/// Bound to the text selected in the drop-down list of addresses.
		/// </summary>
		[BindProperty]
		public string AddressText { get; set; }

		/// <summary>
		/// The list of addresses in <see cref="SelectList"/> format for the drop-down on
		/// the page to read from.
		/// </summary>
		public SelectList Addresses { get; private set; }

		/// <summary>
		/// This is set to give the user additional information for cases when the postcode
		/// is either invalid or there are no certificates available. When this is set the
		/// address list is not shown.
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		/// The EPC response code.
		/// </summary>
		public EPCResponse EPCResponseCode { get; set; } = EPCResponse.CertificateFound;

        /// <summary>
        /// Not all messages can be an error statement. This property is set to distinguish when the entered postcode is invalid or empty.
        /// </summary>
        public bool IsError { get; set; }

		/// <summary>
        /// PostCode is now the landing page
        /// </summary>
        override public string ReturnPage => "";

		public string ESCFeedbackFormUrl => _configuration.GetValue<string>("AppConfigSettings:ESCFeedbackFormUrl");

		/// <summary>
		/// Create a new instance with the service and logger.
		/// </summary>
		public PostCodeModel(IEPCService epcService, ILogger logger,
			IApplicationConfiguration configuration,
			IConfiguration iConfig) : base(logger, configuration)
		{
			EPC = epcService;
			_configuration = iConfig;
		}

		/// <summary>
		/// Clear the existing answers.
		/// </summary>
		public async Task<IActionResult> OnGet()
		{          
            RemoveRedirectPageSession("/Postcode");

			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("postcode")))
			{
				PostcodeText = HttpContext.Session.GetString("postcode");
				
				await LoadPostcodeAddresses();
			}

			ClearAnswers();			

			return Page();
		}

        /// <summary>
        /// The postcode should have been submitted, use the EPC service to look up using
        /// the postcode and populate the list of addresses.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostPostcode()
		{
			Message = string.Empty;

			if (string.IsNullOrEmpty(PostcodeText))
            {
				IsError = true;
				Message = Helpers.Extensions.GetDescription(EPCResponse.PostCodeIsNullOrWhiteSpace);
				return Page();
			}
			
			HttpContext.Session.SetString("postcode", PostcodeText);

			ClearAnswers();

			await LoadPostcodeAddresses();

            if (EPCResponseCode == EPCResponse.NoCertificateFound || EPCResponseCode == EPCResponse.EPCServiceCurrentlyNotAvailable)
            {
                return RedirectToPage("EPCNotFound");
            }
            			
			return RedirectToPage("Postcode");						
		}

		/// <summary>
		/// This method gets called when the Change button is clicked.
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnPostChangePostcode()
        {
			PostcodeText = string.Empty;
			HttpContext.Session.SetString("postcode", string.Empty);
			return RedirectToPage("Postcode");
		}

		/// <summary>
		/// Called when the user has submitted their address from the selection of addresses.
		/// If the user clicks the button without selecting an address simply go back, not
		/// attempting to add additional prompts to select an address.
		/// </summary>
		/// <returns>A redirect to the next page, which may be this one.</returns>
		public async Task<IActionResult> OnPostSelectAddress()
		{
			PostcodeText = HttpContext.Session.GetString("postcode");
			HttpContext.Session.Remove("ContinueWithEpc");
			try {
				if(string.IsNullOrEmpty(AddressText) || AddressText.Equals("found")) {
					IsError = true;
					Message = Helpers.Extensions.GetDescription(EPCResponse.AddressIsNullOrWhiteSpace); 
					await TryGetAddressList().ConfigureAwait(true);
				} else {
					IDictionary<string, string> cert
						= await EPC.CertificateFromAddress(AddressText, PostcodeText).ConfigureAwait(true);

					if(cert == null)
                    {
						HttpContext.Session.SetString("IsEpcPresent", "false");
						return RedirectToPage("EPCNotFound");
					}

					HttpContext.Session.SetString("IsEpcPresent", "true");

					int answeredQuestions = AnswersFromCertificate(cert);

					foreach((string key, string epcKey) in STORED_EPC_VALUES) {
						StoreString(key, GetEPCValue(epcKey, cert));
					}

					return RedirectToPage(answeredQuestions == 0
						? "Question"
						: "EPCSummary");
				}
			} catch(Exception ex) {
				Message = LogException(ex);
			}
			
			HttpContext.Session.SetString("IsEpcPresent", "false");
			return Page();			
		}

		/// <summary>
		/// Gets called when the user clicks the "Continue without postcode" link.
		/// </summary>
		/// <returns>Redirects to the first question page</returns>
		public IActionResult OnPostWithoutEPCData()
		{
			HttpContext.Session.SetString("ContinueWithEpc", "false");
			HttpContext.Session.SetString("IsEpcPresent", "false");
			ClearValue(POSTCODE_KEY);
			ClearAnswers();
			return RedirectToPage("QuestionIntro");			
		}

		/// <summary>
		/// Attempt to find the answers to questions from the energy performance certificate
		/// using the mappings built into the answer choices.
		/// </summary>
		/// <param name="certificate">The certificate in the form of a dictionary mapping
		/// key => value</param>
		/// <returns>The number of answers obtained from the certificate</returns>
		private int AnswersFromCertificate(IDictionary<string, string> certificate)
		{
			int answeredQuestions = 0;

			if(certificate != null && Configuration.Questions is IQuestionCollection questions) {
				foreach(Question question in questions) {
					foreach(var answer in question.AnswerChoices) {
						if(answer.Matches(certificate) && !question.Key.Equals("housesizeoption2")) {
							StoreString(question.Key, answer.Key);
							++answeredQuestions;

							//Skip question according to the result
							//If housesizeoption1 has been matched from the EPC, then skip the Number of bedrooms question(mark it as answered, but don't display it in the Summary page as it wasn't answered by the user)
							if (question.Key.Equals("housesizeoption1"))
							{
								StoreString("housesizeoption2", "housesizeoption2-1");							

								SetQuestionHidden("housesizeoption2", true, true);
								++answeredQuestions;
							}
							break;
						}
					}
				}
			}
			return answeredQuestions;
		}

		/// <summary>
		/// A helper method to call the EPC service and populate the <see cref="Addresses"/>
		/// member and also the <see cref="Message"/> member when appropriate.
		/// </summary>
		private async Task TryGetAddressList()
		{
			if(!string.IsNullOrWhiteSpace(PostcodeText)) {
				string postcode = PostcodeText.Trim().ToUpperInvariant();
				if(rePostcode.Match(postcode).Success) {
					PostcodeText = postcode;
					Addresses = null;

					try {
						Task setCountry = SetCountryFromPostcode(PostcodeText);
						Task<IList<string>> getAddresses = EPC.AddressesFromPostcode(PostcodeText);

						await Task.WhenAll(setCountry, getAddresses).ConfigureAwait(true);

						var addrList = getAddresses.Result;

						if(addrList.Count > 0) {
							List<string> lst = addrList.ToList();
							lst.Sort((a, b) => GetDoorNumber(a) - GetDoorNumber(b));

							Addresses = new SelectList(lst);
						} else {

							EPCResponseCode = EPCResponse.NoCertificateFound;
							Message = Helpers.Extensions.GetDescription(EPCResponseCode);
						}
					} catch(Exception ex) {
						Logger.Exception(ex);
						EPCResponseCode = EPCResponse.EPCServiceCurrentlyNotAvailable;
						Message = Helpers.Extensions.GetDescription(EPCResponseCode);							
					}
				} else {
					IsError = true;
					EPCResponseCode = EPCResponse.InvalidPostCode;
					Message = Helpers.Extensions.GetDescription(EPCResponseCode);
				}
			} else {
				IsError = true;
				EPCResponseCode = EPCResponse.PostCodeIsNullOrWhiteSpace;
				Message = Helpers.Extensions.GetDescription(EPCResponseCode);
			}
		}

		/// <summary>
		/// Extracts the numerical integer value at the beginning of the address and returns
		/// it as an integer.  If not value can be found returns int.Max to put those 
		/// addresses at the bottom of the list.  No attempt to support sorting of numbers
		/// with letters (8a, 8b etc.) is made.
		/// </summary>
		/// <param name="addr">The address to extract the door number from</param>
		/// <returns>An integer value</returns>
		private static int GetDoorNumber(string addr)
		{
			Match m = reDoorNumber_.Match(addr);

			return m.Success && m.Groups.Count > 0
				? int.Parse(m.Groups[0].Value, NumberStyles.Integer, CultureInfo.InvariantCulture)
				: int.MaxValue;
		}

		/// <summary>
		/// Given a postcode try to find which country it belongs to and, if found, add it
		/// as an answer to the location question.
		/// </summary>
		/// <param name="postcodeText">The postcode to search with.</param>
		/// <remarks>
		/// Uses api.getthedata.com under an Open Government License - see
		/// http://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/
		/// 
		/// The structure of the returned JSON is:
		/// <code>
		/// {
		///   "status": "match",
		///   "match_type": "unit_postcode",
		///   "input": "G11GY",
		///   "data": {
		///     "postcode": "G1 1GY",
		///     ...
		///     "country": "Scotland",
		///     ...
		///   },
		///   "copyright": [
		///     ...
		///   ]
		/// }
		/// 
		/// </code>
		/// Due to licensing restrictions addresses in Northern Ireland are not supported,
		/// an alternative would be to try another service such as https://postcodes.io/
		/// but this has not been investigated.
		/// </remarks>
		private async Task SetCountryFromPostcode(string postcodeText)
		{
			if(!(Configuration.Questions is IQuestionCollection questions))
				return;

			if(!(questions["houselocation"] is Question question))
				return;

			try {
				using var client = new HttpClient();

				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue("application/json"));

				var url = new Uri($"http://api.getthedata.com/postcode/{UrlEncode(postcodeText)}");

				HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(true);
				if(response.StatusCode == HttpStatusCode.OK) {
					var str = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

					if(JsonConvert.DeserializeObject(str) is JObject json
						&& json.SelectToken("data")
							?.SelectToken("country")
							?.Value<String>() is string country) {

						var answer = question.AnswerChoices.FirstOrDefault(ans => ans.Text == country);

						if(!(answer is null))
							StoreString(question.Key, answer.Key);
					}
				}
			} catch(Exception ex) {
				Logger.Exception(ex);
			}
		}

		private async Task LoadPostcodeAddresses()
		{
			try
			{
				await TryGetAddressList().ConfigureAwait(true);
			}
			catch (Exception ex)
			{
				Message = LogException(ex);
			}
		}
	}
}