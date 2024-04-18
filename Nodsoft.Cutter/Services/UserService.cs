using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nodsoft.Cutter.Data;
using Nodsoft.Cutter.Data.Models;
using OpenIddict.Abstractions;

namespace Nodsoft.Cutter.Services;

/// <summary>
/// Provides functionality for managing users.
/// </summary>
public sealed class UserService
{
    private readonly CutterDbContext _dbContext;

    public UserService(CutterDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to get.</param>
    /// <returns>The user with the specified identifier, or <see langword="null"/> if no user was found.</returns>
    public async ValueTask<User?> GetUserAsync(uint id) => await _dbContext.Users.FindAsync(id);
    
    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    /// <returns>The user with the specified username, or <see langword="null"/> if no user was found.</returns>
    public async ValueTask<User?> GetUserAsync(string username) => await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

    /// <summary>
    /// Inserts or refreshes a user in the database.
    /// </summary>
    /// <param name="authUser">The user to insert or refresh.</param>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    public async ValueTask<User> RefreshUserAsync(ClaimsPrincipal authUser, HttpContext httpContext)
    {
        uint id = uint.Parse(authUser.GetClaim(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("The user ID claim is missing."));
        
        if (await GetUserAsync(id) is not { } user)
        {
            user = new()
            {
                Id = id,
                Username = authUser.GetClaim(ClaimTypes.Name) ?? throw new InvalidOperationException("The username claim is missing.")
            };
            
            _dbContext.Users.Add(user);
        }
        
        user.MapFromClaims(authUser.Claims);
        user.IpAddresses.Add(httpContext.Connection.RemoteIpAddress ?? throw new InvalidOperationException("The remote IP address is missing."));
        
        await _dbContext.SaveChangesAsync();
        return user;
    }
}