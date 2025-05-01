using System;

namespace Jellyfin.Api.Models.TorrentDownloadAuth
{
    /// <summary>
    /// Authentication response containing torrent server session information.
    /// </summary>
    public class AuthenticationResponse
    {
        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        public required string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the expiration time for the session.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
