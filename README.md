# STRM DirectPlay for Jellyfin

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Jellyfin Version](https://img.shields.io/badge/Jellyfin-10.11.x-blue.svg)](https://jellyfin.org/)

STRM DirectPlay is an open-source plugin for Jellyfin that gives full control over `.strm` file playback behavior.

## Features

- 🎯 **Multiple Modes**: Choose how the plugin handles .strm files
- 🌐 **Domain Whitelist**: Granular control based on domain
- 🚫 **Block Transcoding**: Prevent transcoding for .strm files
- 📺 **HLS Direct Stream**: Resolve `.m3u8` playlists to direct media segment URLs when possible
- 🧩 **Jellyfin Repository Support**: `manifest.json` is published in Jellyfin plugin repository format
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

### Method 1: Jellyfin Plugin Repository

1. Open Jellyfin Dashboard
2. Go to **Plugins** > **Repositories**
3. Add this repository manifest URL:

```text
https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/releases/latest/download/manifest.json
```

4. Go to **Catalog**
5. Install **STRM DirectPlay**
6. Restart Jellyfin server

> `manifest.json` is a Jellyfin plugin repository manifest. It is published as a separate release asset and is not included inside the plugin ZIP.

### Method 2: Manual Installation

1. Download the latest release from [Releases](https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/releases)
2. Extract the ZIP file
3. Copy the `Jellyfin.Plugin.StrmDirectPlay` folder to:
   - Windows: `%AppData%\Jellyfin\Server\plugins\`
   - Linux: `/var/lib/jellyfin/plugins/`
   - Docker: `/config/plugins/`
4. Restart Jellyfin server

### Method 3: Build from Source

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

#### ForceDirectStream
Force direct playback handling for all safe `.strm` URLs.

This mode is intended for remote HLS (`.m3u8`) and cloud/proxy URLs that Jellyfin normally tries to transcode. The plugin modifies the media source so Jellyfin prefers DirectPlay/DirectStream.

Behavior:

- keeps `IsRemote` disabled to avoid Jellyfin's forced remote-source transcoding policy
- marks the source as DirectPlay/DirectStream capable
- avoids probing and index requirements where possible
- resolves `.m3u8` playlists to a direct `.ts` media segment URL when possible

#### BypassUserPolicy
Stricter variant of `ForceDirectStream` for trusted STRM sources.

Use this when Jellyfin user/device policy still forces remuxing or transcoding even though the source is directly playable.

Recommended only for trusted private URLs because it intentionally bypasses more Jellyfin playback checks.

## HLS / `.m3u8` Behavior

Jellyfin commonly transcodes `.m3u8` STRM sources because its internal direct stream checks reject paths containing `.m3u` / `.m3u8`, and remote sources can also trigger forced transcoding policies.

STRM DirectPlay handles this by:

1. Reading the original URL from the `.strm` file
2. Fetching the HLS manifest when the URL ends with `.m3u8`
3. Selecting the first variant playlist for master playlists, or the first media segment for media playlists
4. Returning a direct media segment URL to Jellyfin
5. Overriding media source flags so Jellyfin can choose DirectPlay/DirectStream

Limitations:

- HLS URLs that require custom headers, cookies, signed per-request tokens, or DRM may still fail.
- The client must be able to access the resolved media segment URL directly.
- Live HLS streams may not be ideal if the first segment expires quickly.

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

For HLS troubleshooting, look for logs like:

```text
[STRM-DP] Resolved HLS manifest to first segment: https://example.com/path/segment.ts
[STRM-DP] SupportsDirectStream override -> true for ...
```

## Release Package Format

GitHub Releases publish these assets:

```text
STRM-DirectPlay-Jellyfin_<version>.zip
STRM-DirectPlay-Jellyfin_<version>.zip.md5
manifest.json
```

The ZIP contains runtime plugin files only:

```text
Jellyfin.Plugin.StrmDirectPlay/
    Jellyfin.Plugin.StrmDirectPlay.dll
    README.md
    LICENSE
    CHANGELOG.md
```

`manifest.json` is kept outside the ZIP because it contains the ZIP checksum. Putting it inside the ZIP would create a checksum loop.

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
2. For `.m3u8` URLs, try `ForceDirectStream` first
3. If mode is `DomainWhitelist` or `Smart`, make sure domain is in whitelist
4. If user/device policy still forces transcoding, try `BypassUserPolicy` for trusted sources
5. Enable `Debug Logging` and check logs
6. Make sure `Block Transcoding` is enabled

### Jellyfin says checksum does not match

1. Make sure the repository URL points to the latest `manifest.json` release asset
2. Do not use a stale cached manifest file
3. The manifest checksum is MD5, matching Jellyfin plugin installer expectations
4. Re-add the repository in Jellyfin if it cached an older manifest

### Plugin fails to load `Scrutor`

Upgrade to version `1.0.18` or newer. Older builds used `Scrutor` for service decoration and could fail if `Scrutor.dll` was not present in the Jellyfin plugin folder.

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
├── ServiceRegistrator.cs          # Manual IMediaSourceManager decoration
├── StrmMediaSourceManager.cs      # Jellyfin media source manager decorator
├── PluginConfiguration.cs
├── Core/
│   ├── PlaybackInterceptor.cs    # Main logic
│   ├── StrmUrlReader.cs           # Read .strm files
│   └── DomainMatcher.cs           # Whitelist validation
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
