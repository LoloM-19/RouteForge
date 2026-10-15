
using Microsoft.EntityFrameworkCore;
using RouteForge.Api.Data;
using RouteForge.Api.Models;
using RouteForge.Api.Services;

namespace RouteForge.Api.Tests;

public class DeliveryLocationServiceTests
{
    private static (
        RouteForgeDbContext Context,
        DeliveryLocationService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<RouteForgeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RouteForgeDbContext(options);

        context.DeliveryLocations.AddRange(
            new DeliveryLocation
            {
                Id = 1,
                Name = "Johannesburg Warehouse",
                Latitude = -26.2041,
                Longitude = 28.0473
            },
            new DeliveryLocation
            {
                Id = 2,
                Name = "Sandton Delivery Hub",
                Latitude = -26.1076,
                Longitude = 28.0567
            });

        context.SaveChanges();

        return (context, new DeliveryLocationService(context));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTwoInitialLocations()
    {
        var (context, service) = CreateService();
        using (context)
        {
            var locations = await service.GetAllAsync();

            Assert.Equal(2, locations.Count);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsLocation()
    {
        var (context, service) = CreateService();
        using (context)
        {
            var location = await service.GetByIdAsync(1);

            Assert.NotNull(location);
            Assert.Equal("Johannesburg Warehouse", location.Name);
        }
    }

    [Fact]
    public async Task GetByIdAsync_NonexistentId_ReturnsNull()
    {
        var (context, service) = CreateService();
        using (context)
        {
            var location = await service.GetByIdAsync(999);

            Assert.Null(location);
        }
    }

    [Fact]
    public async Task CreateAsync_SavesLocationAndAssignsId()
    {
        var (context, service) = CreateService();
        using (context)
        {
            var location = new DeliveryLocation
            {
                Name = "Pretoria Distribution Centre",
                Latitude = -25.7479,
                Longitude = 28.2293
            };

            var createdLocation = await service.CreateAsync(location);

            Assert.True(createdLocation.Id > 0);
            Assert.Equal(
                "Pretoria Distribution Centre",
                createdLocation.Name);

            Assert.Equal(
                3,
                await context.DeliveryLocations.CountAsync());
        }
    }
}