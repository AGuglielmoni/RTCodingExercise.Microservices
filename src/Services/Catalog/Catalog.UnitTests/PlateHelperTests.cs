using Catalog.API.Data;
using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PlateLogic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Catalog.UnitTests
{
    public class PlateHelperTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IPlateHelper _plateHelper;
        private readonly Mock<ILogger<PlateHelper>> _loggerMock;

        public PlateHelperTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PlateDb")
                .Options;

            _context = new ApplicationDbContext(options);

            // Mock ILogger
            _loggerMock = new Mock<ILogger<PlateHelper>>();

            // Create PlateHelper with logger
            _plateHelper = new PlateHelper(_context, _loggerMock.Object);

            // Ensure the database is clean before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            // Clean up the database after each test to prevent state leakage
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetPlatesAsync_ShouldReturnPaginatedPlates()
        {
            // Arrange
            var plates = new List<Plate>
            {
                new Plate { Id = Guid.NewGuid(), Registration = "ABC123", PurchasePrice = 100, SalePrice = 120 },
                new Plate { Id = Guid.NewGuid(), Registration = "XYZ789", PurchasePrice = 200, SalePrice = 240 },
                new Plate { Id = Guid.NewGuid(), Registration = "LMN456", PurchasePrice = 300, SalePrice = 360 }
            };

            await _context.Plates.AddRangeAsync(plates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _plateHelper.GetPlatesAsync(1, 2); // 2 plates per page

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, plate => plate.Registration == "ABC123");
            Assert.Contains(result, plate => plate.Registration == "XYZ789");
        }

        [Fact]
        public async Task AddPlateAsync_ShouldAddPlateAndReturnPlateDto()
        {
            // Arrange
            var plateDto = new PlateDto
            {
                Registration = "NEW123",
                PurchasePrice = 150,
                SalePrice = 180
            };

            // Act
            var result = await _plateHelper.AddPlateAsync(plateDto);

            // Assert
            var addedPlate = await _context.Plates.FirstOrDefaultAsync(p => p.Registration == plateDto.Registration);
            Assert.NotNull(addedPlate);  // Verify that the plate is added to the in-memory database
            Assert.Equal(plateDto.Registration, result.Registration);
            Assert.Equal(plateDto.PurchasePrice, result.PurchasePrice);
            Assert.Equal(plateDto.SalePrice, result.SalePrice);
        }

        [Fact]
        public async Task ApplyMarkupAsync_ShouldApplyMarkupToSalePrice()
        {
            // Arrange
            var plates = new List<Plate>
            {
                new Plate { Id = Guid.NewGuid(), Registration = "ABC123", PurchasePrice = 100, SalePrice = 120 },
                new Plate { Id = Guid.NewGuid(), Registration = "XYZ789", PurchasePrice = 200, SalePrice = 240 }
            };

            await _context.Plates.AddRangeAsync(plates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _plateHelper.ApplyMarkupAsync();

            // Assert
            Assert.Equal(120, result[0].SalePrice); // 100 * 1.20 = 120
            Assert.Equal(240, result[1].SalePrice); // 200 * 1.20 = 240
        }

        [Fact]
        public async Task GetPlatesAsync_FilterByLetters_ReturnsMatchingPlates()
        {
            // Arrange
            var plates = new List<Plate>
            {
                new Plate { Id = Guid.NewGuid(), Registration = "Danny DA12 NNY", PurchasePrice = 100, SalePrice = 120, Letters = "DA", Numbers = 12 },
                new Plate { Id = Guid.NewGuid(), Registration = "James JAM 3S", PurchasePrice = 150, SalePrice = 180, Letters = "JAM", Numbers = 3 },
                new Plate { Id = Guid.NewGuid(), Registration = "G Smith GSM 17H", PurchasePrice = 200, SalePrice = 240, Letters = "GSM", Numbers = 17 }
            };

            await _context.Plates.AddRangeAsync(plates);
            await _context.SaveChangesAsync();

            string filter = "DA";

            // Act
            var result = await _plateHelper.GetPlatesAsync(1, 10, null, filter);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, plate => Assert.Contains(filter.ToLower(), plate.Registration.ToLower()));
        }

        [Fact]
        public async Task GetPlatesAsync_FilterByNumbers_ReturnsMatchingPlates()
        {
            // Arrange
            var plates = new List<Plate>
            {
                new Plate { Id = Guid.NewGuid(), Registration = "Danny DA12 NNY", PurchasePrice = 100, SalePrice = 120, Letters = "DA", Numbers = 12 },
                new Plate { Id = Guid.NewGuid(), Registration = "James JAM 3S", PurchasePrice = 150, SalePrice = 180, Letters = "JAM", Numbers = 3 },
                new Plate { Id = Guid.NewGuid(), Registration = "G Smith GSM 17H", PurchasePrice = 200, SalePrice = 240, Letters = "GSM", Numbers = 17 }
            };

            await _context.Plates.AddRangeAsync(plates);
            await _context.SaveChangesAsync();

            string filter = "17";

            // Act
            var result = await _plateHelper.GetPlatesAsync(1, 10, null, filter);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, plate => Assert.Contains(filter, plate.Registration));
        }

        [Fact]
        public async Task GetPlatesAsync_SortByPriceAscending_ReturnsSortedPlates()
        {
            // Arrange
            var plates = new List<Plate>
            {
                new Plate { Id = Guid.NewGuid(), Registration = "Danny DA12 NNY", PurchasePrice = 100, SalePrice = 120 },
                new Plate { Id = Guid.NewGuid(), Registration = "James JAM 3S", PurchasePrice = 150, SalePrice = 180 },
                new Plate { Id = Guid.NewGuid(), Registration = "G Smith GSM 17H", PurchasePrice = 200, SalePrice = 240 }
            };

            await _context.Plates.AddRangeAsync(plates);
            await _context.SaveChangesAsync();

            string sortOrder = "asc";

            // Act
            var result = await _plateHelper.GetPlatesAsync(1, 10, sortOrder);

            // Assert
            Assert.Equal(3, result.Count);  // Verify 3 plates are returned
            Assert.True(result[0].SalePrice <= result[1].SalePrice);
            Assert.True(result[1].SalePrice <= result[2].SalePrice);
        }

        [Fact]
        public async Task GetPlatesAsync_SortByPriceDescending_ReturnsSortedPlates()
        {
            // Arrange
            var plates = new List<Plate>
            {
                new Plate { Id = Guid.NewGuid(), Registration = "Danny DA12 NNY", PurchasePrice = 100, SalePrice = 120 },
                new Plate { Id = Guid.NewGuid(), Registration = "James JAM 3S", PurchasePrice = 150, SalePrice = 180 },
                new Plate { Id = Guid.NewGuid(), Registration = "G Smith GSM 17H", PurchasePrice = 200, SalePrice = 240 }
            };

            await _context.Plates.AddRangeAsync(plates);
            await _context.SaveChangesAsync();

            string sortOrder = "desc";

            // Act
            var result = await _plateHelper.GetPlatesAsync(1, 10, sortOrder);

            // Assert
            Assert.Equal(3, result.Count);  // Verify 3 plates are returned
            Assert.True(result[0].SalePrice >= result[1].SalePrice);
            Assert.True(result[1].SalePrice >= result[2].SalePrice);
        }

        [Fact]
        public async Task GetPlatesAsync_ShouldReturnOnlyForSalePlates_WhenIsForSaleIsTrue()
        {
            // Arrange
            var plates = new List<Plate>
    {
        new Plate { Id = Guid.NewGuid(), Registration = "Danny DA12 NNY", PurchasePrice = 100, SalePrice = 120, IsForSale = true },
        new Plate { Id = Guid.NewGuid(), Registration = "James JAM 3S", PurchasePrice = 150, SalePrice = 180, IsForSale = false },  // Reserved plate
        new Plate { Id = Guid.NewGuid(), Registration = "G Smith GSM 17H", PurchasePrice = 200, SalePrice = 240, IsForSale = true }
    };

            await _context.Plates.AddRangeAsync(plates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _plateHelper.GetPlatesAsync(1, 10, isForSale: true);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, plate => Assert.True(plate.IsForSale));
        }

        [Fact]
        public async Task GetPlatesAsync_ShouldReturnOnlyReservedPlates_WhenIsForSaleIsFalse()
        {
            // Arrange
            var plates = new List<Plate>
    {
        new Plate { Id = Guid.NewGuid(), Registration = "Danny DA12 NNY", PurchasePrice = 100, SalePrice = 120, IsForSale = true },
        new Plate { Id = Guid.NewGuid(), Registration = "James JAM 3S", PurchasePrice = 150, SalePrice = 180, IsForSale = false },  // Reserved plate
        new Plate { Id = Guid.NewGuid(), Registration = "G Smith GSM 17H", PurchasePrice = 200, SalePrice = 240, IsForSale = false }  // Reserved plate
    };

            await _context.Plates.AddRangeAsync(plates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _plateHelper.GetPlatesAsync(1, 10, isForSale: false);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, plate => Assert.False(plate.IsForSale));
        }

        [Fact]
        public async Task MarkPlateAsSoldAsync_ShouldMarkPlateAsSold()
        {
            // Arrange
            var plate = new Plate
            {
                Id = Guid.NewGuid(),
                Registration = "ABC123",
                PurchasePrice = 100,
                SalePrice = 120,
                IsForSale = true // Initially not sold
            };

            await _context.Plates.AddAsync(plate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _plateHelper.MarkPlateAsSoldAsync(plate.Id);

            // Assert
            Assert.True(result); // Ensure the method returns true indicating the plate was marked as sold
            var updatedPlate = await _context.Plates.FirstOrDefaultAsync(p => p.Id == plate.Id);
            Assert.NotNull(updatedPlate);  // Ensure the plate still exists in the database
            Assert.False(updatedPlate.IsForSale);  // Ensure the IsSold property was updated to true
        }
    }


}
