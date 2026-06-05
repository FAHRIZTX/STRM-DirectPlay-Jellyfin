using System;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.StrmDirectPlay.Configuration
{
    /// <summary>
    /// Plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            Mode = PluginMode.Disabled;
            DomainWhitelist = Array.Empty<string>();
            BlockTranscoding = true;
            DebugLogging = false;
        }

        /// <summary>
        /// Gets or sets the plugin mode.
        /// </summary>
        public PluginMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the domain whitelist.
        /// </summary>
        public string[] DomainWhitelist { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to block transcoding.
        /// </summary>
        public bool BlockTranscoding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether debug logging is enabled.
        /// </summary>
        public bool DebugLogging { get; set; }
    }

    /// <summary>
    /// Plugin operation modes.
    /// </summary>
    public enum PluginMode
    {
        /// <summary>
        /// Plugin is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Always direct play STRM files.
        /// </summary>
        AlwaysDirectPlay = 1,

        /// <summary>
        /// Only apply to whitelisted domains.
        /// </summary>
        DomainWhitelist = 2,

        /// <summary>
        /// Return original URL without proxy.
        /// </summary>
        ReturnOriginalUrl = 3,

        /// <summary>
        /// Smart mode - whitelist + return original URL.
        /// </summary>
        Smart = 4,

        /// <summary>
        /// Force direct play for all STRM files, bypasses user policy and
        /// Jellyfin's automatic container/protocol checks. Use this when
        /// you want HLS (.m3u8) and other remote streams to be sent
        /// directly to the client without any transcoding.
        /// </summary>
        ForceDirectStream = 5,

        /// <summary>
        /// Bypass every user policy and Jellyfin internal check. Like
        /// ForceDirectStream but also forces PlayMethod.DirectPlay even
        /// when the user has "Force transcoding" or "Force remux" enabled.
        /// Use only for trusted local STRM sources.
        /// </summary>
        BypassUserPolicy = 6,
    }
}
