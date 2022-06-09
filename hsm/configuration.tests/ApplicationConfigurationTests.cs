using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using OCC.HSM.Services;

using Xunit;

namespace OCC.HSM.Tests
{
	/// <summary>
	/// Test the <see cref="ApplicationConfiguration"/> service
	/// </summary>
	public class ApplicationConfigurationTests
	{
		/// <summary>
		/// Test loading the configuration and check relative paths are handled correctly.
		/// </summary>
		[Fact]
		public void TestLoad()
		{
			string logDir = Path.Combine(".", "logs");
			string questionsFile = Path.Combine("TestFiles", "questions.xml");

			var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var app = new ApplicationConfiguration(".", logDir, "Warning", questionsFile);

			Assert.Equal(logDir, app.LogDirectory);
			Assert.NotNull(app.Questions);

			Assert.Throws<FileNotFoundException>(() => new ApplicationConfiguration(
				".", "logs", "Warning", "data"));
		}

		/// <summary>
		/// Verify that given a valid XML file all questions get loaded.
		/// </summary>
		[Fact]
		public void TestLoadQuestions()
		{
			var app = new ApplicationConfiguration(".", Path.Combine(".", "logs"), "Warning", 
				Path.Combine("TestFiles", "questions.xml")) as IApplicationConfiguration;

			Assert.NotNull(app.Questions);
			CheckQuestion(app.Questions.HouseType);
			CheckQuestion(app.Questions.WallType);
			CheckQuestion(app.Questions.HouseAgeType);
			CheckQuestion(app.Questions.HouseSizeOption1Type);
			CheckQuestion(app.Questions.HouseSizeOption2Type);			
			CheckQuestion(app.Questions.RoofType);
			CheckQuestion(app.Questions.GlazingType);
			CheckQuestion(app.Questions.GasSupplyType);
			CheckQuestion(app.Questions.OutsideSpace);
			CheckQuestion(app.Questions.CurrentHeatingType);
		}

		/// <summary>
		/// Verify that if an XML file cannot be found no exception will be raised
		/// </summary>
		[Fact]
		public void TestNoQuestions()
		{
			Assert.Throws<DirectoryNotFoundException>(()
				=> new ApplicationConfiguration(".", Path.Combine(".", "logs"), "Warning",
					Path.Combine(".", "missing", "some.xml")));
		}

		/// <summary>
		/// Verify that an XML inputs file containing invalid information causes
		/// a validation error.
		/// </summary>
		[Fact]
		public void TestInvalidQuestions()
		{
			Assert.Throws<InvalidOperationException>(
				() => new ApplicationConfiguration(".", Path.Combine(".", "logs"), "Warning", 
				Path.Combine("TestFiles", "invalid-questions.xml")));
		}

		/// <summary>
		/// The question keys are based on the field name in the inputs they come from so
		/// should be unique, this test makes sure that stays true.
		/// </summary>
		[Fact]
		public void TestQuestionsHaveUniqueKeys()
		{
			var app = new ApplicationConfiguration(".", Path.Combine(".", "logs"), "Warning", 
				Path.Combine("TestFiles", "questions.xml")) as IApplicationConfiguration;

			var questions = app.Questions;
			Assert.NotNull(questions);

			var keys = new List<string> 
			{
				questions.HouseType.Key,
				questions.WallType.Key,
				questions.HouseAgeType.Key,
				questions.HouseSizeOption1Type.Key,
				questions.HouseSizeOption2Type.Key,				
				questions.RoofType.Key,
				questions.GlazingType.Key,
				questions.GasSupplyType.Key,
				questions.OutsideSpace.Key,
				questions.CurrentHeatingType.Key
			};

			Assert.Equal(keys.Count, keys.Distinct().Count());
		}

		/// <summary>
		/// Verify that the properties of the <paramref name="question"/> are not empty.
		/// </summary>
		/// <param name="question"></param>
		private static void CheckQuestion(Question question)
		{
			Assert.NotEmpty(question.Prompt);
			Assert.NotNull(question.AnswerChoices);
			Assert.NotEmpty(question.AnswerChoices);

			var akeys = new List<string>();

			foreach(var answer in question.AnswerChoices) {
				Assert.NotEmpty(answer.Key);
				Assert.NotEmpty(answer.Text);
				Assert.NotNull(answer.DbEncoding);

				akeys.Add(answer.Key);
			}
			Assert.Equal(akeys.Count, akeys.Distinct().Count());
		}

		/// <summary>
		/// Check that an invalid root directory gets rejected.
		/// </summary>
		[Fact]
		public void TestBadRootDirectory()
		{
			string logDir = Path.Combine(".", "logs");

			Assert.Throws<ArgumentNullException>(
				() => new ApplicationConfiguration(null, logDir, "Warning", "data"));
			Assert.Throws<ArgumentNullException>(
				() => new ApplicationConfiguration(String.Empty, logDir, "Warning", "data"));
			Assert.Throws<ArgumentNullException>(
				() => new ApplicationConfiguration(" \t ", logDir, "Warning", "data"));

			var bogus = Path.GetFullPath(
				Path.Combine(
					Path.Combine(
						Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
							".."),
						"Bogus",
					"hsm"),
				"root"));

			Assert.Throws<ArgumentException>(
				() => new ApplicationConfiguration(bogus, logDir, "Warning",
				"data"));

			var notepad = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Windows), "notepad.exe");
			Assert.Throws<ArgumentException>(
				() => new ApplicationConfiguration(notepad, logDir, "Warning", "data"));
		}
	}
}
