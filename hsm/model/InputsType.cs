using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OCC.HSM.Model
{
    /// <summary>
    /// The main content of this class is auto-generated, this part of the class makes the
    /// contents easier to use.
    /// </summary>
    public partial class InputsType
	{
		/// <summary>
		/// Flags used to read a single property value by reflection.
		/// </summary>
		private const BindingFlags GET_PROP_FLAGS =
			BindingFlags.Public
			| BindingFlags.Instance
			| BindingFlags.GetProperty;

		/// <summary>
		/// Construct a new instance by 
		/// </summary>
		public IQuestionCollection BuildCollection()
		{
			var questionList = new List<Question>();

			AppendQuestion(nameof(housetypeField)[0..^5], housetypeField, questionList);				
			AppendQuestion(nameof(houseageField)[0..^5], houseageField, questionList);
			//Please keep the Wall Type question after the House Age one as the WallType property calculation will HouseAge's value
			AppendQuestion(nameof(walltypeField)[0..^5], walltypeField, questionList);
			AppendQuestion(nameof(housesizeoption1Field)[0..^5], housesizeoption1Field, questionList);
			AppendQuestion(nameof(housesizeoption2Field)[0..^5], housesizeoption2Field, questionList);
			AppendQuestion(nameof(rooftypeField)[0..^5], rooftypeField, questionList);
			AppendQuestion(nameof(glazingField)[0..^5], glazingField, questionList);			
			AppendQuestion(nameof(outsidespaceField)[0..^5], outsidespaceField, questionList);
			AppendQuestion(nameof(currentheatingsystemField)[0..^5], currentheatingsystemField, questionList);
			AppendQuestion(nameof(gassupplyField)[0..^5], gassupplyField, questionList);

			return new QuestionCollection(questionList);
		}
		/// <summary>
		/// Read the properties from the <paramref name="obj"/> to create a new 
		/// <see cref="Question"/> instance and add it to the <paramref name="lst"/>.  If
		/// <paramref name="obj"/> is null then nothing is added to the list.
		/// </summary>
		/// <param name="questionKey">The key for this question.</param>
		/// <param name="obj">The object to read properties from.</param>
		/// <param name="lst">The list to add the new question to.</param>
		private static void AppendQuestion(string questionKey, object obj, IList<Question> lst)
		{
			if(obj == null)
				return;

			lst.Add(new Question(
				key: questionKey,
				text: GetStringProperty(obj, "question"),
				breadcrumbText: GetStringProperty(obj, "bctext"),
				explanation: GetStringProperty(obj, "explanation"),
				answers: BuildAnswerChoices(obj, questionKey, "answer")));
		}

		/// <summary>
		/// Use reflection to get the named string property value, if the property does not
		/// exist or the value is null will return the empty string.
		/// </summary>
		/// <param name="obj">The instance to read or attempt to read the value from.</param>
		/// <param name="propName">The name of the property to read.</param>
		/// <returns>The string value or the empty string.</returns>
		private static string GetStringProperty(object obj, string propName)
		{
			var prop = obj.GetType().GetProperty(propName, GET_PROP_FLAGS);

			if(prop != null)
				return (prop.GetValue(obj) as string) ?? String.Empty;

			return String.Empty;
		}

		/// <summary>
		/// Get the enum property value from the object.
		/// </summary>
		/// <typeparam name="T">The enumeration type.</typeparam>
		/// <param name="obj">The instance to read or attempt to read the value from.</param>
		/// <param name="propName">The name of the property to read.</param>
		/// <returns>The enum value or 0 cast to the enum</returns>
		private static T GetEnumProperty<T>(object obj, string propName) where T : Enum
		{
			var prop = obj.GetType().GetProperty(propName, GET_PROP_FLAGS);

			if(prop != null)
			{
				var value = prop.GetValue(obj);

				if(value != null)
					return (T)value;
			}
			return (T)Enum.ToObject(typeof(T), 0);
		}

		/// <summary>
		/// Read the array of permitted answers for the question and build an array of
		/// <see cref="AnswerChoice"/> values.
		/// </summary>
		/// <param name="obj">The field containing the list of (enum) values for the answer
		/// choices.</param>
		/// <param name="questionKey">The unique key for the related question to be use as
		/// part of the key for the answer choices.</param>
		/// <param name="propName">The name of the array property containing the 
		/// enumerated values.</param>
		/// <returns>An array of <see cref="AnswerChoice"/> values</returns>
		private static AnswerChoice[] BuildAnswerChoices(object obj, string questionKey,
			string propName)
		{
			var answers = new List<AnswerChoice>();
			var prop = obj.GetType().GetProperty(propName, GET_PROP_FLAGS);

			if(prop != null)
			{
				var propValue = prop.GetValue(obj);
				if(propValue.GetType().IsArray)
				{
					foreach(AnswerType choice in ((IEnumerable)propValue).Cast<AnswerType>())
					{
						answers.Add(new AnswerChoice($"{questionKey}-{choice.key}",
							choice.usertext, choice.description, choice.dbencoding, choice.certificatematch?.Select(cert
								=> new EPCMatch(cert.epckey, cert.matchtype, cert.epctext))));
					}
				}
			}
			return answers.ToArray();
		}
	}
}
