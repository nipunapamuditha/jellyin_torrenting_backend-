# Base image using .NET 9.0 runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

ENV JELLYFIN_FFMPEG=/usr/bin/ffmpeg
ENV JELLYFIN_TEMP_DIR=/cache/temp

# Set working directory
WORKDIR /app

# Copy your pre-built application
COPY ./Jellyfin.Server/bin/Release/net9.0/ ./

# Create web client directory
RUN mkdir -p jellyfin-web

# Copy your custom frontend files
# Use absolute paths to ensure Docker finds the files
COPY ["./custom-frontend/", "./jellyfin-web/"]

# Verify files were copied (debugging step)
RUN ls -la ./jellyfin-web/

# Install FFmpeg and other dependencies
RUN apt-get update && \
    apt-get install -y ffmpeg curl && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Expose ports
EXPOSE 8096/tcp
EXPOSE 8920/tcp
EXPOSE 1900/udp
EXPOSE 8099/tcp

# Set environment variables
ENV JELLYFIN_DATA_DIR=/config
ENV JELLYFIN_CACHE_DIR=/cache

# Create volume mount points
VOLUME /config
VOLUME /cache
VOLUME /media

# Start Jellyfin using the correct dll
ENTRYPOINT ["dotnet", "jellyfin.dll"]