using Microsoft.Extensions.Configuration;
using OCC.HSM.Model.Entities;

namespace OCC.HSM.UI.ViewModels
{
    public class ResultsComparisonViewModel : PotentialUpgradesViewModel
    {
        /// <summary>
		/// The result object loaded from the sqlite db.
		/// </summary>
		public Eoh EohResult { get; set; }                       

        public NextStepsViewModel NextStepsViewModel { get; set; }

        public ResultsComparisonViewModel(IConfiguration iConfig)
        {
            NextStepsViewModel = new NextStepsViewModel(iConfig);            
        }
    }
}
