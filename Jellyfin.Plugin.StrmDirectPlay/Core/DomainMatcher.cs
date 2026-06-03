using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.StrmDirectPlay.Core
{
    /// <summary>
    /// Domain matching service.
    /// </summary>
    public class DomainMatcher
    {
        private readonly ILogger<DomainMatcher> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainMatcher"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public DomainMatcher(ILogger<DomainMatcher> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Check if URL matches any domain in whitelist.
        /// </summary>
        /// <param name="url">URL to check.</param>
        /// <param name="whitelist">Domain whitelist.</param>
        /// <returns>True if matched.</returns>
        public bool IsWhitelisted(string url, string[] whitelist)
        {
            if (string.IsNullOrWhiteSpace(url) || whitelist == null || whitelist.Length == 0)
            {
                return false;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                _logger.LogWarning("[STRM-DP] Invalid URL format: {Url}", url);
                return false;
            }

            var host = uri.Host.ToLowerInvariant();

            foreach (var domain in whitelist)
            {
                if (string.IsNullOrWhiteSpace(domain))
                {
                    continue;
                }

                var normalizedDomain = domain.Trim().ToLowerInvariant();

                // Exact match
                if (host == normalizedDomain)
                {
                    _logger.LogDebug("[STRM-DP] Domain matched (exact): {Host} == {Domain}", host, normalizedDomain);
                    return true;
                }

                // Subdomain match (*.example.com)
                if (normalizedDomain.StartsWith("*."))
                {
                    var baseDomain = normalizedDomain.Substring(2);
                    if (host.EndsWith("." + baseDomain) || host == baseDomain)
                    {
                        _logger.LogDebug("[STRM-DP] Domain matched (wildcard): {Host} matches {Domain}", host, normalizedDomain);
                        return true;
                    }
                }
                // Check if host is subdomain of whitelist domain
                else if (host.EndsWith("." + normalizedDomain))
                {
                    _logger.LogDebug("[STRM-DP] Domain matched (subdomain): {Host} matches {Domain}", host, normalizedDomain);
                    return true;
                }
            }

            _logger.LogDebug("[STRM-DP] Domain not matched: {Host}", host);
            return false;
        }

        /// <summary>
        /// Validate URL security.
        /// </summary>
        /// <param name="url">URL to validate.</param>
        /// <returns>True if URL is safe.</returns>
        public bool IsUrlSafe(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            // Only allow HTTP and HTTPS
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                _logger.LogWarning("[STRM-DP] Unsafe URL scheme detected: {Scheme}", uri.Scheme);
                return false;
            }

            // Block local/private IPs
            if (uri.IsLoopback)
            {
                _logger.LogWarning("[STRM-DP] Loopback URL blocked: {Url}", url);
                return false;
            }

            return true;
        }
    }
}
