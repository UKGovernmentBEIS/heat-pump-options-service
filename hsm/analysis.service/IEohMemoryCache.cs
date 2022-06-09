using OCC.HSM.Model.Entities;
using OCC.HSM.Persistence;
using System.Collections.Generic;

namespace OCC.HSM.Analysis
{
    /// <summary>
	/// An interface used to load the Eoh database to the memory cache and let the analysis service to retrieve results from the memory.
	/// </summary>
    public interface IEohMemoryCache
    {
        IDictionary<HsmKey, Eoh> Results { get; }

        void LoadEohTableFromDb(EohContext context);
    }
}
