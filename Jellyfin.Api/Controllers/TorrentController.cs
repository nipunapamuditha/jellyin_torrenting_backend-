using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Api.Models.Torrent;
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
}
