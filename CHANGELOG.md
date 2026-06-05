# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.13] - 2026-06-06

### Added
- Added a CI build workflow that runs on every push and pull request.
- Added `ForceDirectStream` mode to force direct playback behavior for STRM sources.
- Added `BypassUserPolicy` mode for stricter direct playback bypass behavior on trusted STRM sources.
- Added HLS manifest resolution for `.m3u8` URLs, resolving playlists to a direct media segment URL when possible.

### Changed
- Updated STRM playback handling to prefer DirectPlay/DirectStream for remote HLS sources.
- Updated media source mutation to avoid marking STRM URLs as remote, preventing Jellyfin's forced remote-source transcoding policy from triggering.
- Updated media source metadata with direct-play-friendly container, stream, and probing settings.
- Updated the plugin configuration UI with the new direct stream bypass modes.
- Updated Jellyfin media source decoration to override direct stream capability checks for STRM/HLS playback.

### Fixed
- Fixed `.m3u8` STRM playback being forced into transcoding by Jellyfin's default direct stream checks.
- Fixed `SupportsDirectStream` being reset to `false` for paths containing `.m3u` / `.m3u8`.
- Fixed potential forced transcoding caused by `IsRemote = true` on remote STRM URLs.
- Fixed a compile issue after converting media source modification to an async flow.

## [1.0.0] - 2026-06-04

### Added
- Initial release
- Multiple plugin modes (Disabled, AlwaysDirectPlay, DomainWhitelist, ReturnOriginalUrl, Smart)
- Domain whitelist with wildcard support
- Block transcoding option for .strm files
- Debug logging capability
- Security validation (block file://, smb://, loopback)
- Web configuration UI
- Support for Jellyfin 10.11.x
- Comprehensive documentation
- MIT License

### Features
- **Plugin Modes**:
  - Disabled: No plugin behavior
  - AlwaysDirectPlay: Force direct play all .strm
  - DomainWhitelist: Apply to whitelisted domains only
  - ReturnOriginalUrl: Return URL without Jellyfin proxy
  - Smart: Recommended mode combining whitelist + original URL

- **Security**:
  - URL scheme validation (http/https only)
  - Block local file access
  - Block loopback addresses
  - Path traversal prevention

- **Configuration**:
  - Web-based configuration page
  - Domain whitelist management
  - Toggle transcoding blocking
  - Toggle debug logging

- **Core Components**:
  - PlaybackInterceptor: Main logic
  - StrmUrlReader: Read .strm file contents
  - DomainMatcher: Whitelist validation
  - MediaSourceInterceptor: Jellyfin integration

[Unreleased]: https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/compare/v1.0.13...HEAD
[1.0.13]: https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/releases/tag/v1.0.13
[1.0.0]: https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/releases/tag/v1.0.0

