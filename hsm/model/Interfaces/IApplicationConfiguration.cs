namespace OCC.HSM.Model.Interfaces
{
	/// <summary>
	/// The application's current configuration.
	/// </summary>
	public interface IApplicationConfiguration
	{
		/// <summary>
		/// Where information is to be logged on the local system.
		/// </summary>
		string LogDirectory { get; }

		/// <summary>
		/// The root directory of the application to be used when resolving relative paths.
		/// </summary>
		string RootDirectory { get; }

		/// <summary>
		/// Access the set of <see cref="OCC.HSM.Model.Entities.Question"/> to be asked.
		/// </summary>
		IQuestionCollection Questions { get; }

		/// <summary>
		/// The configured logging level.
		/// </summary>
	    LoggingLevel LoggingLevel { get; }
	}
}
