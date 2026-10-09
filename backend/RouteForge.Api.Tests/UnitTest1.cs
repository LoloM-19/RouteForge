using RouteForge.Api.Services;

namespace RouteForge.Api.Tests;

public class DeliveryLocationServiceTests
{
    [Fact]
    public void GetAll_ReturnsTwoInitialLocations()
    {
        // Arrange
        var service = new DeliveryLocationService();

        // Act
        var locations = service.GetAll();

        // Assert
        Assert.Equal(2, locations.Count);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsLocation()
    {
        // Arrange
        var service = new DeliveryLocationService();

        // Act
        var location = service.GetById(1);

        // Assert
        Assert.NotNull(location);
        Assert.Equal("Johannesburg Warehouse", location.Name);
    }

    [Fact]
    public void GetById_NonexistentId_ReturnsNull()
    {
        // Arrange
        var service = new DeliveryLocationService();

        // Act
        var location = service.GetById(999);

        // Assert
        Assert.Null(location);
    }

    [Fact]
    public void Create_AssignsIdAndStoresLocation()
    {
        // Arrange
        var service = new DeliveryLocationService();

        var location = new RouteForge.Api.Models.DeliveryLocation
        {
            Name = "Pretoria Distribution Centre",
            Latitude = -25.7479,
            Longitude = 28.2293
        };

        // Act
        var createdLocation = service.Create(location);

        // Assert
        Assert.Equal(3, createdLocation.Id);
        Assert.Equal("Pretoria Distribution Centre", createdLocation.Name);
        Assert.Contains(createdLocation, service.GetAll());
    }
}