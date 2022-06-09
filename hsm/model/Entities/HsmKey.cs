namespace OCC.HSM.Model.Entities
{
    /// <summary>
    /// This class is used to gather the user answers in as single object.
    /// </summary>
    public class HsmKey
    {
        public HsmKey() 
        {
        }

        public ushort? HouseType { get; set; }
        
        public ushort? WallType { get; set; }
        
        public ushort? HouseAge { get; set; }

        public ushort? HouseSizeOption1 { get; set; }

        /// <summary>
        /// This property is needed in AnalysisService.GetUserChoices() method to calculate HouseSizeOption1 property value 
        /// </summary>
        public ushort? HouseSizeOption2 { get; set; }

        public ushort? RoofType { get; set; }

        public ushort? Glazing { get; set; }

        public static HsmKey FromEoh(Eoh eohRow)
        {
            if (eohRow == null) throw new System.ArgumentNullException(nameof(eohRow));

            return new HsmKey
            {
                CurrentHeatingSystem = eohRow.CurrentSystem,
                GasSupply = eohRow.GasSupply,
                Glazing = eohRow.Glazing,
                HouseAge = eohRow.HouseAge,
                HouseSizeOption1 = eohRow.HouseSize,                
                HouseType = eohRow.HouseType,
                OutsideSpace = eohRow.OutsideSpace,
                RoofType = eohRow.RoofType,
                WallType = eohRow.WallType,
            };
        }

        public ushort? GasSupply { get; set; }

        public ushort? OutsideSpace { get; set; } 

        public ushort? CurrentHeatingSystem { get; set; }

        public override bool Equals(object other)
        {
            return other is HsmKey p
                && p.HouseType == HouseType
                && p.WallType == WallType
                && p.HouseAge == HouseAge
                && p.HouseSizeOption1 == HouseSizeOption1
                && p.RoofType == RoofType
                && p.Glazing == Glazing
                && p.GasSupply == GasSupply
                && p.OutsideSpace == OutsideSpace
                && p.CurrentHeatingSystem == CurrentHeatingSystem;
        }

        public override int GetHashCode()
        {
            unchecked // Allow arithmetic overflow, numbers will just "wrap around"
            {
                int hashcode = 1430287;
                hashcode = hashcode * 7302013 ^ HouseType.GetHashCode();
                hashcode = hashcode * 7302013 ^ WallType.GetHashCode();
                hashcode = hashcode * 7302013 ^ HouseAge.GetHashCode();                
                hashcode = hashcode * 7302013 ^ RoofType.GetHashCode();
                hashcode = hashcode * 7302013 ^ Glazing.GetHashCode();
                hashcode = hashcode * 7302013 ^ GasSupply.GetHashCode();
                hashcode = hashcode * 7302013 ^ OutsideSpace.GetHashCode();
                hashcode = hashcode * 7302013 ^ CurrentHeatingSystem.GetHashCode();
                return hashcode;
            }
        }        
    }
}
