using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Nodsoft.Cutter.Data.Models;

/// <summary>
/// Represents a link that can be used to redirect to a destination URL.
/// </summary>
[Index(nameof(Name), IsUnique = true)]
public sealed class Link
{
    /// <summary>
    /// The unique identifier for the link.
    /// </summary>
    [Key]
    public Guid Id { get; init; }
    
    /// <summary>
    /// The fragment of the URL that the link is accessed by.
    /// </summary>
    [Required, RegularExpression(@"^[a-zA-Z\d-_]+$"), MaxLength(512)]
    public string Name { get; init; } = "";

    /// <summary>
    /// The destination URL of the redirect.
    /// </summary>
    [Required, Url]
    public string Destination { get; init; } = "";

    /// <summary>
    /// The date and time the link was created.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// The user that created the link.
    /// </summary>
    public User CreatedBy { get; init; } = null!;
    
    /// <summary>
    /// The IP address the link was created from.
    /// </summary>
    [Required]
    public IPAddress CreatedFromIp { get; init; } = IPAddress.None;
    
    /// <summary>
    /// Whether the link is enabled and can be used.
    /// </summary>
    /// <remarks>
    /// This is used to disable links without removing them from the database.
    /// </remarks>
    public bool IsBlocked { get; set; }
}