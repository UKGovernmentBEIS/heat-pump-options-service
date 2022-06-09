using System;
using System.Text;
using OCC.HSM.Model;
using OCC.HSM.Model.Interfaces;

using Serilog;
using Serilog.Events;

namespace OCC.HSM.Services
{
	/// <summary>
	/// An implementation of <see cref="Model.Interfaces.ILogger"/> which uses Serilog
	/// to write to a rolling log file.
	/// </summary>
	public class Logger : Model.Interfaces.ILogger
	{
		/// <summary>
		/// The <see cref="Serilog.ILogger"/> implementation being used.
		/// </summary>
		private readonly Serilog.ILogger seriLogger_;

		/// <summary>
		/// Construct a new logger using the 
		/// </summary>
		/// <param name="config"></param>
		public Logger(IApplicationConfiguration config)
		{
			var loggerConfiguration = new LoggerConfiguration()
				.WriteTo.RollingFile($"{config.LogDirectory}/log.log");

			switch(config.LoggingLevel) {
				case LoggingLevel.Error:
					loggerConfiguration.MinimumLevel.Error();
					break;
				case LoggingLevel.Warning:
					loggerConfiguration.MinimumLevel.Warning();
					break;
				case LoggingLevel.Information:
					loggerConfiguration.MinimumLevel.Information();
					break;
				case LoggingLevel.Debug:
					loggerConfiguration.MinimumLevel.Debug();
					break;
				default:
					loggerConfiguration.MinimumLevel.Warning();
					break;
			}
			seriLogger_ = loggerConfiguration.CreateLogger();
		}

		/// <summary>
		/// Write the exception information to the log file.
		/// </summary>
		/// <param name="ex">The exception to log</param>
		public void Exception(Exception ex)
		{
			try {
				Exception e = ex;
				var sb = new StringBuilder("EXCEPTION: ");

				while(e != null) {
					var st = new System.Diagnostics.StackTrace(e, true);

					sb.AppendFormat("{1} {0}: ",
						e.GetType().Name, Environment.NewLine);
					sb.AppendFormat("{0} in {1}(), {3} Stack Trace:{3}{2}{3}",
						e.Message, st.GetFrames()[0].GetMethod().Name,
						e.StackTrace,
						Environment.NewLine);
					e = e.InnerException;
				}
				Error(sb.ToString());
			} catch { }
		}

		/// <summary>
		/// Write warning text to a log file.
		/// </summary>
		/// <param name="message">The text to write</param>
		public void Error(string message)
		{
			seriLogger_.Write(LogEventLevel.Error, message);
		}

		/// <summary>
		/// Write warning text to a log file.
		/// </summary>
		/// <param name="message">The text to write</param>
		public void Warning(string message)
		{
			seriLogger_.Write(LogEventLevel.Warning, message);
		}

		/// <summary>
		/// Write informational text to a log file.
		/// </summary>
		/// <param name="message">The text to write</param>
		public void Information(string message)
		{
			seriLogger_.Write(LogEventLevel.Information, message);
		}

		/// <summary>
		/// Write informational text to a log file.
		/// </summary>
		/// <param name="message">The text to write</param>
		public void Debug(string message)
		{
			seriLogger_.Write(LogEventLevel.Debug, message);
		}
	}
}
