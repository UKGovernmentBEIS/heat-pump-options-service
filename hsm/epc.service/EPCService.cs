using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OCC.HSM.Model.Interfaces;

namespace OCC.HSM.EPC
{
	/// <summary>
	/// Access the Energy Performance Certificate information for postcodes in England.
	/// </summary>
	public sealed class EPCService : IEPCService
	{
		/// <summary>
		/// The maximum number of results a query can return.
		/// </summary>
		public const int MAX_RESULTS = 100;

		/// <summary>
		/// The URL of the EPC service.
		/// </summary>
		private readonly Uri serviceUri_;

		/// <summary>
		/// The encoded token derived from the email and key string provided to the constructor.
		/// </summary>
		private readonly string serviceToken_;

		/// <summary>
		/// Construct a new instance which uses the service identified by <paramref name="serviceUri"/>
		/// </summary>
		/// <param name="serviceUri">The EPC service endpoint address, must be a valid
		/// https address.</param>
		/// <param name="apiEmail">The email address used when the account was registered
		/// with energy performance certificate website.</param>
		/// <param name="apiKey">The key provided by the EPC</param>
		public EPCService(string serviceUri, string apiEmail, string apiKey)
		{
			if(string.IsNullOrWhiteSpace(apiEmail))
				throw new ArgumentNullException(nameof(apiEmail));

			if(!new EmailAddressAttribute().IsValid(apiEmail))
				throw new ArgumentException("not a valid email", nameof(apiEmail));

			if(string.IsNullOrWhiteSpace(apiKey))
				throw new ArgumentNullException(nameof(apiKey));

			if(string.IsNullOrEmpty(serviceUri))
				throw new ArgumentNullException(nameof(serviceUri));

			if(Uri.TryCreate(serviceUri, UriKind.Absolute, out Uri? res)) {
				if(res != null && res.Scheme == Uri.UriSchemeHttps) {
					serviceUri_ = res;
				} else {
					throw new ArgumentException(
						$"{nameof(serviceUri)} must be a https address");
				}
				serviceUri_ = res;
			} else {
				throw new ArgumentException("Not a valid url", nameof(serviceUri));
			}
			serviceToken_ = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiEmail}:{apiKey}"));
		}

		/// <summary>
		/// Construct a new instance which uses the service identified by <paramref name="serviceUrl"/>
		/// </summary>
		/// <param name="serviceUri">The EPC service endpoint address, must be built from
		/// a valid https address.</param>
		/// <param name="apiEmail">The email address used when the account was registered
		/// with energy performance certificate website.</param>
		/// <param name="apiKey">The key provided by the EPC</param>
		public EPCService(Uri serviceUri, string apiEmail, string apiKey)
		{
			if(serviceUri == null)
				throw new ArgumentNullException(nameof(serviceUri));

			if(string.IsNullOrWhiteSpace(apiEmail))
				throw new ArgumentNullException(nameof(apiEmail));

			if(string.IsNullOrWhiteSpace(apiKey))
				throw new ArgumentNullException(apiKey);

			if(serviceUri.Scheme == Uri.UriSchemeHttps) {
				serviceUri_ = serviceUri;
			} else {
				throw new ArgumentException(
					$"{nameof(serviceUri)} needs to be a https address");
			}
			serviceToken_ = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiEmail.Trim()}:{apiKey.Trim()}"));
		}

		/// <summary>
		/// Request addressed which have EPC for the given postcode.
		/// </summary>
		/// <param name="postcode">The postcode to search with.</param>
		/// <returns>The addresses which may be empty</returns>
		/// <exception cref="EPCServiceException">May be raised if the service is mis-configured
		/// or the not available.</exception>
		public async Task<IList<string>> AddressesFromPostcode(string postcode)
		{
			var response = await QueryEPCAsync(("postcode", postcode)).ConfigureAwait(true);

			if(response.StatusCode == HttpStatusCode.OK) {
				var str = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

				try {
					return ReadAddresses(JsonConvert.DeserializeObject(str));
				} catch(JsonReaderException ex) {
					throw new EPCServiceException("Invalid response", ex);
				}
			} else {
				throw new EPCServiceException(response.StatusCode, response.ReasonPhrase);
			}
		}

		/// <summary>
		/// Retrieve the latest certificate for the <paramref name="address"/> if available
		/// </summary>
		/// <param name="address">The address to use int he query.</param>
		/// <param name="postcode">The postcode for the <paramref name="address"/></param>
		/// <returns>The certificate for the address or null if not available.</returns>
		/// <exception cref="EPCServiceException">thrown if the request fails or the 
		/// content returned cannot be parsed.</exception>
		public async Task<IDictionary<string, string>?> CertificateFromAddress(
			string address, string postcode)
		{
			var response = await QueryEPCAsync(
				("address", address), ("postcode", postcode)
				).ConfigureAwait(true);

			if(response.StatusCode == HttpStatusCode.OK) {
				var str = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

				try {
					if(TryGetLatestCertificate(
							JsonConvert.DeserializeObject(str), out JObject? row) && row != null) {
						return row.ToObject<Dictionary<string, string>>();
					}
					return null;
				} catch(JsonReaderException ex) {
					throw new EPCServiceException("Invalid response", ex);
				}
			} else {
				throw new EPCServiceException(response.StatusCode, response.ReasonPhrase);
			}
		}

		/// <summary>
		/// Send a query to the EPC service with the specified query parameters.
		/// </summary>
		/// <param name="queries">A list of queries to add to the url</param>
		/// <returns>The response</returns>
		private async Task<HttpResponseMessage> QueryEPCAsync(
			params (string key, string value)[] queries)
		{
			using var client = new HttpClient();

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization
				= new AuthenticationHeaderValue("Basic", serviceToken_);

			var ub = new UriBuilder(serviceUri_);
			var query = HttpUtility.ParseQueryString(ub.Query);

			query.Add("size", MAX_RESULTS.ToString(CultureInfo.InvariantCulture));

			foreach(var (key, value) in queries)
				query.Add(key, value);

			ub.Query = query.ToString();

			return await client.GetAsync(ub.Uri).ConfigureAwait(true);
		}

		/// <summary>
		/// Read the address values from the rows in <paramref name="obj"/>.  The object
		/// is assumed to have been returned by a call to NewtonSoft's DeserializeObject(),
		/// looks for an object containing an array of which contain an address string
		/// value.
		/// </summary>
		/// <param name="obj">The result of deserialising the JSON returned from the EPC
		/// service when queried with a postcode.</param>
		/// <returns>A list of addresses.</returns>
		private static IList<string> ReadAddresses(object? obj)
		{
			var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			if(obj is JObject job) {
				if(job.TryGetValue("rows", out JToken? jtoken) && jtoken is JArray rows) {
					foreach(JToken row in rows) {
						if(TryGetString("address", row, out string? value)
							&& !String.IsNullOrWhiteSpace(value)) {
							set.Add(value);
						}
					}
				}
			}
			return set.ToList();
		}

		/// <summary>
		/// Given the deserialised result of querying the EPC for an address pick the
		/// row with the latest lodgement time.
		/// </summary>
		/// <param name="obj">The result returned from
		/// <see cref="JsonConvert.DeserializeObject(string)"/></param>
		/// <param name="latestCertificate"></param>
		/// <returns></returns>
		private static bool TryGetLatestCertificate(object? obj, out JObject? latestCertificate)
		{
			if(obj is JObject job) {
				if(job.TryGetValue("rows", out JToken? jtoken) && jtoken is JArray rows) {
					var rowIdx = -1;
					var latestDate = DateTime.MinValue;

					for(var idx = 0; idx < rows.Count; ++idx) {
						if(TryGetLodgementDatetime(rows[idx], out DateTime dt)) {
							if(dt > latestDate) {
								rowIdx = idx;
								latestDate = dt;
							}
						}
					}
					if(rowIdx >= 0 && rows[rowIdx] is JObject certRow) {
						latestCertificate = certRow;
						return true;
					}
				}
			}
			latestCertificate = null;
			return false;
		}

		/// <summary>
		/// Read the lodgement date value from the token as a <see cref="DateTime"/> value.
		/// </summary>
		/// <param name="token">The item to search</param>
		/// <param name="lodgementDate">The result when returning true</param>
		/// <returns>True if a valid date can be found otherwise false.</returns>
		private static bool TryGetLodgementDatetime(JToken token, out DateTime lodgementDate)
		{
			if(TryGetString("lodgement-datetime", token, out string? dateStr)) {
				if(DateTime.TryParse(dateStr, out DateTime dt)) {
					lodgementDate = dt;
					return true;
				}
			}
			lodgementDate = DateTime.MinValue;
			return false;
		}

		/// <summary>
		/// Attempt to retrieve a string value from the <paramref name="token"/> identified
		/// by <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key identifying the sought value</param>
		/// <param name="token">The token containing values.</param>
		/// <param name="value">Returns the value if found otherwise is null</param>
		/// <returns>True if the value has been found</returns>
		private static bool TryGetString(string key, JToken token, out string? value)
		{
			if(token is JObject r) {
				if(r.TryGetValue(key, out JToken? tok)) {
					if(tok != null && tok.Type == JTokenType.String) {
						value = tok.Value<string>().Trim();
						return true;
					}
				}
			}
			value = null;
			return false;
		}
	}
}
