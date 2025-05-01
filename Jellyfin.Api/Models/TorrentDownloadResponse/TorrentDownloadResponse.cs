using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Api.Models.Torrent;
using Jellyfin.Api.Models.TorrentDownloadProgress;
using Jellyfin.Api.Models.TorrentDownloadRequest;
using Jellyfin.Api.Models.TorrentSearchResult;
using MediaBrowser.Common.Api;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Api.Models.TorrentDownloadResponse;

/// <summary>
/// Represents the response for a torrent download request.
/// </summary>
public class TorrentDownloadResponse
{
    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the torrent id/hash.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the save path for downloaded files.
    /// </summary>
    public string? SavePath { get; set; }

    /// <summary>
    /// Gets or sets the server response.
    /// </summary>
    public string? ServerResponse { get; set; }
}
