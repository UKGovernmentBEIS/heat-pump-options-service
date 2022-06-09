using System.ComponentModel.DataAnnotations;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
#nullable disable

namespace OCC.HSM.Model.Entities
{
    /// <summary>
    /// The database entity EoH(Electrification of Heat) model
    /// </summary>
    public partial class Eoh
    {
        /// <summary>
        /// The key is used only for unit testing
        /// </summary>
        [Key]
        public int Id { get; set; }
        public ushort HouseType { get; set; }
        public ushort WallType { get; set; }
        public ushort HouseAge { get; set; }
        public ushort HouseSize { get; set; }        
        public ushort RoofType { get; set; }
        public ushort Glazing { get; set; }
        public ushort GasSupply { get; set; }
        public ushort OutsideSpace { get; set; }
        public ushort CurrentSystem { get; set; }
        public ushort LtAshpSuitable { get; set; }
        public ushort HtAshpSuitable { get; set; }
        public ushort GshpSuitable { get; set; }
        public ushort HhpSuitable { get; set; }
        public double? LtAshpEmissionSavings { get; set; }
        public double? HtAshpEmissionSavings { get; set; }
        public double? GshpEmissionSavings { get; set; }
        public double? HhpEmissionSavings { get; set; }
        public double? LtAshpRunCostMin { get; set; }
        public double? LtAshpRunCostMax { get; set; }
        public double? HtAshpRunCostMin { get; set; }
        public double? HtAshpRunCostMax { get; set; }
        public double? GshpRunCostMin { get; set; }
        public double? GshpRunCostMax { get; set; }
        public double? HhpRunCostMin { get; set; }
        public double? HhpRunCostMax { get; set; }
        public double? LtAshp { get; set; }
        public double? HtAshp { get; set; }
        public double? Gshp { get; set; }
        public double? Hhp { get; set; }
        public double? LtAshpEquipCostMin { get; set; }
        public double? LtAshpEquipCostMax { get; set; }
        public double? HtAshpEquipCostMin { get; set; }
        public double? HtAshpEquipCostMax { get; set; }
        public double? GshpEquipCostMin { get; set; }
        public double? GshpEquipCostMax { get; set; }
        public double? HhpEquipCostMin { get; set; }
        public double? HhpEquipCostMax { get; set; }
        public double? HpInsulationUpgrade { get; set; }
        public double? HpRadiatorUpgrade { get; set; }
        public double? HpDblGlazingUpgrade { get; set; }
        public double? HpLoftInsUpgrade { get; set; }
        public double? HhpInsulationUpgrade { get; set; }
        public double? HhpRadiatorUpgrade { get; set; }
        public double? HhpDblGlazingUpgrade { get; set; }
        public double? HhpLoftInsUpgrade { get; set; }
        public double? LoftInsulationUpgradeCost { get; set; }
        public double? WallInsulationUpgradeCost { get; set; }
        public double? DblGlazingUpgradeCost { get; set; }
        public double? RadiatorUpgradeCost { get; set; }
    }
}
