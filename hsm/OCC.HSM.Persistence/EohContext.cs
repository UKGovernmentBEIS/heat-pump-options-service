using Microsoft.EntityFrameworkCore;
using OCC.HSM.Model.Entities;
using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace OCC.HSM.Persistence
{
    public partial class EohContext : DbContext
    {
        public EohContext()
        {
        }

        public EohContext(DbContextOptions<EohContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Eoh> Eoh { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Eoh>(entity =>
            {

                if (!Database.IsInMemory())
                {
                    entity.HasNoKey();
                    entity.Ignore(e => e.Id);                    
                }
                else
                {
                    entity.HasKey(e => e.Id);
                }

                entity.ToTable("eoh");

                entity.Property(e => e.CurrentSystem)
                    .HasColumnName("Current_System")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.DblGlazingUpgradeCost)
                    .HasColumnName("dbl_glazing_upgrade_cost")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.GasSupply)
                    .HasColumnName("Gas_Supply")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.Glazing).HasColumnType("SMALLINT");

                entity.Property(e => e.Gshp)
                    .HasColumnName("gshp")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.GshpEmissionSavings)
                    .HasColumnName("gshp_emission_savings")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.GshpEquipCostMax)
                    .HasColumnName("gshp_equip_cost_max")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.GshpEquipCostMin)
                    .HasColumnName("gshp_equip_cost_min")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.GshpRunCostMax)
                    .HasColumnName("gshp_run_cost_max")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.GshpRunCostMin)
                    .HasColumnName("gshp_run_cost_min")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.GshpSuitable)
                    .HasColumnName("gshp_suitable")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.Hhp)
                    .HasColumnName("hhp")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HhpDblGlazingUpgrade)
                    .HasColumnName("hhp_dbl_glazing_upgrade")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HhpEmissionSavings)
                    .HasColumnName("hhp_emission_savings")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HhpEquipCostMax)
                    .HasColumnName("hhp_equip_cost_max")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HhpEquipCostMin)
                    .HasColumnName("hhp_equip_cost_min")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HhpInsulationUpgrade)
                    .HasColumnName("hhp_insulation_upgrade")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HhpLoftInsUpgrade)
                    .HasColumnName("hhp_loft_ins_upgrade")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HhpRadiatorUpgrade)
                    .HasColumnName("hhp_radiator_upgrade")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HhpRunCostMax)
                    .HasColumnName("hhp_run_cost_max")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HhpRunCostMin)
                    .HasColumnName("hhp_run_cost_min")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HhpSuitable)
                    .HasColumnName("hhp_suitable")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HouseAge)
                    .HasColumnName("House_Age")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HouseSize)
                    .HasColumnName("House_Size")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HouseType)
                    .HasColumnName("House_Type")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HpDblGlazingUpgrade)
                    .HasColumnName("hp_dbl_glazing_upgrade")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HpInsulationUpgrade)
                    .HasColumnName("hp_insulation_upgrade")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HpLoftInsUpgrade)
                    .HasColumnName("hp_loft_ins_upgrade")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HpRadiatorUpgrade)
                    .HasColumnName("hp_radiator_upgrade")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HtAshp)
                    .HasColumnName("ht_ashp")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HtAshpEmissionSavings)
                    .HasColumnName("ht_ashp_emission_savings")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HtAshpEquipCostMax)
                    .HasColumnName("ht_ashp_equip_cost_max")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HtAshpEquipCostMin)
                    .HasColumnName("ht_ashp_equip_cost_min")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HtAshpRunCostMax)
                    .HasColumnName("ht_ashp_run_cost_max")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HtAshpRunCostMin)
                    .HasColumnName("ht_ashp_run_cost_min")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.HtAshpSuitable)
                    .HasColumnName("ht_ashp_suitable")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.LoftInsulationUpgradeCost)
                    .HasColumnName("loft_insulation_upgrade_cost")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.LtAshp)
                    .HasColumnName("lt_ashp")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.LtAshpEmissionSavings)
                    .HasColumnName("lt_ashp_emission_savings")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.LtAshpEquipCostMax)
                    .HasColumnName("lt_ashp_equip_cost_max")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.LtAshpEquipCostMin)
                    .HasColumnName("lt_ashp_equip_cost_min")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.LtAshpRunCostMax)
                    .HasColumnName("lt_ashp_run_cost_max")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.LtAshpRunCostMin)
                    .HasColumnName("lt_ashp_run_cost_min")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.LtAshpSuitable)
                    .HasColumnName("lt_ashp_suitable")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.OutsideSpace)
                    .HasColumnName("Outside_Space")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.RadiatorUpgradeCost)
                    .HasColumnName("radiator_upgrade_cost")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.RoofType)
                    .HasColumnName("Roof_Type")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.WallInsulationUpgradeCost)
                    .HasColumnName("wall_insulation_upgrade_cost")
                    .HasColumnType("SMALLINT");

                entity.Property(e => e.WallType)
                    .HasColumnName("Wall_Type")
                    .HasColumnType("SMALLINT");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
