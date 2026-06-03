# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |

## Reporting a Vulnerability

If you discover a security vulnerability, please report it responsibly:

1. **DO NOT** open a public GitHub issue
2. Email security details to: your.email@example.com
3. Include:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

We will respond within 48 hours and work with you to address the issue.

## Security Measures

### URL Validation

The plugin validates all URLs from .strm files:

- ✅ Only `http://` and `https://` schemes allowed
- ❌ Blocks `file://`, `smb://`, `ftp://`, and other local protocols
- ❌ Blocks loopback addresses (localhost, 127.0.0.1)
- ❌ Prevents path traversal attacks

### Configuration Security

- Plugin configuration is stored in Jellyfin's secure configuration system
- Only administrators can modify plugin settings
- No credentials or secrets are stored by the plugin

### Best Practices

When using this plugin:

1. **Use HTTPS**: Always prefer HTTPS URLs in .strm files
2. **Whitelist Trusted Domains**: Use Smart or DomainWhitelist mode
3. **Keep Updated**: Update to latest plugin version
4. **Monitor Logs**: Enable debug logging to detect suspicious activity
5. **Network Security**: Use firewall rules to restrict Jellyfin server access

## Known Limitations

- Plugin does not validate SSL certificates (relies on client)
- Plugin does not perform deep packet inspection
- Plugin does not cache or proxy streams (by design)
- Plugin cannot prevent client-side attacks

## Disclosure Policy

We follow responsible disclosure:

1. Security issue reported
2. Issue verified and fixed
3. New version released
4. Public disclosure after 90 days or patch release (whichever comes first)

## Hall of Fame

We appreciate security researchers who help keep this project secure.

<!-- List of contributors who reported vulnerabilities -->

## Contact

Security concerns: your.email@example.com

For general issues: [GitHub Issues](https://github.com/fahriztx/STRM-DirectPlay-Jellyfin/issues)
