using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;

namespace OCC.HSM.Model
{
	/// <summary>
	/// The collection of questions as read from the configuration file.
	/// </summary>
	sealed class QuestionCollection : IQuestionCollection
	{
		/// <summary>
		/// The list of questions read from the configuration file.
		/// </summary>
		private readonly IList<Question> questions_;

		/// <summary>
		/// Exception message for all the methods on the <see cref="ICollection{T}"/> and
		/// <see cref="IList{T}"/> interfaces which are not being supported because the
		/// collection is read only.
		/// </summary>
		private readonly string NOT_SUPPORTED_IS_READONLY
			= $"{nameof(QuestionCollection)} is a read only collection";

		/// <summary>
		/// Create a new immutable instance from the list of <see cref="Question"/>.
		/// </summary>
		/// <param name="questions"></param>
		internal QuestionCollection(IList<Question> questions)
		{
			questions_ = questions ?? new List<Question>();
		}

		/// <summary>
		/// Get the question for the HouseType
		/// </summary>
		public Question HouseType => FindQuestion("housetype");

		/// <summary>
		/// Get the question for the WallType
		/// </summary>
		public Question WallType => FindQuestion("walltype");

		/// <summary>
		/// Get the question for the HouseAgeType
		/// </summary>
		public Question HouseAgeType => FindQuestion("houseage");

		/// <summary>
		/// Get the question for the HouseSizeOption1Type
		/// </summary>
		public Question HouseSizeOption1Type => FindQuestion("housesizeoption1");

		/// <summary>
		/// Get the question for the HouseSizeOption2Type
		/// </summary>
		public Question HouseSizeOption2Type => FindQuestion("housesizeoption2");

		/// <summary>
		/// Get the question for the NumberOfFloorsType
		/// </summary>
		//public Question NumberOfFloorsType => FindQuestion("numberoffloors");

		/// <summary>
		/// Get the question for the RoofType
		/// </summary>
		public Question RoofType => FindQuestion("rooftype");

		/// <summary>
		/// Get the question for the GlazingType
		/// </summary>
		public Question GlazingType => FindQuestion("glazing");

		/// <summary>
		/// Get the question for the GasSupplyType
		/// </summary>
		public Question GasSupplyType => FindQuestion("gassupply");

		/// <summary>
		/// Get the question for the OutsideSpace
		/// </summary>
		public Question OutsideSpace => FindQuestion("outsidespace");

		/// <summary>
		/// Get the question for the CurrentHeatingType
		/// </summary>
		public Question CurrentHeatingType => FindQuestion("currentheatingsystem");

		/// <summary>
		/// Index a question by key
		/// </summary>
		/// <param name="key">The key being sought.</param>
		/// <returns>The matching question or null if not found</returns>
		public Question this[string key] => FindQuestion(key);

		/// <summary>
		/// Look up a question by key.
		/// </summary>
		/// <param name="key">The key to look up, must be an exact match.</param>
		/// <returns>The question or null if not found.</returns>
		private Question FindQuestion(string key)
		{
			return questions_.FirstOrDefault(q => q.Key == key);
		}

		#region ICollection<Question> implementation

		/// <inheritdoc/>
		public int Count => questions_.Count;

		/// <inheritdoc/>
		public bool IsReadOnly => true;
		
        /// <inheritdoc/>
        /// <exception cref="NotSupportedException">Always thrown</exception>
        public void Add(Question item)
			=> throw new NotSupportedException(NOT_SUPPORTED_IS_READONLY);

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">Always thrown</exception>
		public void Clear()
		{
			throw new NotSupportedException(NOT_SUPPORTED_IS_READONLY);
		}

		/// <inheritdoc/>
		public bool Contains(Question item)
		{
			return questions_.Contains(item);
		}

		/// <inheritdoc/>
		public void CopyTo(Question[] array, int arrayIndex)
		{
			questions_.CopyTo(array, arrayIndex);
		}

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">Always thrown</exception>
		public bool Remove(Question item)
		{
			throw new NotSupportedException(NOT_SUPPORTED_IS_READONLY);
		}

		/// <inheritdoc/>
		public IEnumerator<Question> GetEnumerator()
		{
			return questions_.GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion  // ICollection<Question>

		#region IList<Question>

		/// <inheritdoc/>
		public Question this[int index] {
			get => questions_[index];
			set => throw new NotSupportedException(NOT_SUPPORTED_IS_READONLY);
		}

		/// <inheritdoc/>
		public int IndexOf(Question item)
		{
			return questions_.IndexOf(item);
		}

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">Always thrown</exception>
		public void Insert(int index, Question item)
		{
			throw new NotSupportedException(NOT_SUPPORTED_IS_READONLY);
		}

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">Always thrown</exception>
		public void RemoveAt(int index)
		{
			throw new NotSupportedException(NOT_SUPPORTED_IS_READONLY);
		}
		#endregion // IList<Question>
	}
}
