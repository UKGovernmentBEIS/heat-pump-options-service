using System.ComponentModel;

namespace OCC.HSM.UI.Pages.Enums
{
    public enum EnumHeatPumpType
    {       
        [Description("Air Source")]
        AirSource,
        [Description("High Temperature Air Source")]
        HighTempAirSource,
        [Description("Ground Source")]
        GroundSource,
        [Description("Hybrid")]
        Hybrid
    }
}
