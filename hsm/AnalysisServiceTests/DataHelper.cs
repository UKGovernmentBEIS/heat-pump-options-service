using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using OCC.HSM.Model.Entities;
using OCC.HSM.Persistence;

namespace OCC.HSM.Tests
{
    internal class DataHelper
    {
        internal static EohContext BuildEohContext(string filePath = "TestFiles\\eoh.db")
        {
            File.Exists(filePath).Should().BeTrue();

            var optBuilder = new DbContextOptionsBuilder<EohContext>();
            optBuilder.UseSqlite($"Data Source={filePath}");

            return new EohContext(optBuilder.Options);
        }

        internal static EohContext BuildEohContextInMemory()
        {
            var optBuilder = new DbContextOptionsBuilder<EohContext>();
            optBuilder.UseInMemoryDatabase(databaseName: "InMemoryDb");

            return new EohContext(optBuilder.Options);
        }

        internal static async IAsyncEnumerable<Eoh> TakeRandom(int take, DbSet<Eoh> dbSet)
        {
            take.Should().BeGreaterThan(0);
            var r = new Random();
            int recordCount = await dbSet.CountAsync();

            while (take > 0)
            {
                Eoh eoh = await dbSet
                    .Skip(r.Next(0, recordCount - 1))
                    .FirstOrDefaultAsync();
                eoh.Should().NotBeNull();

                yield return eoh;
                take--;
            }
        }
    }
}
