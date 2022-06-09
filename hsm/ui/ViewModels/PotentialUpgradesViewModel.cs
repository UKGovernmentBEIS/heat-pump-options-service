using OCC.HSM.Model.Entities;
using OCC.HSM.UI.Pages.Enums;
using System;

namespace OCC.HSM.UI.ViewModels
{
    public class PotentialUpgradesViewModel
    {
        /// <summary>
        /// Wall Insulation approx cost (�)
        /// </summary>
        public double? WallInsulationApproxCost { get; set; }

        /// <summary>
        /// Loft Insulation approx cost (�)
        /// </summary>
        public double? LoftInsulationApproxCost { get; set; }

        /// <summary>
        /// Glazing upgrade approx cost (�)
        /// </summary>
        public double? GlazingUpgradeApproxCost { get; set; }

        /// <summary>        
        /// Radiator change approx cost (�)
        /// </summary>
        public double? RadiatorChangeApproxCost { get; set; }

        /// <summary>
        /// Get the potential upgrade costs for all the heat pumps except hybrid
        /// </summary>
        public void GetPotentialUpgradesHeatPump(Eoh eohResult)
        {
            if (eohResult == null)
                throw new ArgumentNullException(nameof(eohResult));

            if (eohResult.HpInsulationUpgrade == (int)EnumUpgrade.TypicallyRequired)
            {
                WallInsulationApproxCost = eohResult.WallInsulationUpgradeCost;
            }

            if (eohResult.HpRadiatorUpgrade == (int)EnumUpgrade.TypicallyRequired)
            {
                RadiatorChangeApproxCost = eohResult.RadiatorUpgradeCost;
            }

            if (eohResult.HpLoftInsUpgrade == (int)EnumUpgrade.TypicallyRequired)
            {
                LoftInsulationApproxCost = eohResult.LoftInsulationUpgradeCost;
            }

            if (eohResult.HpDblGlazingUpgrade == (int)EnumUpgrade.TypicallyRequired)
            {
                GlazingUpgradeApproxCost = eohResult.DblGlazingUpgradeCost;
            }
        }

        /// <summary>
        /// Get the potential upgrade costs for hybrid heat pump
        /// </summary>
        public void GetPotentialUpgradesHybrid(Eoh eohResult)
        {
            if (eohResult == null)
                throw new ArgumentNullException(nameof(eohResult));

            if (eohResult.HhpInsulationUpgrade == (int)EnumUpgrade.TypicallyRequired)
            {
                WallInsulationApproxCost = eohResult.WallInsulationUpgradeCost;
            }

            if (eohResult.HhpRadiatorUpgrade == (int)EnumUpgrade.TypicallyRequired)
            {
                RadiatorChangeApproxCost = eohResult.RadiatorUpgradeCost;
            }

            if (eohResult.HhpLoftInsUpgrade == (int)EnumUpgrade.TypicallyRequired)
            {
                LoftInsulationApproxCost = eohResult.LoftInsulationUpgradeCost;
            }

            if (eohResult.HhpDblGlazingUpgrade == (int)EnumUpgrade.TypicallyRequired)
            {
                GlazingUpgradeApproxCost = eohResult.DblGlazingUpgradeCost;
            }
        }
    }
}
