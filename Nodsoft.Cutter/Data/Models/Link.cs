using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Nodsoft.Cutter.Data.Models;

/// <summary>
/// Represents a link that can be used to redirect to a destination URL.
/// </summary>
[Index(nameof(Name), IsUnique = true)]
[Description("Represents a link that can be used to redirect to a destination URL.")]
public class Link
{
    /// <summary>
    /// The unique identifier for the link.
    /// </summary>
    [Key, Description("The unique identifier for the link.")]
    public Guid Id { get; init; }
    
    /// <summary>
    /// The fragment of the URL that the link is accessed by.
    /// </summary>
    [Required, RegularExpression(@"^[a-zA-Z\d-_]+$"), Description("The fragment of the URL that the link is accessed by.")]
    public string Name { get; init; }

    /// <summary>
    /// The destination URL of the redirect.
    /// </summary>
    [Required, Url, Description("The destination URL of the redirect.")]
    public string Destination { get; init; }

    /// <summary>
    /// The date and time the link was created.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity), Description("The date and time the link was created.")]
    public DateTimeOffset CreatedAt { get; init; }

    [Required, Description("The IP address the link was created from.")]
    public IPAddress CreatedFromIp { get; init; }
}