using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.StrmDirectPlay.Core
{
    /// <summary>
    /// STRM file URL reader.
    /// </summary>
    public class StrmUrlReader
    {
        private readonly ILogger<StrmUrlReader> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrmUrlReader"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public StrmUrlReader(ILogger<StrmUrlReader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Read URL from STRM file.
        /// </summary>
        /// <param name="path">Path to STRM file.</param>
        /// <returns>URL string or null.</returns>
        public async Task<string?> ReadUrlAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (!File.Exists(path))
            {
                _logger.LogWarning("[STRM-DP] STRM file not found: {Path}", path);
                return null;
            }

            try
            {
                // Read first line from STRM file
                var url = await File.ReadAllTextAsync(path).ConfigureAwait(false);
                
                // Get first non-empty line
                using var reader = new StringReader(url);
                string? line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        _logger.LogDebug("[STRM-DP] Read URL from {Path}: {Url}", path, line);
                        return line;
                    }
                }

                _logger.LogWarning("[STRM-DP] Empty STRM file: {Path}", path);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[STRM-DP] Error reading STRM file: {Path}", path);
                return null;
            }
        }

        /// <summary>
        /// Check if file is a STRM file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>True if STRM file.</returns>
        public bool IsStrmFile(string? path) => IsStrmPath(path);

        /// <summary>
        /// Static helper: check if the given path is a STRM file (suffix check).
        /// </summary>
        public static bool IsStrmPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return path.EndsWith(".strm", StringComparison.OrdinalIgnoreCase);
        }
    }
}
