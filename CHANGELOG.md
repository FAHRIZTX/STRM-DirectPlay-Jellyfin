# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/releases/tag/v1.0.0
