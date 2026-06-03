# Contributing to STRM DirectPlay

Thank you for your interest in contributing! 🎉

## Code of Conduct

Be respectful and constructive. We're all here to make this plugin better.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues. When you create a bug report, include:

- **Clear title and description**
- **Steps to reproduce**
- **Expected behavior**
- **Actual behavior**
- **Jellyfin version**
- **Plugin version**
- **Relevant logs** (with Debug Logging enabled)
- **Environment details** (OS, Docker, etc.)

### Suggesting Features

Feature requests are welcome! Include:

- **Use case**: Why do you need this feature?
- **Proposed solution**: How should it work?
- **Alternatives**: What alternatives have you considered?

### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes
4. Follow coding standards (see below)
5. Add/update tests if applicable
6. Update documentation
7. Commit with clear messages (`git commit -m 'Add some AmazingFeature'`)
8. Push to your branch (`git push origin feature/AmazingFeature`)
9. Open a Pull Request

## Coding Standards

### C# Style

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use XML documentation comments for public APIs
- Enable nullable reference types
- Use `var` for obvious types
- Keep methods focused and small

### Example

```csharp
/// <summary>
/// Validates URL safety.
/// </summary>
/// <param name="url">URL to validate.</param>
/// <returns>True if URL is safe.</returns>
public bool IsUrlSafe(string url)
{
    if (string.IsNullOrWhiteSpace(url))
    {
        return false;
    }

    // Implementation
}
```

### Logging

Use structured logging:

```csharp
_logger.LogInformation("[STRM-DP] Processing {ItemName} with mode {Mode}", item.Name, config.Mode);
```

Prefix all log messages with `[STRM-DP]`.

### Configuration

- Keep config properties simple
- Provide sensible defaults
- Document each option

## Development Setup

1. Install .NET 8.0 SDK
2. Clone repository
3. Open in Visual Studio 2022 or VS Code
4. Build: `dotnet build`
5. Test: Copy to Jellyfin plugins folder and restart

## Testing

### Manual Testing

1. Create test `.strm` files with various URLs
2. Test all plugin modes
3. Test domain whitelist matching
4. Test transcoding blocking
5. Test with different clients
6. Check logs for errors

### Test Cases

- Empty .strm file
- Invalid URL format
- Local file URL (should block)
- Loopback URL (should block)
- HTTP vs HTTPS
- Domain with subdomains
- Wildcard domain matching
- URL with query parameters
- HLS master.m3u8

## Documentation

- Update README.md for user-facing changes
- Update BUILD.md for build process changes
- Update CHANGELOG.md following Keep a Changelog format
- Add XML comments for new public APIs

## Commit Messages

Use clear, descriptive commit messages:

```
Add domain wildcard support

- Implement wildcard matching (*.example.com)
- Add tests for subdomain matching
- Update documentation
```

Format:
```
<type>: <subject>

<body>
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance tasks

## Release Process

1. Update version in `.csproj`
2. Update CHANGELOG.md
3. Update manifest.json
4. Create git tag (`v1.0.0`)
5. Build release package
6. Create GitHub release
7. Update checksum in manifest.json

## Questions?

Open a [GitHub Discussion](https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/discussions) or create an issue.

Thank you for contributing! 🙏
