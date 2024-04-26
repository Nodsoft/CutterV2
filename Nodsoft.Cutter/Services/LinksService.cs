using Microsoft.EntityFrameworkCore;
using Nodsoft.Cutter.Data;
using Nodsoft.Cutter.Data.Models;

namespace Nodsoft.Cutter.Services;

/// <summary>
/// Provides functionality for managing shortened links.
/// </summary>
public sealed class LinksService
{
    private readonly CutterDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinksService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context for the Cutter API.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor for the current request.</param>
    public LinksService(CutterDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    
    /// <summary>
    /// Gets a link by its fragment.
    /// </summary>
    /// <param name="name">The fragment of the link to get.</param>
    /// <returns>The link with the specified fragment, or <see langword="null"/> if no link was found.</returns>
    public async ValueTask<Link?> GetLinkAsync(string name) => await _dbContext.Links.FirstOrDefaultAsync(l => l.Name == name);
    
    /// <summary>
    /// Determines if a specified link is owned by a specified user.
    /// </summary>
    /// <param name="linkId">The identifier of the link to check ownership of.</param>
    /// <param name="uid">The identifier of the user to check ownership for.</param>
    /// <returns><see langword="true"/> if the user owns the link; otherwise <see langword="false"/>.</returns>
    public async ValueTask<bool> IsLinkOwnedByUserAsync(Guid linkId, uint uid) 
        => await _dbContext.Links.AsNoTracking().AnyAsync(l => l.Id == linkId && l.CreatedBy.Id == uid);

    /// <summary>
    /// Inserts a new link into the database.
    /// </summary>
    /// <param name="name">The fragment of the link to insert.</param>
    /// <param name="destination">The destination URL of the link to insert.</param>
    /// <returns>The newly inserted link.</returns>
    public async ValueTask<Link> InsertLinkAsync(string? name, string destination)
    {
        Link link = new()
        {
            Id = Guid.NewGuid(),
            Name = name ?? Base62Generator.GenerateString(6),
            Destination = destination,
            CreatedBy = await _dbContext.Users.FindAsync(1) ?? throw new InvalidOperationException("The legacy user is missing."),
            CreatedFromIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress ?? throw new InvalidOperationException("The remote IP address is missing.")
        };
        
        _dbContext.Links.Add(link);
        await _dbContext.SaveChangesAsync();
        return link;
    }
    
    /// <summary>
    /// Disables a link in the database.
    /// </summary>
    /// <param name="name">The fragment of the link to disable.</param>
    /// <returns>The disabled link.</returns>
    public async ValueTask<Link> DisableLinkAsync(string name)
    {
        Link link = await GetLinkAsync(name) ?? throw new InvalidOperationException("The link does not exist.");
        link.IsBlocked = true;
        await _dbContext.SaveChangesAsync();
        return link;
    }
}