using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OCC.HSM.Analysis;
using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using OCC.HSM.Persistence;

using System.Threading.Tasks;
using Xunit;

namespace OCC.HSM.Tests
{
    public class AnalysisServiceTests
    {
        [Fact]
        public void Test_Connection_String_Configuration_Exists_Success()
        {
            //Arrange
            var mockConfSection = new Mock<IConfigurationSection>();
            mockConfSection.SetupGet(m => m[It.Is<string>(s => s == "SqliteDatabase")]).Returns("mock value");

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(a => a.GetSection(It.Is<string>(s => s == "ConnectionStrings"))).Returns(mockConfSection.Object);

            //Assert
            Assert.Equal("mock value", mockConfiguration.Object.GetConnectionString("SqliteDatabase"));
        }

        [Fact]
        public async Task GetResult_Answers_Null_Failure()
        {
            // Arrange
            using EohContext dbContext = DataHelper.BuildEohContextInMemory();
            Assert.NotNull(dbContext);
            await AddEntityToInMemoryDbContext(dbContext);

            var analysisService = new AnalysisService.AnalysisService(Mock.Of<ILogger>(), Mock.Of<IEohMemoryCache>(), Mock.Of<IHttpContextAccessor>(), Mock.Of<IApplicationConfiguration>());

            // Act
            var results = analysisService.GetResult(null);

            // Assert
            Assert.Equal(0, results.CurrentSystem);
        }

        [Fact]
        public async Task GetResult_GivenHsmKey_EohRowShouldMatchKeyFields()
        {
            // Arrange
            using EohContext dbContext = DataHelper.BuildEohContext();
            var sample = DataHelper.TakeRandom(5, dbContext.Eoh);
            EohMemoryCache eohCache = new EohMemoryCache();
            eohCache.LoadEohTableFromDb(dbContext);

            var sut = new AnalysisService.AnalysisService(Mock.Of<ILogger>(), eohCache, Mock.Of<IHttpContextAccessor>(), Mock.Of<IApplicationConfiguration>());
            
            await foreach (Eoh randomEoh in sample)
            {
                HsmKey key = HsmKey.FromEoh(randomEoh);

                // Act
                Eoh act = sut.GetResult(key);

                // Assert
                act.CurrentSystem.Should().Be(key.CurrentHeatingSystem);
                act.GasSupply.Should().Be(key.GasSupply);
                act.Glazing.Should().Be(key.Glazing);
                act.HouseAge.Should().Be(key.HouseAge);
                act.HouseSize.Should().Be(key.HouseSizeOption1);
                act.HouseType.Should().Be(key.HouseType);
                act.OutsideSpace.Should().Be(key.OutsideSpace);
                act.RoofType.Should().Be(key.RoofType);
                act.WallType.Should().Be(key.WallType);
            }
        }

        private async Task AddEntityToInMemoryDbContext(EohContext dbContext)
        {
            var entity = new Eoh
            {
                HouseType = 1,
                HouseAge = 1,
                HouseSize = 1,
                WallType = 1,
                RoofType = 4,
                Glazing = 2,
                OutsideSpace = 2,
                CurrentSystem = 3,
                GasSupply = 1
            };

            var entry = await dbContext.Eoh.AddAsync(entity);
            entry.State.Should().Be(EntityState.Added);

            (await dbContext.SaveChangesAsync()).Should().Be(1);
        }
    }
}
