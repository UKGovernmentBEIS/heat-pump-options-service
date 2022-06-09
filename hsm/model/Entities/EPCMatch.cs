using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OCC.HSM.Model.Entities
{
	/// <summary>
	/// Encapsulates a single match between a value extracted from an energy performance
	/// certificate and one or more test values.  This is used in the <see cref="AnswerChoice"/>
	/// class when searching for a match.
	/// </summary>
	public sealed class EPCMatch
	{
		/// <summary>
		/// The key to the certificate value.
		/// </summary>
		private readonly string epcKey_;

		/// <summary>
		/// The type of match to perform.
		/// </summary>
		private readonly EPCMatchType matchType_;

		/// <summary>
		/// The values to match against.
		/// </summary>
		private readonly string[] matchValues_;

		/// <summary>
		/// A range structure used when matching an integer value to a range.
		/// </summary>
		private struct Range
		{
			/// <summary>
			/// The lower bound of the range.
			/// </summary>
			public int lower;
			/// <summary>
			/// Upper bound of the range.
			/// </summary>
			public int upper;
		}

		/// <summary>
		/// Used to match one or more integer ranges.
		/// </summary>
		private readonly Range[] ranges_;

		/// <summary>
		/// Constructs a new instance based on the match type and values.
		/// </summary>
		/// <param name="epcKey">Used to find the value to match in the certificate.</param>
		/// <param name="matchType">The type of match, see <see cref="EPCMatchType"/></param>
		/// <param name="matchValues">The array of values to match.</param>
		public EPCMatch(string epcKey, EPCMatchType matchType, string[] matchValues)
		{
			epcKey_ = epcKey;
			matchType_ = matchType;
			ranges_ = BuildRanges(matchType, matchValues);
			matchValues_ = ranges_.Length == 0 ? matchValues : Array.Empty<string>();
		}

		/// <summary>
		/// Given a certificate this instance tests to see if it matches the required
		/// values.
		/// </summary>
		/// <param name="certificate">A dictionary containing the certificate values.</param>
		/// <returns>True if there is a match, false otherwise.</returns>
		public bool Match(IDictionary<string, string> certificate)
		{
			if(!(certificate is null) && certificate.TryGetValue(epcKey_, out string value)) {
				return matchType_ switch
				{
					EPCMatchType.exact => MatchExact(value),
					EPCMatchType.integerinrange => MatchIntegerInRange(value),
					_ => throw new InvalidOperationException($"Unknown match type {matchType_}")
				};
			}
			return false;
		}

		/// <summary>
		/// Tests if the <paramref name="value"/> is an exact match to *any* of the
		/// <see cref="matchValues_"/>, the match is case sensitive and must be a
		/// whole-word match.
		/// </summary>
		/// <param name="value">The value to test</param>
		/// <returns>True if there is a match, false otherwise.</returns>
		private bool MatchExact(string value)
		{
			return matchValues_.Contains(value);
		}

		/// <summary>
		/// If the <paramref name="text"/> to be an integer value in string form test to
		/// see if it falls within one of the <see cref="ranges_"/>.
		/// </summary>
		/// <param name="text">The integer value in string form.</param>
		/// <returns>True <paramref name="text"/> represents an integer and the value
		/// falls within one of the ranges.</returns>
		private bool MatchIntegerInRange(string text)
		{
			if(double.TryParse(text, out double value)) {
				foreach(Range range in ranges_) {
					if(range.lower <= value && value < range.upper)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Called by the constructor to build an array of <see cref="Range"/> values
		/// to be used when trying an <see cref="EPCMatchType.integerinrange"/>	match.
		/// </summary>
		/// <returns>A new <see cref="Range"/> array which may be empty.</returns>
		private static Range[] BuildRanges(EPCMatchType matchType, string[]? matchValues)
		{
			if(matchType != EPCMatchType.integerinrange
				|| matchValues == null
				|| matchValues.Length == 0)
				return Array.Empty<Range>();

			var ranges = new Range[matchValues.Length];

			for(int idx = 0; idx < ranges.Length; ++idx) {
				string range = matchValues[idx];
				int lower = int.MinValue;
				int upper = int.MaxValue;

				if(range.StartsWith("<", StringComparison.InvariantCultureIgnoreCase)) {
					upper = int.Parse(range[1..], NumberStyles.Integer, CultureInfo.InvariantCulture);
				} else if(range.StartsWith(">", StringComparison.InvariantCultureIgnoreCase)) {
					lower = int.Parse(range[1..], NumberStyles.Integer, CultureInfo.InvariantCulture);
				} else {
					string[] parts = range.Split("-");

					if(parts.Length != 2)
						throw new InvalidOperationException($"{range} is not a valid range");

					lower = int.Parse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture);
					upper = int.Parse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
				}
				ranges[idx].lower = lower;
				ranges[idx].upper = upper;
			}
			return ranges;
		}
	}
}
