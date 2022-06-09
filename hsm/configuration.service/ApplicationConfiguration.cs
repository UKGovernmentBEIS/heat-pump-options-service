using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using OCC.HSM.Model;
using OCC.HSM.Model.Interfaces;

namespace OCC.HSM.Services
{
	/// <summary>
	/// An implementation of the <see cref="IApplicationConfiguration"/> interface.
	/// </summary>
	public class ApplicationConfiguration : IApplicationConfiguration
	{
		/// <summary>
		/// The root directory of the application, used to resolve relative paths.
		/// </summary>
		private readonly string rootDirectory_;

		/// <summary>
		/// Where logs should be written, the "./" prefix is used to indicate the path
		/// should be relative to the application root directory which can be supplied
		/// by calling the <see cref="UpdateRootPath(string)"/> method.
		/// </summary>
		private readonly string logDirectory_;

		/// <summary>
		/// The collection of questions loaded from the XML file.
		/// </summary>
		private readonly IQuestionCollection questions_;

		/// <summary>
		/// Create a new instance of the configuration using the configured directory.
		/// </summary>
		/// <param name="rootDir">The root directory of the application.</param>
		/// <param name="logDir">Where to write log files</param>
		/// <param name="loggingLevel">The logging level, can be one of:
		/// 1.	Error
		/// 2.	Warning
		/// 3.	Information
		/// 4.	Debug
		/// </param>
		/// <param name="questionsXmlFile">Where the information for questions is to be
		/// read from.</param>
		public ApplicationConfiguration(string rootDir, string logDir, string loggingLevel,
			string questionsXmlFile)
		{
			if(String.IsNullOrWhiteSpace(rootDir))
				throw new ArgumentNullException(nameof(rootDir), "The application root directory must be specified");
			if(!Directory.Exists(rootDir))
				throw new ArgumentException($"{rootDir} must be a directory");

			rootDirectory_ = Path.GetFullPath(rootDir);

			logDirectory_ = ValueOrDefault(logDir, "./logs");
			if(logDirectory_.StartsWith("./", StringComparison.OrdinalIgnoreCase)) {
				logDirectory_ = Path.GetFullPath(Path.Combine(rootDirectory_, logDirectory_.Substring(2)));
			}
			questionsXmlFile = ValueOrDefault(questionsXmlFile, "./data");
			if(questionsXmlFile.StartsWith("./", StringComparison.OrdinalIgnoreCase)) {
				questionsXmlFile = Path.GetFullPath(Path.Combine(rootDirectory_, questionsXmlFile.Substring(2)));
			}

			LoggingLevel = loggingLevel switch
			{
				"Error" => LoggingLevel.Error,
				"Warning" => LoggingLevel.Warning,
				"Information" => LoggingLevel.Information,
				"Debug" => LoggingLevel.Debug,
				_ => LoggingLevel.Warning
			};
			questions_ = LoadQuestions(questionsXmlFile);
		}

		/// <summary>
		/// Trim a value and return, returns the default value if <paramref name="textValue"/>
		/// does not have a value.
		/// </summary>
		/// <param name="textValue">The value to return</param>
		/// <param name="defaultValue">Returned if <paramref name="textValue"/> has no value</param>
		/// <returns>The value</returns>
		private string ValueOrDefault(string textValue, string defaultValue)
		{
			return String.IsNullOrWhiteSpace(textValue)
				? defaultValue
				: textValue.Trim();
		}

		/// <summary>
		/// Implementation of the <see cref="IApplicationConfiguration"/> interface.
		/// </summary>
		public string LogDirectory => logDirectory_;

		/// <summary>
		/// Access the set of questions loaded from the configuration.
		/// </summary>
		public IQuestionCollection Questions => questions_;

		/// <summary>
		/// The root directory of the application.s
		/// </summary>
		public string RootDirectory => rootDirectory_;

		/// <summary>
		/// The enumerated value for the configured logging level.
		/// </summary>
		public LoggingLevel LoggingLevel { get; }

		/// <summary>
		/// Load the questions from the provided XML file, if the file or directory is not
		/// present the result is null, if there are validation errors an exception will
		/// be thrown.
		/// </summary>
		/// <param name="filename">The file to read, if not present no inputs will be loaded.</param>
		/// <returns>The inputs instance, will not return null.</returns>
		/// <exception cref="Exception">(various types) thrown if questions cannot be
		/// obtained meaning the application cannot be used (to ask questions).</exception>
		private static IQuestionCollection LoadQuestions(string filename)
		{
			using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			Exception? ex = null;

			var settings = new XmlReaderSettings {
				Schemas = LoadSchema("OCC.HSM.Services.Resources.hsm-inputs.xsd"),
				ValidationType = ValidationType.Schema,
				ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints,
			};
			settings.ValidationEventHandler += (sender, args) => ex ??= args.Exception;

			using var reader = XmlReader.Create(fs, settings);

			if(!(new XmlSerializer(typeof(InputsType)).Deserialize(reader) is InputsType res))
				throw new ApplicationException("There are no questions --- cannot continue");

			if(ex != null)
				throw ex;

			return res.BuildCollection();
		}

		/// <summary>
		/// Load a schema from a resource.to be used when validating an XML file.
		/// </summary>
		/// <param name="schemaResource">The path and resource name from which the schema
		/// is to be loaded from.</param>
		/// <returns>A new <see cref="XmlSchemaSet"/></returns>
		private static XmlSchemaSet LoadSchema(string schemaResource)
		{
			using var stream = Assembly
				.GetExecutingAssembly()
				.GetManifestResourceStream(schemaResource);
			using var schemaReader = XmlReader.Create(stream);

			var schemaSet = new XmlSchemaSet();
			schemaSet.Add(XmlSchema.Read(schemaReader, (s, e) => { }));

			return schemaSet;
		}
	}
}
