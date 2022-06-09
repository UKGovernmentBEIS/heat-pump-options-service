using OCC.HSM.Model.Entities;

namespace OCC.HSM.Model.Interfaces
{
	/// <summary>
	/// An interface used to give access to the analysis engine to retrieve results from
	/// a set of answers.
	/// </summary>
	public interface IAnalysisService
	{
		/// <summary>
		/// Runs the analysis.
		/// </summary>
		/// <param name="answers">The answers to be sent to the analysis engine.</param>
		/// <returns></returns>
		Eoh GetResult(HsmKey answers);

		public HsmKey GetUserChoices();
	}
}
