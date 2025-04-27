using System.Collections.Generic;

namespace Jellyfin.Api.Models.Torrent;

/// <summary>
/// Represents library information with directory paths.
/// </summary>
public class LibraryInfo
{
    /// <summary>
    /// Gets or sets the name of the library.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the list of directory paths for this library.
    /// </summary>
    public List<string>? Paths { get; set; }
}
