using System.Collections.Generic;
using System.Linq;

namespace OCC.HSM.Model.Entities
{
	/// <summary>
	/// Instance of this contain a single choice for an answer to a question.
	/// </summary>
	public sealed class AnswerChoice
	{
		/// <summary>
		/// A unique key for this option.
		/// </summary>
		public string Key { get; }

		/// <summary>
		/// The text displayed for this answer.
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// The description text displayed for this answer.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// The database encoding against this answer.
		/// </summary>
		public short DbEncoding { get; }

		/// <summary>
		/// The array of zero or more certificate match instances used to determine if this
		/// answer matches the give certificate value.
		/// </summary>
		private readonly EPCMatch[]? epcMatches_;

		/// <summary>
		/// Construct an immutable instance of an answer.
		/// </summary>
		/// <param name="key">Unique key for this answer.</param>
		/// <param name="text">The display text for this answer.</param>
		/// <param name="epcMatches">The <see cref="EPCMatch"/> matches, maybe null.</param>
		public AnswerChoice(string key, string text, string description, short dbencoding, IEnumerable<EPCMatch>? epcMatches)
		{
			Key = key;
			Text = text;
			Description = description;
			DbEncoding = dbencoding;
			epcMatches_ = epcMatches?.ToArray();
		}

		/// <summary>
		/// Tests this choice against the <paramref name="certificate"/> to see if all 
		/// <see cref="epcMatches_"/> match.  **Note**: currently a match to all matches
		/// is required---there is no provision for supporting less that all matches.
		/// </summary>
		/// <param name="certificate">A dictionary object containing the certificate values.</param>
		/// <returns>True if there is a match false if either there is no match or this
		/// choice has a null or empty <see cref="EPCMatch"/> array.</returns>
		public bool Matches(IDictionary<string, string> certificate)
		{
			if(epcMatches_ is null || epcMatches_.Length == 0)
				return false;

			return epcMatches_.All(m => m.Match(certificate));
		}

		/// <summary>
		/// Make the string representation useful.
		/// </summary>
		public override string ToString() => $"{Text} ({Key})";
	}
}
