# STRM DirectPlay for Jellyfin

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Jellyfin Version](https://img.shields.io/badge/Jellyfin-10.11.x-blue.svg)](https://jellyfin.org/)

STRM DirectPlay is an open-source plugin for Jellyfin that gives full control over `.strm` file playback behavior.

## Features

- 🎯 **Multiple Modes**: Choose how the plugin handles .strm files
- 🌐 **Domain Whitelist**: Granular control based on domain
- 🚫 **Block Transcoding**: Prevent transcoding for .strm files
- 🔒 **Security**: URL validation and dangerous scheme blocking
- 📊 **Debug Logging**: Verbose logging for troubleshooting
- ⚡ **Performance**: Minimize CPU usage and server bandwidth

## Use Cases

This plugin is suited for:

- Self-hosted streaming proxy
- HLS (`master.m3u8`) streams
- Alist/OpenList
- WebDAV
- Cloud storage (Google Drive, OneDrive, etc.)
- Private CDN
- IPTV-style streams

## Installation

### Method 1: Manual Installation

1. Download the latest release from [Releases](https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/releases)
2. Extract the ZIP file
3. Copy the `Jellyfin.Plugin.StrmDirectPlay` folder to:
   - Windows: `%AppData%\Jellyfin\Server\plugins\`
   - Linux: `/var/lib/jellyfin/plugins/`
   - Docker: `/config/plugins/`
4. Restart Jellyfin server

### Method 2: Build from Source

```powershell
# Clone repository
git clone https://github.com/fahriztx/STRM-DirectPlay-Jellyfin.git
cd STRM-DirectPlay-Jellyfin

# Build plugin
dotnet build -c Release

# Copy output to plugins folder
# Output is at: Jellyfin.Plugin.StrmDirectPlay\bin\Release\net9.0\
```

## Configuration

1. Open Jellyfin Dashboard
2. Navigate to **Plugins** > **STRM DirectPlay**
3. Select the operation mode

### Plugin Modes

#### Disabled
Plugin does nothing. Jellyfin runs normally.

#### AlwaysDirectPlay
Force direct play for all `.strm` files. Transcoding is blocked.

```
.strm → Direct Play → Client
```

#### DomainWhitelist
Plugin is only active for domains in the whitelist.

```
https://proxy.domain.com/video.m3u8 → Processed (if whitelisted)
https://example.com/video.m3u8     → Normal Jellyfin behavior
```

#### ReturnOriginalUrl
Plugin returns the original URL from the `.strm` file directly to the client.

```
.strm → Original URL → Client (no Jellyfin proxy)
```

#### Smart (Recommended)
Combination of DomainWhitelist + ReturnOriginalUrl.

```
If URL matches whitelist:
    Return original URL
Else:
    Normal Jellyfin behavior
```

### Domain Whitelist Configuration

Enter one domain per line:

```
proxy.domain.com
cdn.domain.com
media.domain.com
```

Wildcard subdomain support:

```
*.example.com
```

### Block Transcoding

When enabled, the plugin prevents Jellyfin from creating a transcoding session for `.strm` files.

### Debug Logging

Enable to see detailed logs:

```
[STRM-DP] Mode=Smart
[STRM-DP] Detected STRM item: Movie.strm
[STRM-DP] Read URL: https://proxy.domain.com/video/master.m3u8
[STRM-DP] Domain matched: proxy.domain.com
[STRM-DP] Returning original URL
```

## Security

Plugin has security checks:

✅ **Allowed Schemes**:
- `http://`
- `https://`

❌ **Blocked Schemes**:
- `file://`
- `smb://`
- `ftp://`
- Loopback addresses

## Troubleshooting

### Plugin not showing in Dashboard

1. Make sure plugin files are in the correct folder
2. Check Jellyfin logs: `/var/log/jellyfin/`
3. Restart Jellyfin server

### .strm file still being transcoded

1. Check plugin mode (must be other than `Disabled`)
2. If mode is `DomainWhitelist` or `Smart`, make sure domain is in whitelist
3. Enable `Debug Logging` and check logs
4. Make sure `Block Transcoding` is enabled

### Client cannot play .strm file

1. Test URL directly in browser
2. Check if URL requires authentication
3. Check logs for error messages
4. Make sure client supports stream format (HLS, DASH, etc.)

### Debug Logging not appearing

1. Check Jellyfin logging configuration
2. Set minimum log level to `Debug` or `Information`
3. Restart Jellyfin after changing config

## Development

### Requirements

- .NET 9.0 SDK
- Jellyfin 10.11.x
- Visual Studio 2022 or VS Code

### Build

```bash
dotnet restore
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Debug

1. Build plugin in Debug mode
2. Copy output to Jellyfin plugins folder
3. Attach debugger to Jellyfin process

## Architecture

```
Plugin.cs
├── PluginConfiguration.cs
├── Core/
│   ├── PlaybackInterceptor.cs    # Main logic
│   ├── StrmUrlReader.cs           # Read .strm files
│   └── DomainMatcher.cs           # Whitelist validation
├── MediaSourceInterceptor.cs      # Jellyfin integration
└── Configuration/
    └── configPage.html            # Web UI
```

## Roadmap

- [ ] Regex domain matching
- [ ] Blacklist mode
- [ ] STRM metadata cache
- [ ] Health check for URLs
- [ ] Statistics page
- [ ] Support Jellyfin 10.12.x & 11.x

## Contributing

Contributions welcome! Please:

1. Fork repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Jellyfin Team for the amazing platform
- Community for feedback and testing

## Support

- 🐛 **Issues**: [GitHub Issues](https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/discussions)
- 📧 **Email**: your.email@example.com

## Disclaimer

This plugin is not affiliated with the Jellyfin project. Use at your own risk.

---

Made with ❤️ for the self-hosting community
