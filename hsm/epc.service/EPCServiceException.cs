using System;
using System.Net;
using System.Runtime.Serialization;

namespace OCC.HSM.EPC
{
	/// <summary>
	/// This exception may be thrown from the <see cref="EPCService"/>
	/// </summary>
	public class EPCServiceException : Exception
	{
		/// <summary>
		/// The status code.
		/// </summary>
		public HttpStatusCode Status { get; }

		/// <inheritdoc/>
		public EPCServiceException() {}

		/// <summary>
		/// Record a status code for when a HTTP response error is detected.
		/// </summary>
		/// <param name="status">The status code</param>
		public EPCServiceException(HttpStatusCode status)
		{
			Status = status;
		}

		/// <inheritdoc/>
		public EPCServiceException(string message) : base(message)
		{
		}

		/// <summary>
		/// A custom constructor which records the <paramref name="status"/> along with
		/// the message.
		/// </summary>
		/// <param name="status">A HTTP status code</param>
		/// <param name="message">The message text for the base class.</param>
		public EPCServiceException(HttpStatusCode status, string message) : base(message)
		{
			Status = status;
		}

		/// <inheritdoc/>
		public EPCServiceException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <inheritdoc/>
		protected EPCServiceException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
