using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using OCC.HSM.Analysis;
using OCC.HSM.Persistence;
using OCC.HSM.UI;

using Xunit;

namespace OCC.HSM.Tests
{
    public class WebAppTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public WebAppTest(WebApplicationFactory<Startup> fixture)
        {
            _factory = fixture;
        }

        [Fact]
        public void Startup_DiShouldOnlyHaveCachedResultsNotDbContext()
        {
            // Arrange
            var sut = _factory.Services;
            sut.Should().NotBeNull();

            // Act
            var eohMemCache = sut.GetService<IEohMemoryCache>();
            var dbContext = sut.GetService<EohContext>();

            // Assert
            eohMemCache.Should().NotBeNull();
            eohMemCache.Results.Should().NotBeNullOrEmpty("should have cached the SQLite database contents");

            dbContext.Should().BeNull("EohContext not needed in DI; rather it is constructed in a factory just for EohMemoryCache");
        }
    }
}
