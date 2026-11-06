
using Microsoft.EntityFrameworkCore;
using RouteForge.Api.Data;
using RouteForge.Api.DTOs;
using RouteForge.Api.Models;
using RouteForge.Api.Services;
using RouteForge.Api.Optimization;
using RouteForge.Api.Enums;

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

builder.Services.AddHttpClient<AddressSearchService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);

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

app.MapGet("/api/warehouses", async (RouteForgeDbContext dbContext) =>
{
    var warehouses = await dbContext.DeliveryLocations
        .Where(location => location.Type == LocationType.Warehouse)
        .ToListAsync();

    return Results.Ok(warehouses);
});

app.MapPost("/api/warehouses", async (
    CreateDeliveryLocationRequest request,
    RouteForgeDbContext dbContext) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Warehouse name is required.");
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

    var warehouse = new DeliveryLocation
    {
        Name = request.Name.Trim(),
        Latitude = request.Latitude,
        Longitude = request.Longitude,
        Type = LocationType.Warehouse
    };

    dbContext.DeliveryLocations.Add(warehouse);
    await dbContext.SaveChangesAsync();

    return Results.Created(
        $"/api/warehouses/{warehouse.Id}",
        warehouse);
});

app.MapPut("/api/warehouses/{id:int}", async (
    int id,
    CreateDeliveryLocationRequest request,
    RouteForgeDbContext dbContext) =>
{
    var warehouse = await dbContext.DeliveryLocations
        .FirstOrDefaultAsync(location =>
            location.Id == id &&
            location.Type == LocationType.Warehouse);

    if (warehouse is null)
    {
        return Results.NotFound("Warehouse not found.");
    }

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Warehouse name is required.");
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

    warehouse.Name = request.Name.Trim();
    warehouse.Latitude = request.Latitude;
    warehouse.Longitude = request.Longitude;

    await dbContext.SaveChangesAsync();

    return Results.Ok(warehouse);
});

app.MapDelete("/api/warehouses/{id:int}", async (
    int id,
    RouteForgeDbContext dbContext) =>
{
    var warehouse = await dbContext.DeliveryLocations
        .FirstOrDefaultAsync(location =>
            location.Id == id &&
            location.Type == LocationType.Warehouse);

    if (warehouse is null)
    {
        return Results.NotFound("Warehouse not found.");
    }

    dbContext.DeliveryLocations.Remove(warehouse);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
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
    RouteOptimizer optimizer,
    TwoOptImprover twoOptImprover,
    OsrmRoutingService osrmRoutingService,
    CancellationToken cancellationToken) =>
{
    if (request.Stops is null || request.Stops.Count == 0)
    {
        return Results.BadRequest(
            "Please provide at least one delivery stop.");
    }

    if (request.Stops.Any(stop =>
        string.IsNullOrWhiteSpace(stop.Name) ||
        !double.IsFinite(stop.Latitude) ||
        !double.IsFinite(stop.Longitude) ||
        stop.Latitude is < -90 or > 90 ||
        stop.Longitude is < -180 or > 180))
    {
        return Results.BadRequest(
            "Each delivery stop must have a name and valid coordinates.");
    }

    if (string.IsNullOrWhiteSpace(request.StartName) ||
        !double.IsFinite(request.StartLatitude) ||
        !double.IsFinite(request.StartLongitude) ||
        request.StartLatitude is < -90 or > 90 ||
        request.StartLongitude is < -180 or > 180)
    {
        return Results.BadRequest(
            "A valid starting location is required.");
    }

    if (string.IsNullOrWhiteSpace(request.EndName) ||
        !double.IsFinite(request.EndLatitude) ||
        !double.IsFinite(request.EndLongitude) ||
        request.EndLatitude is < -90 or > 90 ||
        request.EndLongitude is < -180 or > 180)
    {
        return Results.BadRequest(
            "A valid finishing location is required.");
    }

    if (request.Stops.Count > 25)
    {
        return Results.BadRequest(
            "A maximum of 25 delivery stops is supported.");
    }

    var start = new DeliveryLocation
    {
        Id = 1,
        Name = request.StartName.Trim(),
        Latitude = request.StartLatitude,
        Longitude = request.StartLongitude,
        Type = LocationType.Warehouse
    };

    var stops = request.Stops.Select((stop, index) =>
        new DeliveryLocation
        {
            Id = index + 2,
            Name = stop.Name.Trim(),
            Latitude = stop.Latitude,
            Longitude = stop.Longitude,
            Type = LocationType.Delivery
        }).ToList();

    var end = new DeliveryLocation
    {
        Id = stops.Count + 2,
        Name = request.EndName.Trim(),
        Latitude = request.EndLatitude,
        Longitude = request.EndLongitude,
        Type = LocationType.Delivery
    };

    var locations = new List<DeliveryLocation> { start };
    locations.AddRange(stops);
    locations.Add(end);

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
            detail: "Could not contact the OSRM routing service.");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"OSRM distance error: {ex}");

        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Road distances unavailable",
            detail: "OSRM could not calculate driving distances for these locations.");
    }

    var locationIndices = locations
        .Select((location, index) => new
        {
            location.Id,
            Index = index
        })
        .ToDictionary(item => item.Id, item => item.Index);

    double GetRoadDistanceKm(
        DeliveryLocation from,
        DeliveryLocation to)
    {
        return distanceMatrix[
            locationIndices[from.Id],
            locationIndices[to.Id]];
    }

    // Nearest Neighbour chooses the order of the delivery stops.
   var result = optimizer.OptimizeRoute(
        start,
        stops,
        end,
        GetRoadDistanceKm);

    result.NearestNeighbourDistanceKm = result.TotalDistanceKm;

    // 2-opt improves the stop order.
    result.OptimizedRoute = twoOptImprover.ImproveRoute(
        start,
        result.OptimizedRoute,
        end,
        GetRoadDistanceKm);

    // Calculate the final journey, ending at the selected destination.
    double totalDistanceKm = 0;
    var currentLocation = start;

    foreach (var stop in result.OptimizedRoute)
    {
        totalDistanceKm += GetRoadDistanceKm(currentLocation, stop);
        currentLocation = stop;
    }

    totalDistanceKm += GetRoadDistanceKm(currentLocation, end);

    result.TotalDistanceKm = totalDistanceKm;

    // Include the custom finishing point in the returned route.
    var optimizedLocations = new List<DeliveryLocation> { start };
    optimizedLocations.AddRange(result.OptimizedRoute);
    optimizedLocations.Add(end);

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
            detail: "OSRM could not return the road geometry.");
    }

    return Results.Ok(result);
});


app.MapGet("/api/addresses/search",
    async (
        string q,
        AddressSearchService addressSearchService,
        CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3)
        {
            return Results.Ok(new List<AddressSuggestion>());
        }

        try
        {
            var suggestions = await addressSearchService.SearchAsync(
                q.Trim(),
                cancellationToken);

            return Results.Ok(suggestions);
        }
        catch (HttpRequestException)
        {
            return Results.Problem(
                title: "Address search unavailable",
                detail: "The address provider could not be reached.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (TaskCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return Results.Problem(
                title: "Address search timed out",
                detail: "Please try searching again.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    });

app.Run();

public partial class Program { }