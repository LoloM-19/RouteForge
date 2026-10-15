
using Microsoft.EntityFrameworkCore;
using RouteForge.Api.Data;
using RouteForge.Api.DTOs;
using RouteForge.Api.Models;
using RouteForge.Api.Services;
using RouteForge.Api.Optimization;
using RouteForge.Api.Services;

var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowRouteForgeFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Register application services.
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("RouteForge");

builder.Services.AddDbContext<RouteForgeDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<DeliveryLocationService>();
builder.Services.AddScoped<RouteOptimizer>();
builder.Services.AddScoped<TwoOptImprover>();

builder.Services.AddHttpClient<OsrmRoutingService>(client =>
{
    client.BaseAddress = new Uri("https://router.project-osrm.org/");
    client.Timeout = TimeSpan.FromSeconds(20);

    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "RouteForge/1.0 (route optimization portfolio project)");
});

var app = builder.Build();
app.UseCors("AllowRouteForgeFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// GET: Retrieve all delivery locations.
app.MapGet("/api/locations", async (DeliveryLocationService service) =>
{
    var locations = await service.GetAllAsync();

    return Results.Ok(locations);
});

// GET: Retrieve a single location by ID.
app.MapGet("/api/locations/{id:int}", async (
    int id,
    DeliveryLocationService service) =>
{
    var location = await service.GetByIdAsync(id);

    return location is null
        ? Results.NotFound()
        : Results.Ok(location);
});

// POST: Create a delivery location.
app.MapPost("/api/locations", async (
    CreateDeliveryLocationRequest request,
    DeliveryLocationService service) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Location name is required.");
    }

    if (!double.IsFinite(request.Latitude) ||
        request.Latitude < -90 ||
        request.Latitude > 90)
    {
        return Results.BadRequest("Latitude must be between -90 and 90.");
    }

    if (!double.IsFinite(request.Longitude) ||
        request.Longitude < -180 ||
        request.Longitude > 180)
    {
        return Results.BadRequest("Longitude must be between -180 and 180.");
    }

    var location = new DeliveryLocation
    {
        Name = request.Name.Trim(),
        Latitude = request.Latitude,
        Longitude = request.Longitude
    };

    var createdLocation = await service.CreateAsync(location);

    return Results.Created(
        $"/api/locations/{createdLocation.Id}",
        createdLocation);
});

 // PUT: Update an existing delivery location.
app.MapPut("/api/locations/{id:int}", async (
    int id,
    CreateDeliveryLocationRequest request,
    DeliveryLocationService service) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Location name is required.");
    }

    if (!double.IsFinite(request.Latitude) ||
        request.Latitude < -90 ||
        request.Latitude > 90)
    {
        return Results.BadRequest("Latitude must be between -90 and 90.");
    }

    if (!double.IsFinite(request.Longitude) ||
        request.Longitude < -180 ||
        request.Longitude > 180)
    {
        return Results.BadRequest("Longitude must be between -180 and 180.");
    }

    var updatedLocation = new DeliveryLocation
    {
        Name = request.Name.Trim(),
        Latitude = request.Latitude,
        Longitude = request.Longitude
    };

    var location = await service.UpdateAsync(id, updatedLocation);

    return location is null
        ? Results.NotFound()
        : Results.Ok(location);
});

// DELETE: Remove a delivery location.
app.MapDelete("/api/locations/{id:int}", async (
    int id,
    DeliveryLocationService service) =>
{
    var deleted = await service.DeleteAsync(id);

    return deleted
        ? Results.NoContent()
        : Results.NotFound();
});

// POST: Optimize a delivery route.
app.MapPost("/api/routes/optimize", async (
    OptimizeRouteRequest request,
    RouteForgeDbContext dbContext,
    RouteOptimizer optimizer,
    TwoOptImprover twoOptImprover,
    OsrmRoutingService osrmRoutingService,
    CancellationToken cancellationToken) =>
{
    if (request.WarehouseId <= 0)
    {
        return Results.BadRequest("A valid warehouse ID is required.");
    }

    if (request.DeliveryLocationIds is null)
    {
        return Results.BadRequest("Delivery location IDs are required.");
    }

    if (request.DeliveryLocationIds.Any(id => id <= 0))
    {
        return Results.BadRequest(
            "Delivery location IDs must be positive.");
    }

    if (request.DeliveryLocationIds.Contains(request.WarehouseId))
    {
        return Results.BadRequest(
            "The warehouse cannot also be a delivery stop.");
    }

    if (request.DeliveryLocationIds.Distinct().Count()
        != request.DeliveryLocationIds.Count)
    {
        return Results.BadRequest(
            "Duplicate delivery location IDs are not allowed.");
    }

    var warehouse = await dbContext.DeliveryLocations
        .AsNoTracking()
        .FirstOrDefaultAsync(location =>
            location.Id == request.WarehouseId);

    if (warehouse is null)
    {
        return Results.NotFound("Warehouse not found.");
    }

    var deliveryStops = await dbContext.DeliveryLocations
        .AsNoTracking()
        .Where(location =>
            request.DeliveryLocationIds.Contains(location.Id))
        .ToListAsync();

    if (deliveryStops.Count != request.DeliveryLocationIds.Count)
    {
        return Results.NotFound(
            "One or more delivery locations were not found.");
    }

    // Preserve the exact order requested by the client.
    var stopsById = deliveryStops.ToDictionary(location => location.Id);

    var originalOrder = request.DeliveryLocationIds
        .Select(id => stopsById[id])
        .ToList();

    // Build the list used to request the OSRM driving-distance matrix.
    var locations = new List<DeliveryLocation> { warehouse };
    locations.AddRange(originalOrder);

    double[,] distanceMatrix;

    try
    {
        distanceMatrix = await osrmRoutingService.GetDistanceMatrixAsync(
            locations,
            cancellationToken);
    }
    
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"OSRM HTTP error: {ex}");

        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Road routing service unavailable",
            detail: "Could not contact the OSRM routing service. Please check the API console logs.");
    }

    catch (InvalidOperationException)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Road distances unavailable",
            detail: "OSRM could not calculate a complete driving-distance matrix for these locations.");
    }

    // Map each database ID to its position in the OSRM matrix.
    var locationIndices = locations
        .Select((location, index) => new
        {
            location.Id,
            Index = index
        })
        .ToDictionary(item => item.Id, item => item.Index);

    // Every algorithm will now use driving distances in kilometres.
    double GetRoadDistanceKm(
        DeliveryLocation from,
        DeliveryLocation to)
    {
        return distanceMatrix[
            locationIndices[from.Id],
            locationIndices[to.Id]];
    }

    // Run Nearest Neighbour using road distances.
    var result = optimizer.OptimizeRoute(
        warehouse,
        originalOrder,
        GetRoadDistanceKm);

    // Preserve the distance before 2-opt improvement.
    result.NearestNeighbourDistanceKm = result.TotalDistanceKm;

    // Improve the route using the same road distances.
    result.OptimizedRoute = twoOptImprover.ImproveRoute(
        warehouse,
        result.OptimizedRoute,
        GetRoadDistanceKm);

    // Recalculate the final optimized round-trip distance.
    double totalDistanceKm = 0;
    var currentLocation = warehouse;

    foreach (var stop in result.OptimizedRoute)
    {
        totalDistanceKm += GetRoadDistanceKm(currentLocation, stop);
        currentLocation = stop;
    }

    // Include the return journey to the warehouse.
    totalDistanceKm += GetRoadDistanceKm(
        currentLocation,
        warehouse);

   
result.TotalDistanceKm = totalDistanceKm;

var optimizedLocations = new List<DeliveryLocation>
{
    warehouse
};

optimizedLocations.AddRange(result.OptimizedRoute);
optimizedLocations.Add(warehouse);

try
{
    result.RouteGeometry =
        await osrmRoutingService.GetRouteGeometryAsync(
            optimizedLocations,
            cancellationToken);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"OSRM route geometry error: {ex}");

    return Results.Problem(
        statusCode: StatusCodes.Status503ServiceUnavailable,
        title: "Road geometry unavailable",
        detail: "The route was optimized, but its road geometry could not be retrieved.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"OSRM route geometry error: {ex}");

    return Results.Problem(
        statusCode: StatusCodes.Status503ServiceUnavailable,
        title: "Road geometry unavailable",
        detail: "The route was optimized, but OSRM could not return its road geometry.");
}

return Results.Ok(result);

});

app.Run();

public partial class Program { }