# Configuration Examples

## Example 1: Single Proxy Domain

**Use Case**: All .strm files point to a self-hosted proxy

**Configuration**:
- Mode: `Smart`
- Domain Whitelist: `proxy.mydomain.com`
- Block Transcoding: `true`
- Debug Logging: `false`

**Result**:
```
Movie.strm (https://proxy.mydomain.com/movie.m3u8)
  → Direct play
  → Original URL returned to client
  → No transcoding

Show.strm (https://other-site.com/show.m3u8)
  → Normal Jellyfin behavior
```

---

## Example 2: Multiple CDN Domains

**Use Case**: Using multiple CDNs for different content

**Configuration**:
- Mode: `Smart`
- Domain Whitelist:
  ```
  cdn1.mydomain.com
  cdn2.mydomain.com
  cdn3.mydomain.com
  ```
- Block Transcoding: `true`

**Result**: All URLs from the 3 CDNs will direct play without transcoding.

---

## Example 3: Wildcard Subdomain

**Use Case**: CDN with dynamic subdomains (cdn1, cdn2, cdn3, etc.)

**Configuration**:
- Mode: `Smart`
- Domain Whitelist: `*.cdn.mydomain.com`
- Block Transcoding: `true`

**Result**:
```
https://server1.cdn.mydomain.com/video.m3u8  → Matched
https://server2.cdn.mydomain.com/video.m3u8  → Matched
https://cdn.mydomain.com/video.m3u8          → Matched
https://other.mydomain.com/video.m3u8        → Not matched
```

---

## Example 4: Alist Setup

**Use Case**: Using Alist for cloud storage

**Configuration**:
- Mode: `ReturnOriginalUrl`
- Domain Whitelist: `alist.mydomain.com`
- Block Transcoding: `true`

**STRM Content**:
```
https://alist.mydomain.com/d/GoogleDrive/Movies/Movie.mp4
```

**Result**: URL is forwarded directly to client, Jellyfin does not act as proxy.

---

## Example 5: Mixed Content (Safe Mode)

**Use Case**: Some .strm from proxy, some from public URLs

**Configuration**:
- Mode: `Smart`
- Domain Whitelist: `proxy.mydomain.com`
- Block Transcoding: `false`

**Result**:
- Proxy domain: direct play
- Public URL: normal Jellyfin behavior (can transcode if needed)

---

## Example 6: IPTV Style

**Use Case**: IPTV streams from self-hosted provider

**Configuration**:
- Mode: `AlwaysDirectPlay`
- Block Transcoding: `true`

**Result**: All .strm files direct play immediately, suitable for live streams.

---

## Example 7: Development/Testing

**Use Case**: Testing plugin behavior

**Configuration**:
- Mode: `Smart`
- Domain Whitelist: `localhost:8080`
- Block Transcoding: `true`
- Debug Logging: `true`

**Logs**:
```
[STRM-DP] Mode=Smart
[STRM-DP] Detected STRM item: test.strm
[STRM-DP] Read URL: http://localhost:8080/test.m3u8
[STRM-DP] Domain matched: localhost:8080
[STRM-DP] Returning original URL
[STRM-DP] Modified MediaSource:
[STRM-DP]   Path: http://localhost:8080/test.m3u8
[STRM-DP]   SupportsDirectPlay: True
[STRM-DP]   SupportsTranscoding: False
```

---

## Example 8: WebDAV Cloud Storage

**Use Case**: NextCloud/ownCloud via WebDAV

**Configuration**:
- Mode: `Smart`
- Domain Whitelist: `cloud.mydomain.com`
- Block Transcoding: `true`

**STRM Content**:
```
https://cloud.mydomain.com/remote.php/webdav/Movies/Movie.mkv
```

---

## Example 9: Google Drive via Proxy

**Use Case**: Google Drive with Cloudflare worker

**Configuration**:
- Mode: `ReturnOriginalUrl`
- Domain Whitelist:
  ```
  worker.mydomain.workers.dev
  drive.mydomain.com
  ```
- Block Transcoding: `true`

---

## Example 10: Disabled for Testing

**Use Case**: Temporarily disable plugin without uninstalling

**Configuration**:
- Mode: `Disabled`

**Result**: Plugin does nothing, Jellyfin runs normally.

---

## Recommended Configuration (Default)

For most users:

```yaml
Mode: Smart
Domain Whitelist: [your-proxy-domain.com]
Block Transcoding: true
Debug Logging: false
```

Safe, flexible, and optimal performance.
