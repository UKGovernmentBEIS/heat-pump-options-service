using System;

namespace OCC.HSM.Model.Interfaces
{
	/// <summary>
	/// Simple logging interface to the information / audit log file.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Write the exception information to the log file.
		/// </summary>
		/// <param name="ex">The exception to log.</param>
		void Exception(Exception ex);

		/// <summary>
		/// Write error text to the log file.
		/// </summary>
		/// <param name="str">The text to write.</param>
#pragma warning disable CA1716 // Identifiers should not match keywords
		void Error(string str);// Error is no keyword I can find so shut up
#pragma warning restore CA1716 // Identifiers should not match keywords

		/// <summary>
		/// Write warning text to the log file.
		/// </summary>
		/// <param name="str">The text to write.</param>
		void Warning(string str);

		/// <summary>
		/// Write informational text to the log file.
		/// </summary>
		/// <param name="str">The text to write.</param>
		void Information(string str);

		/// <summary>
		/// Write debug text to the log file.
		/// </summary>
		/// <param name="str">The text to write.</param>
		void Debug(string str);
	}
}
