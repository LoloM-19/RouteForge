
using Microsoft.EntityFrameworkCore;
using RouteForge.Api.Data;
using RouteForge.Api.Models;

namespace RouteForge.Api.Services;

public class DeliveryLocationService
{
    private readonly RouteForgeDbContext _context;

    public DeliveryLocationService(RouteForgeDbContext context)
    {
        _context = context;
    }

    public async Task<List<DeliveryLocation>> GetAllAsync()
    {
        return await _context.DeliveryLocations
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<DeliveryLocation?> GetByIdAsync(int id)
    {
        return await _context.DeliveryLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(location => location.Id == id);
    }

    public async Task<DeliveryLocation> CreateAsync(
        DeliveryLocation location)
    {
        _context.DeliveryLocations.Add(location);

        await _context.SaveChangesAsync();

        return location;
    }

        
    public async Task<DeliveryLocation?> UpdateAsync(
        int id,
        DeliveryLocation updatedLocation)
    {
        var existingLocation = await _context.DeliveryLocations
            .FirstOrDefaultAsync(location => location.Id == id);

        if (existingLocation is null)
        {
            return null;
        }

        existingLocation.Name = updatedLocation.Name;
        existingLocation.Latitude = updatedLocation.Latitude;
        existingLocation.Longitude = updatedLocation.Longitude;

        await _context.SaveChangesAsync();

        return existingLocation;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var location = await _context.DeliveryLocations
            .FirstOrDefaultAsync(location => location.Id == id);

        if (location is null)
        {
            return false;
        }

        _context.DeliveryLocations.Remove(location);

        await _context.SaveChangesAsync();

        return true;
    }
}