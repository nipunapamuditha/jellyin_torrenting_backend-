using System.Collections.Generic;

namespace Jellyfin.Api.Models.TorrentSearchResult;

/// <summary>
/// Represents a torrent search result from the API.
/// </summary>
public class TorrentSearchResult
{
    /// <summary>
    /// Gets or sets the ID of the torrent.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the torrent.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the info hash of the torrent.
    /// </summary>
    public string Info_hash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of leechers.
    /// </summary>
    public string Leechers { get; set; } = string.Empty; // API returns string

    /// <summary>
    /// Gets or sets the number of seeders.
    /// </summary>
    public string Seeders { get; set; } = string.Empty; // API returns string

    /// <summary>
    /// Gets or sets the number of files in the torrent.
    /// </summary>
    public string Num_files { get; set; } = string.Empty; // API returns string

    /// <summary>
    /// Gets or sets the size of the torrent in bytes.
    /// </summary>
    public string Size { get; set; } = string.Empty; // API returns string

    /// <summary>
    /// Gets or sets the username of the uploader.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the torrent was added.
    /// </summary>
    public string Added { get; set; } = string.Empty; // API returns string (Unix timestamp)

    /// <summary>
    /// Gets or sets the status of the uploader (e.g., vip).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category ID.
    /// </summary>
    public string Category { get; set; } = string.Empty; // API returns string

    /// <summary>
    /// Gets or sets the IMDB ID, if available.
    /// </summary>
    public string Imdb { get; set; } = string.Empty;
}
