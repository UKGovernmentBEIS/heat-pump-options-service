using System.Collections;
using System.Collections.Generic;

using OCC.HSM.Model.Entities;

namespace OCC.HSM.Model.Interfaces
{
	/// <summary>
	/// An interface to the questions to be made available through the application's 
	/// configuration.
	/// </summary>
	public interface IQuestionCollection : ICollection<Question>, IEnumerable<Question>, IEnumerable, IList<Question>
	{
        /// <summary>
        /// Get the question for the HouseType
        /// </summary>
        Question HouseType { get; }

        /// <summary>
        /// Get the question for the WallType
        /// </summary>
        Question WallType { get; }

        /// <summary>
        /// Get the question for the HouseAgeType
        /// </summary>
        Question HouseAgeType { get; }

        /// <summary>
        /// Get the question for the HouseSizeOption1Type
        /// </summary>
        Question HouseSizeOption1Type { get; }

        /// <summary>
        /// Get the question for the HouseSizeOption2Type
        /// </summary>
        Question HouseSizeOption2Type { get; }

        /// <summary>
        /// Get the question for the RoofType
        /// </summary>
        Question RoofType { get; }

        /// <summary>
        /// Get the question for the GlazingType
        /// </summary>
        Question GlazingType { get; }

        /// <summary>
        /// Get the question for the GasSupplyType
        /// </summary>
        Question GasSupplyType { get; }

        /// <summary>
        /// Get the question for the OutsideSpace
        /// </summary>
        Question OutsideSpace { get; }

        /// <summary>
        /// Get the question for the CurrentHeatingType
        /// </summary>
        Question CurrentHeatingType { get; }

        /// <summary>
        /// Get a question using its key value.
        /// </summary>
        /// <param name="key">The key to the question.</param>
        /// <returns>The question or null if not available</returns>
        Question this[string key] { get; }
	}
}
