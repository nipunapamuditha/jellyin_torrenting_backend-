# Jellyfin Server - Torrenting Integration

This document outlines the torrenting functionalities integrated into this Jellyfin server instance. It allows users with administrative privileges to search for torrents, manage downloads via a qBittorrent backend, and interact with Jellyfin libraries.

## Prerequisites

*   **Jellyfin Server**: This custom version of Jellyfin with the torrenting controller.
*   **qBittorrent Instance**: A running qBittorrent instance accessible from the Jellyfin server.
    *   The integration is hardcoded to connect to qBittorrent at `http://qbittorrent:8099`. Ensure your qBittorrent service is named `qbittorrent` if running in Docker and accessible on port `8099`.
    *   The qBittorrent WebUI API must be enabled.

## Authentication

All torrent-related API endpoints require administrative privileges. Requests must be authenticated, and the user must have elevated permissions (as per `Policy = Policies.RequiresElevation`).

## API Endpoints

The base path for all torrenting API endpoints is `/torrent`.

---

### 1. Test Admin Access

*   **Endpoint**: `GET /torrent/Test1`
*   **Description**: A simple test endpoint to verify that the torrent controller is reachable and the user has administrative access.
*   **Response (200 OK)**:
    ```
    "test 1 success"
    ```

---

### 2. Get Jellyfin Libraries

*   **Endpoint**: `GET /torrent/libraries`
*   **Description**: Retrieves a list of all configured media libraries in Jellyfin and their associated directory paths.
*   **Response (200 OK)**:
    ```json
    [
      {
        "Name": "Movies",
        "Paths": ["/media/movies", "/media/new_movies"]
      },
      {
        "Name": "TV Shows",
        "Paths": ["/media/tv"]
      }
    ]
    ```
    *(Structure of `LibraryInfo`)*

---

### 3. Search Torrents

*   **Endpoint**: `GET /torrent/search`
*   **Description**: Searches for torrents using an external provider (`apibay.org`).
*   **Query Parameters**:
    *   `q` (string, required): The search query.
    *   `cat` (string, optional, default: "0"): The category ID for the search.
*   **Response (200 OK)**: A list of torrent search results (limited to the first 20).
    ```json
    [
      {
        "id": "12345",
        "name": "Example Movie (2025) [1080p]",
        "info_hash": "INFO_HASH_STRING",
        "leechers": "10",
        "seeders": "100",
        "num_files": "1",
        "size": "2147483648", // Size in bytes
        "username": "uploader123",
        "added": "1678886400", // Unix timestamp
        "status": "vip",
        "category": "207", // Example category ID
        "imdb": "tt1234567"
      }
      // ... more results
    ]
    ```
    *(Structure of `TorrentSearchResult`)*
*   **Error Responses**:
    *   `400 Bad Request`: If the search query `q` is empty.
    *   `500 Internal Server Error`: If there's an error during the search.

---

### 4. Start Torrent Download

*   **Endpoint**: `POST /torrent/download`
*   **Description**: Adds a torrent to the qBittorrent client for download using its info_hash.
*   **Request Body**:
    ```json
    {
      "Name": "Optional Torrent Name",
      "Info_hash": "TORRENT_INFO_HASH_STRING",
      "SavePath": "/downloads/movies/" // Optional: specific path within qBittorrent's configured download directory
    }
    ```
    *(Structure of `TorrentDownloadRequest`)*
*   **Details**:
    *   Constructs a magnet link: `magnet:?xt=urn:btih:{Info_hash}&dn={Name}`.
    *   Sends the request to qBittorrent's `/api/v2/torrents/add` endpoint.
*   **Response (200 OK)**:
    ```json
    {
      "Message": "Started downloading: Optional Torrent Name",
      "Id": "TORRENT_INFO_HASH_STRING",
      "SavePath": "/downloads/movies/",
      "ServerResponse": "Ok." // Or other response from qBittorrent
    }
    ```
    *(Structure of `TorrentDownloadResponse`)*
*   **Error Responses**:
    *   `400 Bad Request`: If `Info_hash` is missing.
    *   `500 Internal Server Error`: If there's an error communicating with qBittorrent.

---

### 5. Get Torrent Progress

*   **Endpoint**: `GET /torrent/progress`
*   **Description**: Retrieves information about all torrents currently being processed by qBittorrent.
*   **Details**:
    *   Queries qBittorrent's `/api/v2/torrents/info?filter=all` endpoint.
*   **Response (200 OK)**: An array of objects, where each object represents a torrent and its properties as returned by qBittorrent. The structure depends on the qBittorrent API response.
    ```json
    [
      {
        "added_on": 1678886400,
        "amount_left": 0,
        "auto_tmm": false,
        "availability": -1,
        "category": "movies",
        "completed": 2147483648,
        "completion_on": 1678886500,
        "content_path": "/downloads/Example Movie (2025) [1080p]",
        "dl_limit": -1,
        "dlspeed": 0,
        "download_path": "",
        "downloaded": 2147483648,
        "downloaded_session": 0,
        "eta": 8640000,
        "f_l_piece_prio": false,
        "force_start": false,
        "hash": "TORRENT_INFO_HASH_STRING",
        "infohash_v1": "TORRENT_INFO_HASH_V1",
        "infohash_v2": "",
        "last_activity": 1678886600,
        "magnet_uri": "magnet:?xt=urn:btih:TORRENT_INFO_HASH_STRING...",
        "max_ratio": -1,
        "max_seeding_time": -1,
        "name": "Example Movie (2025) [1080p]",
        "num_complete": -1,
        "num_incomplete": -1,
        "num_leechs": 0,
        "num_seeds": 0,
        "priority": 0,
        "progress": 1,
        "ratio": 0,
        "ratio_limit": -1,
        "save_path": "/downloads/",
        "seeding_time": 0,
        "seeding_time_limit": -1,
        "seen_complete": 1678886550,
        "seq_dl": false,
        "size": 2147483648,
        "state": "pausedUP", // or "downloading", "stalledDL", "uploading", etc.
        "super_seeding": false,
        "tags": "",
        "time_active": 120,
        "total_size": 2147483648,
        "tracker": "udp://tracker.opentrackr.org:1337/announce",
        "trackers_count": 10,
        "up_limit": -1,
        "uploaded": 0,
        "uploaded_session": 0,
        "upspeed": 0
      }
      // ... more torrents
    ]
    ```
*   **Error Responses**:
    *   `500 Internal Server Error`: If there's an error communicating with qBittorrent.

---

### 6. Get Library Paths (Detailed)

*   **Endpoint**: `GET /torrent/libraries/paths`
*   **Description**: Retrieves detailed information about media libraries, including their names and folder paths. Similar to `/torrent/libraries` but may have a slightly different internal processing or intended use.
*   **Response (200 OK)**:
    ```json
    [
      {
        "LibraryName": "Movies",
        "FolderPaths": ["/media/movies", "/media/new_movies"]
      },
      {
        "LibraryName": "TV Shows",
        "FolderPaths": ["/media/tv"]
      }
    ]
    ```
*   **Error Responses**:
    *   `500 Internal Server Error`: If there's an error retrieving library information.

---

### 7. Pause Torrent

*   **Endpoint**: `POST /torrent/pause`
*   **Description**: Pauses a specific torrent in qBittorrent.
*   **Query Parameters**:
    *   `hash` (string, required): The info_hash of the torrent to pause.
*   **Details**:
    *   Sends a request to qBittorrent's `/api/v2/torrents/stop` endpoint (Note: qBittorrent's standard API uses `/api/v2/torrents/pause`. This implementation uses `/stop`).
*   **Response (200 OK)**:
    ```json
    {
      "success": true,
      "message": "Torrent paused successfully"
    }
    ```
*   **Error Responses**:
    *   `400 Bad Request`: If the `hash` parameter is missing.
    *   `500 Internal Server Error`: If there's an error communicating with qBittorrent.

---

### 8. Resume Torrent

*   **Endpoint**: `POST /torrent/resume`
*   **Description**: Resumes a specific paused torrent in qBittorrent.
*   **Query Parameters**:
    *   `hash` (string, required): The info_hash of the torrent to resume.
*   **Details**:
    *   Sends a request to qBittorrent's `/api/v2/torrents/start` endpoint (Note: qBittorrent's standard API uses `/api/v2/torrents/resume`. This implementation uses `/start`).
*   **Response (200 OK)**:
    ```json
    {
      "success": true,
      "message": "Torrent resumed successfully"
    }
    ```
*   **Error Responses**:
    *   `400 Bad Request`: If the `hash` parameter is missing.
    *   `500 Internal Server Error`: If there's an error communicating with qBittorrent.

---

### 9. Delete Torrent

*   **Endpoint**: `POST /torrent/delete`
*   **Description**: Deletes a specific torrent from qBittorrent.
*   **Query Parameters**:
    *   `hash` (string, required): The info_hash of the torrent to delete.
    *   `deleteFiles` (boolean, optional, default: `false`): If `true`, also deletes the downloaded files associated with the torrent from the disk.
*   **Details**:
    *   Sends a request to qBittorrent's `/api/v2/torrents/delete` endpoint.
*   **Response (200 OK)**:
    ```json
    {
      "success": true,
      "message": "Torrent deleted successfully" // or "Torrent deleted successfully with files"
    }
    ```
*   **Error Responses**:
    *   `400 Bad Request`: If the `hash` parameter is missing.
    *   `500 Internal Server Error`: If there's an error communicating with qBittorrent.

---

## Notes

*   The qBittorrent server URL (`http://qbittorrent:8099`) is currently hardcoded in the `TorrentController`.
*   The torrent search functionality relies on the external API `https://apibay.org/`.
*   Ensure your qBittorrent `SavePath` in its settings aligns with how Jellyfin accesses media if you intend for Jellyfin to automatically pick up completed downloads. The `SavePath` parameter in the `/torrent/download` endpoint is relative to qBittorrent's internal filesystem view.
```// filepath: README.md
# Jellyfin Server - Torrenting Integration

This document outlines the torrenting functionalities integrated into this Jellyfin server instance. It allows users with administrative privileges to search for torrents, manage downloads via a qBittorrent backend, and interact with Jellyfin libraries.

## Prerequisites

*   **Jellyfin Server**: This custom version of Jellyfin with the torrenting controller.
*   **qBittorrent Instance**: A running qBittorrent instance accessible from the Jellyfin server.
    *   The integration is hardcoded to connect to qBittorrent at `http://qbittorrent:8099`. Ensure your qBittorrent service is named `qbittorrent` if running in Docker and accessible on port `8099`.
    *   The qBittorrent WebUI API must be enabled.

## Authentication

All torrent-related API endpoints require administrative privileges. Requests must be authenticated, and the user must have elevated permissions (as per `Policy = Policies.RequiresElevation`).

## API Endpoints

The base path for all torrenting API endpoints is `/torrent`.

---

### 1. Test Admin Access

*   **Endpoint**: `GET /torrent/Test1`
*   **Description**: A simple test endpoint to verify that the torrent controller is reachable and the user has administrative access.
*   **Response (200 OK)**:
    ```
    "test 1 success"
    ```

---

### 2. Get Jellyfin Libraries

*   **Endpoint**: `GET /torrent/libraries`
*   **Description**: Retrieves a list of all configured media libraries in Jellyfin and their associated directory paths.
*   **Response (200 OK)**:
    ```json
    [
      {
        "Name": "Movies",
        "Paths": ["/media/movies", "/media/new_movies"]
      },
      {
        "Name": "TV Shows",
        "Paths": ["/media/tv"]
      }
    ]
    ```
    *(Structure of `LibraryInfo`)*

---

### 3. Search Torrents

*   **Endpoint**: `GET /torrent/search`
*   **Description**: Searches for torrents using an external provider (`apibay.org`).
*   **Query Parameters**:
    *   `q` (string, required): The search query.
    *   `cat` (string, optional, default: "0"): The category ID for the search.
*   **Response (200 OK)**: A list of torrent search results (limited to the first 20).
    ```json
    [
      {
        "id": "12345",
        "name": "Example Movie (2025) [1080p]",
        "info_hash": "INFO_HASH_STRING",
        "leechers": "10",
        "seeders": "100",
        "num_files": "1",
        "size": "2147483648", // Size in bytes
        "username": "uploader123",
        "added": "1678886400", // Unix timestamp
        "status": "vip",
        "category": "207", // Example category ID
        "imdb": "tt1234567"
      }
      // ... more results
    ]
    ```
    *(Structure of `TorrentSearchResult`)*
*   **Error Responses**:
    *   `400 Bad Request`: If the search query `q` is empty.
    *   `500 Internal Server Error`: If there's an error during the search.

---

### 4. Start Torrent Download

*   **Endpoint**: `POST /torrent/download`
*   **Description**: Adds a torrent to the qBittorrent client for download using its info_hash.
*   **Request Body**:
    ```json
    {
      "Name": "Optional Torrent Name",
      "Info_hash": "TORRENT_INFO_HASH_STRING",
      "SavePath": "/downloads/movies/" // Optional: specific path within qBittorrent's configured download directory
    }
    ```
    *(Structure of `TorrentDownloadRequest`)*
*   **Details**:
    *   Constructs a magnet link: `magnet:?xt=urn:btih:{Info_hash}&dn={Name}`.
    *   Sends the request to qBittorrent's `/api/v2/torrents/add` endpoint.
*   **Response (200 OK)**:
    ```json
    {
      "Message": "Started downloading: Optional Torrent Name",
      "Id": "TORRENT_INFO_HASH_STRING",
      "SavePath": "/downloads/movies/",
      "ServerResponse": "Ok." // Or other response from qBittorrent
    }
    ```
    *(Structure of `TorrentDownloadResponse`)*
*   **Error Responses**:
    *   `400 Bad Request`: If `Info_hash` is missing.
    *   `500 Internal Server Error`: If there's an error communicating with qBittorrent.

---

### 5. Get Torrent Progress

*   **Endpoint**: `GET /torrent/progress`
*   **Description**: Retrieves information about all torrents currently being processed by qBittorrent.
*   **Details**:
    *   Queries qBittorrent's `/api/v2/torrents/info?filter=all` endpoint.
*   **Response (200 OK)**: An array of objects, where each object represents a torrent and its properties as returned by qBittorrent. The structure depends on the qBittorrent API response.
    ```json
    [
      {
        "added_on": 1678886400,
        "amount_left": 0,
        "auto_tmm": false,
        "availability": -1,
        "category": "movies",
        "completed": 2147483648,
        "completion_on": 1678886500,
        "content_path": "/downloads/Example Movie (2025) [1080p]",
        "dl_limit": -1,
        "dlspeed": 0,
        "download_path": "",
        "downloaded": 2147483648,
        "downloaded_session": 0,
        "eta": 8640000,
        "f_l_piece_prio": false,
        "force_start": false,
        "hash": "TORRENT_INFO_HASH_STRING",
        "infohash_v1": "TORRENT_INFO_HASH_V1",
        "infohash_v2": "",
        "last_activity": 1678886600,
        "magnet_uri": "magnet:?xt=urn:btih:TORRENT_INFO_HASH_STRING...",
        "max_ratio": -1,
        "max_seeding_time": -1,
        "name": "Example Movie (2025) [1080p]",
        "num_complete": -1,
        "num_incomplete": -1,
        "num_leechs": 0,
        "num_seeds": 0,
        "priority": 0,
        "progress": 1,
        "ratio": 0,
        "ratio_limit": -1,
        "save_path": "/downloads/",
        "seeding_time": 0,
        "seeding_time_limit": -1,
        "seen_complete": 1678886550,
        "seq_dl": false,
        "size": 2147483648,
        "state": "pausedUP", // or "downloading", "stalledDL", "uploading", etc.
        "super_seeding": false,
        "tags": "",
        "time_active": 120,
        "total_size": 2147483648,
        "tracker": "udp://tracker.opentrackr.org:1337/announce",
        "trackers_count": 10,
        "up_limit": -1,
        "uploaded": 0,
        "uploaded_session": 0,
        "upspeed": 0
      }
      // ... more torrents
    ]
    ```
*   **Error Responses**:
    *   `500 Internal Server Error`: If there's an error communicating with qBittorrent.

---

### 6. Get Library Paths (Detailed)

*   **Endpoint**: `GET /torrent/libraries/paths`
*   **Description**: Retrieves detailed information about media libraries, including their names and folder paths. Similar to `/torrent/libraries` but may have a slightly different internal processing or intended use.
*   **Response (200 OK)**:
    ```json
    [
      {
        "LibraryName": "Movies",
        "FolderPaths": ["/media/movies", "/media/new_movies"]
      },
      {
        "LibraryName": "TV Shows",
        "FolderPaths": ["/media/tv"]
      }
    ]
    ```
*   **Error Responses**:
    *   `500 Internal Server Error`: If there's an error retrieving library information.

---

### 7. Pause Torrent

*   **Endpoint**: `POST /torrent/pause`
*   **Description**: Pauses a specific torrent in qBittorrent.
*   **Query Parameters**:
    *   `hash` (string, required): The info_hash of the torrent to pause.
*   **Details**:
    *   Sends a request to qBittorrent's `/api/v2/torrents/stop` endpoint (Note: qBittorrent's standard API uses `/api/v2/torrents/pause`. This implementation uses `/stop`).
*   **Response (200 OK)**:
    ```json
    {
      "success": true,
      "message": "Torrent paused successfully"
    }
    ```
*   **Error Responses**:
    *   `400 Bad Request`: If the `hash` parameter is missing.
    *   `500 Internal Server Error`: If there's an error communicating with qBittorrent.

---

### 8. Resume Torrent

*   **Endpoint**: `POST /torrent/resume`
*   **Description**: Resumes a specific paused torrent in qBittorrent.
*   **Query Parameters**:
    *   `hash` (string, required): The info_hash of the torrent to resume.
*   **Details**:
    *   Sends a request to qBittorrent's `/api/v2/torrents/start` endpoint (Note: qBittorrent's standard API uses `/api/v2/torrents/resume`. This implementation uses `/start`).
*   **Response (200 OK)**:
    ```json
    {
      "success": true,
      "message": "Torrent resumed successfully"
    }
    ```
*   **Error Responses**:
    *   `400 Bad Request`: If the `hash` parameter is missing.
    *   `500 Internal Server Error`: If there's an error communicating with qBittorrent.

---

### 9. Delete Torrent

*   **Endpoint**: `POST /torrent/delete`
*   **Description**: Deletes a specific torrent from qBittorrent.
*   **Query Parameters**:
    *   `hash` (string, required): The info_hash of the torrent to delete.
    *   `deleteFiles` (boolean, optional, default: `false`): If `true`, also deletes the downloaded files associated with the torrent from the disk.
*   **Details**:
    *   Sends a request to qBittorrent's `/api/v2/torrents/delete` endpoint.
*   **Response (200 OK)**:
    ```json
    {
      "success": true,
      "message": "Torrent deleted successfully" // or "Torrent deleted successfully with files"
    }
    ```
*   **Error Responses**:
    *   `400 Bad Request`: If the `hash` parameter is missing.
    *   `500 Internal Server Error`: If there's an error communicating with qBittorrent.

---

## Notes

*   The qBittorrent server URL (`http://qbittorrent:8099`) is currently hardcoded in the `TorrentController`.
*   The torrent search functionality relies on the external API `https://apibay.org/`.
*   Ensure your qBittorrent `SavePath` in its settings aligns with how Jellyfin accesses media if you intend for Jellyfin to automatically pick up completed downloads. The `SavePath` parameter in the `/torrent/download` endpoint is relative to qBittorrent's internal filesystem view.