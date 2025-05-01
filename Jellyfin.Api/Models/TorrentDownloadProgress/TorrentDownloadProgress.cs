using System;

namespace Jellyfin.Api.Models.TorrentDownloadProgress
{
    /// <summary>
    /// Progress information for a torrent download.
    /// </summary>
    public class TorrentDownloadProgress
    {
        /// <summary>
        /// Gets or sets the info hash of the torrent.
        /// </summary>
        public required string Info_hash { get; set; }

        /// <summary>
        /// Gets or sets the name of the torrent.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the progress percentage (0-100).
        /// </summary>
        public double ProgressPercentage { get; set; }

        /// <summary>
        /// Gets or sets the download speed in bytes per second.
        /// </summary>
        public long DownloadSpeed { get; set; }

        /// <summary>
        /// Gets or sets the total downloaded bytes.
        /// </summary>
        public long DownloadedBytes { get; set; }

        /// <summary>
        /// Gets or sets the total size in bytes.
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// Gets or sets the status of the download.
        /// </summary>
        public required string Status { get; set; }
    }
}
