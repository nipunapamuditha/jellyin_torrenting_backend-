using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Models.Torrent;
using Jellyfin.Api.Models.TorrentDownloadProgress;
using Jellyfin.Api.Models.TorrentDownloadRequest;
using Jellyfin.Api.Models.TorrentDownloadResponse;
using Jellyfin.Api.Models.TorrentSearchResult;
using MediaBrowser.Common.Api;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Api.Controllers;

/// <summary>
/// Admin test controller.
/// </summary>
[Route("torrent")]
[Authorize(Policy = Policies.RequiresElevation)]
#pragma warning disable SA1402 // File may only contain a single type
public class TorrentController : BaseJellyfinApiController
#pragma warning restore SA1402 // File may only contain a single type
{
    private readonly ILibraryManager _libraryManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _torrentServerUrl = "http://localhost:8099";

    /// <summary>
    /// Initializes a new instance of the <see cref="TorrentController"/> class.
    /// </summary>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
    public TorrentController(ILibraryManager libraryManager, IHttpClientFactory httpClientFactory)
    {
        _libraryManager = libraryManager;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Tests administrative access.
    /// </summary>
    /// <response code="200">Admin access test successful.</response>
    /// <returns>A success message indicating the test passed.</returns>
    [HttpGet("Test1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string> TestAdminAccess()
    {
        return Ok("test 1 success");
    }

    /// <summary>
    /// Gets all libraries and their directory information.
    /// </summary>
    /// <response code="200">Libraries retrieved successfully.</response>
    /// <returns>A list of libraries with their directory paths.</returns>
    [HttpGet("libraries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<LibraryInfo>> GetLibraries()
    {
        var libraries = _libraryManager.GetVirtualFolders().Select(folder => new LibraryInfo
        {
            Name = folder.Name,
            Paths = folder.Locations.ToList()
        }).ToList();

        return Ok((IEnumerable<LibraryInfo>)libraries);
    }

    /// <summary>
    /// Searches for torrents using an external API.
    /// </summary>
    /// <param name="q">The search query string.</param>
    /// <param name="cat">The category ID (optional, defaults to "0").</param>
    /// <response code="200">Torrents retrieved successfully.</response>
    /// <response code="400">Search query is empty.</response>
    /// <response code="500">Error searching for torrents.</response>
    /// <returns>A list of torrent search results, limited to the first 20.</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TorrentSearchResult>>> SearchTorrents([FromQuery] string q, [FromQuery] string cat = "0")
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Search query cannot be empty.");
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient("TorrentApi");
            var response = await httpClient.GetStringAsync($"https://apibay.org/q.php?q={Uri.EscapeDataString(q)}&cat={cat}");

#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances

            var searchResults = JsonSerializer.Deserialize<List<TorrentSearchResult>>(response, options);

            // Return only the first 20 results
            return Ok(searchResults?.Take(20));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error searching for torrents: {ex.Message}");
        }
    }

/// <summary>
/// Starts downloading a torrent.
/// </summary>
/// <param name="downloadRequest">The torrent download request containing Name, Info_hash, and optional SavePath.</param>
/// <response code="200">Download started successfully.</response>
/// <response code="400">Invalid download request.</response>
/// <response code="500">Error starting the download.</response>
/// <returns>A result indicating success or failure of starting the download.</returns>
    [HttpPost("download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TorrentDownloadResponse>> StartTorrentDownload([FromBody] TorrentDownloadRequest downloadRequest)
{
    if (downloadRequest == null || string.IsNullOrEmpty(downloadRequest.Info_hash))
    {
        return BadRequest("Invalid download request. Info_hash is required.");
    }

    try
    {
        // Create magnet link from info_hash
        string magnetLink = $"magnet:?xt=urn:btih:{downloadRequest.Info_hash}";
        if (!string.IsNullOrEmpty(downloadRequest.Name))
        {
            magnetLink += $"&dn={Uri.EscapeDataString(downloadRequest.Name)}";
        }

        // Send torrent to server
        var httpClient = _httpClientFactory.CreateClient();

        // Create multipart form content with both URLs and savepath if provided
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("urls", magnetLink)
        };

        // Add save path if provided
        if (!string.IsNullOrWhiteSpace(downloadRequest.SavePath))
        {
            formData.Add(new KeyValuePair<string, string>("savepath", downloadRequest.SavePath));
        }

        var formContent = new FormUrlEncodedContent(formData);

        var response = await httpClient.PostAsync($"{_torrentServerUrl}/api/v2/torrents/add", formContent);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();

        return Ok(new TorrentDownloadResponse
        {
            Message = $"Started downloading: {downloadRequest.Name}",
            Id = downloadRequest.Info_hash,
            SavePath = downloadRequest.SavePath,
            ServerResponse = responseContent
        });
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, $"Error starting torrent download: {ex.Message}");
    }
}

    /// <summary>
    /// Gets information about all torrents currently being processed by the torrent server.
    /// </summary>
    /// <response code="200">Torrent information retrieved successfully.</response>
    /// <response code="500">Error retrieving torrent information.</response>
    /// <returns>A list of torrent information and progress details.</returns>
    [HttpGet("progress")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<object>>> GetTorrentProgress()
    {
        try
        {
            // Query torrent server for progress
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetAsync($"{_torrentServerUrl}/api/v2/torrents/info?filter=all");
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse the JSON response
#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances

            var progressInfo = JsonSerializer.Deserialize<List<object>>(responseContent, options);

            return Ok((IEnumerable<object>)(progressInfo ?? new List<object>()));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving torrent progress: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets detailed information about all media libraries.
    /// </summary>
    /// <response code="200">Library information retrieved successfully.</response>
    /// <returns>A list of libraries with their names and directory paths.</returns>
    [HttpGet("libraries/paths")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetLibraryPaths()
    {
        try
        {
            var libraries = _libraryManager.GetVirtualFolders().Select(folder =>
            {
                Console.WriteLine($"Library: {folder.Name}, Paths: {string.Join(", ", folder.Locations)}");
                return new
                {
                    LibraryName = folder.Name,
                    FolderPaths = folder.Locations.ToList()
                };
            }).ToList();

            return Ok(libraries);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"Error retrieving library paths: {ex.Message}");
        }
    }

/// <summary>
/// Pauses a torrent download.
/// </summary>
/// <param name="hash">The hash of the torrent to pause.</param>
/// <response code="200">Torrent paused successfully.</response>
/// <response code="400">Hash parameter is missing.</response>
/// <response code="500">Error pausing the torrent.</response>
/// <returns>A result indicating success or failure of pausing the torrent.</returns>
    [HttpPost("pause")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> PauseTorrent([FromQuery] string hash)
{
    if (string.IsNullOrEmpty(hash))
    {
        return BadRequest("Hash parameter is required.");
    }

    try
    {
        var httpClient = _httpClientFactory.CreateClient();

        // Create form content with the hash in the body
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hashes", hash)
        });

        var response = await httpClient.PostAsync($"{_torrentServerUrl}/api/v2/torrents/stop", formContent);
        response.EnsureSuccessStatusCode();

        return Ok(new { success = true, message = "Torrent paused successfully" });
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, $"Error pausing torrent: {ex.Message}");
    }
}

/// <summary>
/// Resumes a paused torrent download.
/// </summary>
/// <param name="hash">The hash of the torrent to resume.</param>
/// <response code="200">Torrent resumed successfully.</response>
/// <response code="400">Hash parameter is missing.</response>
/// <response code="500">Error resuming the torrent.</response>
/// <returns>A result indicating success or failure of resuming the torrent.</returns>
    [HttpPost("resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> ResumeTorrent([FromQuery] string hash)
{
    if (string.IsNullOrEmpty(hash))
    {
        return BadRequest("Hash parameter is required.");
    }

    try
    {
        var httpClient = _httpClientFactory.CreateClient();

        // Create form content with the hash in the body
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hashes", hash)
        });

        var response = await httpClient.PostAsync($"{_torrentServerUrl}/api/v2/torrents/start", formContent);
        response.EnsureSuccessStatusCode();

        return Ok(new { success = true, message = "Torrent resumed successfully" });
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, $"Error resuming torrent: {ex.Message}");
    }
}

/// <summary>
/// Deletes a torrent.
/// </summary>
/// <param name="hash">The hash of the torrent to delete.</param>
/// <param name="deleteFiles">Whether to delete the torrent files (optional, defaults to false).</param>
/// <response code="200">Torrent deleted successfully.</response>
/// <response code="400">Hash parameter is missing.</response>
/// <response code="500">Error deleting the torrent.</response>
/// <returns>A result indicating success or failure of deleting the torrent.</returns>
    [HttpPost("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> DeleteTorrent([FromQuery] string hash, [FromQuery] bool deleteFiles = false)
{
    if (string.IsNullOrEmpty(hash))
    {
        return BadRequest("Hash parameter is required.");
    }

    try
    {
        var httpClient = _httpClientFactory.CreateClient();

        // Create form content with the hash in the body and deleteFiles parameter
        var formParams = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("hashes", hash),
            new KeyValuePair<string, string>("deleteFiles", deleteFiles.ToString().ToLowerInvariant())
        };

        var formContent = new FormUrlEncodedContent(formParams);

        var response = await httpClient.PostAsync($"{_torrentServerUrl}/api/v2/torrents/delete", formContent);
        response.EnsureSuccessStatusCode();

        return Ok(new { success = true, message = $"Torrent deleted successfully{(deleteFiles ? " with files" : string.Empty)}" });
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting torrent: {ex.Message}");
    }
}
}
