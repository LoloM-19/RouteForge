
using RouteForge.Api.Models;
using RouteForge.Api.Services;

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
app.MapPost("/api/locations", (DeliveryLocation location, DeliveryLocationService service) =>
{
    if (string.IsNullOrWhiteSpace(location.Name))
    {
        return Results.BadRequest("Location name is required.");
    }

    if (!double.IsFinite(location.Latitude) ||
        location.Latitude < -90 ||
        location.Latitude > 90)
    {
        return Results.BadRequest("Latitude must be between -90 and 90.");
    }

    if (!double.IsFinite(location.Longitude) ||
        location.Longitude < -180 ||
        location.Longitude > 180)
    {
        return Results.BadRequest("Longitude must be between -180 and 180.");
    }

    var createdLocation = service.Create(location);

    return Results.Created(
        $"/api/locations/{createdLocation.Id}",
        createdLocation);
});

app.Run();