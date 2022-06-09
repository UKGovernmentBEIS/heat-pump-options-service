using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using OCC.HSM.Persistence;

using Xunit;

namespace OCC.HSM.Tests
{
    public class EohContextTest
    {
        [Fact]
        public void EohContext_ShouldNotHaveDuplicateCompoundKeyRows()
        {
            // Arrange
            using EohContext sut = DataHelper.BuildEohContext();

            // Act
            var act = sut.Eoh
                .Select(x => 
                    new { // the compound key
                        x.HouseType,
                        x.WallType,
                        x.HouseAge,
                        x.HouseSize,
                        x.RoofType,
                        x.Glazing,
                        x.GasSupply,
                        x.OutsideSpace,
                        x.CurrentSystem
                    })
                .ToList();

            // Assert
            act.Should()
                .HaveCountGreaterThan(300000)
                .And
                .OnlyHaveUniqueItems(x => x);
        }
    }
}
