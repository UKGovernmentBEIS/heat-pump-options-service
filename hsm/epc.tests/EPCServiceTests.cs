using System;
using System.Net;
using System.Threading.Tasks;

using OCC.HSM.EPC;

using Xunit;

namespace OCC.HSM.Tests
{
	/// <summary>
	/// Tests for the <see cref="EPCService"/>
	/// </summary>
	public class EPCServiceTests
	{
		/// <summary>
		/// The url to the domestic EPC search.
		/// </summary>
		private const string EPC_URL = "https://epc.opendatacommunities.org/api/v1/domestic/search";

		/// <summary>
		/// The EPC account email.
		/// </summary>
		private const string ACCOUNT_EMAIL = "mike.hewlett@oxfordcc.co.uk";

		/// <summary>
		/// The EPC account key.
		/// </summary>
		private const string ACCOUNT_KEY = "0b9a41e07360ceda876aa83d8e94bd3e0d69e4ba";

		/// <summary>
		/// A valid postcode to use in tests.
		/// </summary>
		private const string VALID_POSTCODE = "OX1 2EP";

		/// <summary>
		/// A valid address to use in tests.
		/// </summary>
		private const string VALID_ADDRESS = "43, Hythe Bridge Street";

		/// <summary>
		/// Simple check that an instance is created without error when passed reasonable
		/// parameters.
		/// </summary>
		[Fact]
		public void TestCreate()
		{
			string email = "nobody@home.com";
			string key = "0fadeddecafbeeffacade";

			var svc = new EPCService(EPC_URL, email, key);

			Assert.NotNull(svc);
		}

		/// <summary>
		/// Check that missing or malformed urls get caught.
		/// parameters.
		/// </summary>
		[Fact]
		public void TestCreateBadUrl()
		{
			string url = null;
			string email = "nobody@home.com";
			string key = "0fadeddecafbeeffacade";

			Assert.Throws<ArgumentNullException>(()
				=> new EPCService(url, email, key));

			Assert.Throws<ArgumentNullException>(()
				=> new EPCService(string.Empty, email, key));

			Assert.Throws<ArgumentException>(()
				=> new EPCService("bogus://not.a.url", email, key));

			Assert.Throws<ArgumentException>(()
				=> new EPCService("http://google.com", email, key));

			Assert.NotNull(new EPCService("https://google.com", email, key));
		}

		/// <summary>
		/// Check that missing or malformed email text gets caught.
		/// parameters.
		/// </summary>
		[Fact]
		public void TestCreateBadEmail()
		{
			string key = "0fadeddecafbeeffacade";

			Assert.Throws<ArgumentNullException>(()
				=> new EPCService(EPC_URL, null, key));

			Assert.Throws<ArgumentNullException>(()
				=> new EPCService(EPC_URL, string.Empty, key));

			Assert.Throws<ArgumentException>(()
				=> new EPCService(EPC_URL, "some text", key));

			Assert.NotNull(new EPCService(EPC_URL, "someone@example.com", key));
		}

		/// <summary>
		/// Check that missing or malformed email text gets caught.
		/// parameters.
		/// </summary>
		[Fact]
		public void TestCreateBadKey()
		{
			string email = "someone@example.com";

			Assert.Throws<ArgumentNullException>(()
				=> new EPCService(EPC_URL, email, null));

			Assert.Throws<ArgumentNullException>(()
				=> new EPCService(EPC_URL, email, string.Empty));

			Assert.Throws<ArgumentNullException>(()
				=> new EPCService(EPC_URL, email, "  "));

			Assert.NotNull(new EPCService(EPC_URL, email, "0fadeddecafbeeffacade"));
		}

		/// <summary>
		/// Retrieve information for a valid postcode.
		/// parameters.
		/// </summary>
		[Fact]
		public async Task TestRetrieval()
		{
			var svc = new EPCService(EPC_URL, ACCOUNT_EMAIL, ACCOUNT_KEY);

			var addresses = await svc.AddressesFromPostcode(VALID_POSTCODE);

			Assert.NotNull(addresses);
			Assert.NotEmpty(addresses);

			var certificate = await svc.CertificateFromAddress(addresses[^1], VALID_POSTCODE);

			Assert.NotNull(certificate);
			Assert.True(certificate.ContainsKey("current-energy-rating"));
		}

		/// <summary>
		/// Getting the server wrong should fail even though the server may return a 200
		/// because it is, in itself, a valid address but the content returned will not be
		/// valid JSON.
		/// </summary>
		[Fact]
		public async Task TestWrongServer()
		{
			var svc = new EPCService("https://oxfordcc.co.uk/", ACCOUNT_EMAIL, ACCOUNT_KEY);

			var ex = await Assert.ThrowsAsync<EPCServiceException>(() =>
				svc.AddressesFromPostcode(VALID_POSTCODE));

			Assert.Equal((HttpStatusCode)0, ex.Status);
		}

		/// <summary>
		/// Getting the url path wrong should 404
		/// </summary>
		[Fact]
		public async Task TestWrongPath()
		{
			string url = "https://epc.opendatacommunities.org/api/v1/";

			var svc = new EPCService(url, ACCOUNT_EMAIL, ACCOUNT_KEY);

			var ex = await Assert.ThrowsAsync<EPCServiceException>(() =>
				svc.AddressesFromPostcode(VALID_POSTCODE));

			Assert.Equal(HttpStatusCode.NotFound, ex.Status);
		}

		/// <summary>
		/// Check that using an invalid key results in a 404.
		/// </summary>
		[Fact]
		public async Task TestWrongKey()
		{
			var svc = new EPCService(EPC_URL, ACCOUNT_EMAIL, "0b8a41f40579b1fa876aa41d2e34db6a9e01aadc");

			var ex = await Assert.ThrowsAsync<EPCServiceException>(() =>
				svc.AddressesFromPostcode(VALID_POSTCODE));

			Assert.Equal(HttpStatusCode.Unauthorized, ex.Status);
		}
	}
}
