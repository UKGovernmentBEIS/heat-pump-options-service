using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCC.HSM.Model.Interfaces
{
	/// <summary>
	/// Access the Energy Performance Certificate for english postcodes.
	/// </summary>
	public interface IEPCService
	{
		/// <summary>
		/// Search for addresses by <paramref name="postcode"/> for which certificates are
		/// available 
		/// </summary>
		/// <param name="postcode">Used to identify the addresses with EPC information</param>
		/// <returns>A list of addresses, which may be empty, that can be used in the
		/// <see cref="CertificateFromAddress(string)"/> method.</returns>
		Task<IList<string>> AddressesFromPostcode(string postcode);

		/// <summary>
		/// Retrieve the latest certificate for the <paramref name="address"/> if available
		/// </summary>
		/// <param name="address">The address to use int he query.</param>
		/// <param name="postcode">THe postcode for the address</param>
		/// <returns>The certificate for the address or null if not available.</returns>
		Task<IDictionary<string, string>?> CertificateFromAddress(string address,
			string postcode);
	}
}