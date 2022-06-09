using System;
using System.Collections.ObjectModel;

namespace OCC.HSM.Model.Entities
{
	/// <summary>
	/// Information for a single question presented to the user.
	/// </summary>
	public sealed class Question
	{
		/// <summary>
		/// An empty question to be used as a safe default.
		/// </summary>
		public static readonly Question Empty = new Question("empty", String.Empty,
			String.Empty, String.Empty, Array.Empty<AnswerChoice>());

		/// <summary>
		/// The array of answer texts read from the configuration.
		/// </summary>
		private readonly ReadOnlyCollection<AnswerChoice> answers_;

		/// <summary>
		/// Construct a new immutable instance.
		/// </summary>
		/// <param name="key">Unique key for the question.</param>
		/// <param name="text">The question text</param>
		/// <param name="breadcrumbText">Text for rendering in a breadcrumb component.</param>
		/// <param name="explanation">The optional explanation text for this question</param>
		/// <param name="answers">An array containing the choices to be presented as answers.</param>
		public Question(string key, string text, string breadcrumbText,
			string explanation, AnswerChoice[] answers)
		{
			if(String.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			Key = key;
			Prompt = text;
			BreadcrumbText = breadcrumbText;
			Explanation = explanation;
			answers_ = Array.AsReadOnly<AnswerChoice>(answers);
		}

		/// <summary>
		/// Define if the question needs to be asked to the user or not.
		/// </summary>
		public bool IsHidden { get; set; }

		/// <summary>
		/// Some of the questions can be skipped because of the previous question's answer.
		/// </summary>
		public bool IsAutoAnswered { get; set; }

		/// <summary>
		/// A unique key identifying the question.
		/// </summary>
		public string Key { get; }

		/// <summary>
		/// The text to be displayed with the question.
		/// </summary>
		public string Prompt { get; }
		
		/// <summary>
		/// Short text intended for displaying in a bread crumb component.
		/// </summary>
		public string BreadcrumbText { get; }

		/// <summary>
		/// The optional explanation text for this question.
		/// </summary>
		public string Explanation { get; }

		/// <summary>
		/// Indicates if there are separate images available to display alongside the answers.
		/// </summary>
		public bool HasChoiceImages { get; private set; }

		/// <summary>
		/// The single image path to display alongside the answers if there are no separate images for each choice.
		/// </summary>
		public string? SingleImage { get; private set; }

		/// <summary>
		/// A list of <see cref="AnswerChoice"/> values available for the questions.
		/// </summary>
		public ReadOnlyCollection<AnswerChoice> AnswerChoices => answers_;

		/// <summary>
		/// Set the <see cref="HasChoiceImages"/> flag.
		/// </summary>
		/// <param name="hasImages">The true/false value to set</param>
		public void SetHasImages(bool hasImages)
		{
			HasChoiceImages = hasImages;
		}

		/// <summary>
		/// Set the <see cref="SingleImage"/> property.
		/// </summary>
		/// <param name="singleImage">The string value to set</param>
		public void SetSingleImage(string singleImage)
		{
			SingleImage = singleImage;
		}

		/// <summary>
		/// Make the string format look like a question.
		/// </summary>
		public override string ToString() => Prompt;
	}
}
