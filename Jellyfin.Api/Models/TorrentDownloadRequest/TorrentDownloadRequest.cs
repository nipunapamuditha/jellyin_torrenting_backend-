using System;

namespace Jellyfin.Api.Models.TorrentDownloadRequest
{
    /// <summary>
    /// Request model for torrent download.
    /// </summary>
public class TorrentDownloadRequest
{
    /// <summary>
    /// Gets or sets the torrent info hash.
    /// </summary>
    public string? Info_hash { get; set; }

    /// <summary>
    /// Gets or sets the torrent name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the save path for downloaded files.
    /// </summary>
    public string? SavePath { get; set; }
}
}
