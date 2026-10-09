
using RouteForge.Api.Models;
using RouteForge.Api.Services;
using RouteForge.Api.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Register application services.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<DeliveryLocationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// GET: Retrieve all delivery locations.
app.MapGet("/api/locations", (DeliveryLocationService service) =>
{
    return Results.Ok(service.GetAll());
});

// GET: Retrieve a single location by ID.
app.MapGet("/api/locations/{id:int}", (int id, DeliveryLocationService service) =>
{
    var location = service.GetById(id);

    return location is null
        ? Results.NotFound()
        : Results.Ok(location);
});

// POST: Create a delivery location.
app.MapPost("/api/locations", (
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

    var createdLocation = service.Create(location);

    return Results.Created(
        $"/api/locations/{createdLocation.Id}",
        createdLocation);
});

app.Run();

public partial class Program { }