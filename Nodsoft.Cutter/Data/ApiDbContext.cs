using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Nodsoft.Cutter.Data.Models;

namespace Nodsoft.Cutter.Data;

/// <summary>
/// The database context for the Cutter API.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class CutterDbContext(DbContextOptions<CutterDbContext> options) : DbContext(options)
{
    /// <summary>
    /// The set of cutter links in the database.
    /// </summary>
    public DbSet<Link> Links { get; }
}