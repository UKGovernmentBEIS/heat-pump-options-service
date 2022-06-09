using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using OCC.HSM.Analysis;
using OCC.HSM.Model.Entities;

using Xunit;

namespace OCC.HSM.Tests
{
    public class EohMemoryCacheTest
    {
        [Fact]
        public async Task Results_GivenHsmKey_EohRowShouldMatchKeyFields()
        {
            // Arrange
            using var dbContext = DataHelper.BuildEohContext();
            var keys = DataHelper.TakeRandom(5, dbContext.Eoh);

            var sut = new EohMemoryCache();
            sut.LoadEohTableFromDb(dbContext);

            int eohCount = dbContext.Eoh.Count();
            sut.Results.Count().Should().Be(eohCount);

            await foreach (Eoh expectedKey in keys)
            {
                HsmKey answerKey = HsmKey.FromEoh(expectedKey);

                // Act
                Eoh act = sut.Results[answerKey];

                // Assert
                act.CurrentSystem.Should().Be(expectedKey.CurrentSystem);
                act.GasSupply.Should().Be(expectedKey.GasSupply);
                act.Glazing.Should().Be(expectedKey.Glazing);
                act.HouseAge.Should().Be(expectedKey.HouseAge);
                act.HouseSize.Should().Be(expectedKey.HouseSize);
                act.HouseType.Should().Be(expectedKey.HouseType);
                act.OutsideSpace.Should().Be(expectedKey.OutsideSpace);
                act.RoofType.Should().Be(expectedKey.RoofType);
                act.WallType.Should().Be(expectedKey.WallType);

                act.Should().BeEquivalentTo(expectedKey);
            }
        }
    }
}
