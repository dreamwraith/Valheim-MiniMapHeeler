using System;
using BepInEx.Configuration;

namespace MiniMapHeeler.Configuration
{
    /// <summary>
    /// Custom configuration attributes recognized by BepInEx ConfigurationManager.
    /// </summary>
    internal class ConfigurationManagerAttributes
    {
        public bool? IsAdvanced { get; set; }
        public int? Order { get; set; }
        public bool? Browsable { get; set; }
        public Action<ConfigEntryBase>? CustomDrawer { get; set; }
    }
}
