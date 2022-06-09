using OCC.HSM.Model.Entities;
using OCC.HSM.Persistence;
using System.Collections.Generic;
using System.Linq;

namespace OCC.HSM.Analysis
{
    public class EohMemoryCache : IEohMemoryCache
    {
        public IDictionary<HsmKey, Eoh> Results { get; private set; }

        public void LoadEohTableFromDb(EohContext dbContext)
        {
            IDictionary<HsmKey, Eoh> results = new Dictionary<HsmKey, Eoh>();

            var eoh = dbContext.Eoh.ToList();
           
            foreach (var item in eoh)
            {
                var hsmKeyObj = new HsmKey
                {
                    HouseType = item.HouseType,
                    WallType = item.WallType,
                    HouseAge = item.HouseAge,
                    HouseSizeOption1 = item.HouseSize,                   
                    RoofType = item.RoofType,
                    Glazing = item.Glazing,
                    GasSupply = item.GasSupply,
                    OutsideSpace = item.OutsideSpace,
                    CurrentHeatingSystem = item.CurrentSystem
                };

                results.Add(hsmKeyObj, item);
            }

            Results = results;
        }
    }
}
