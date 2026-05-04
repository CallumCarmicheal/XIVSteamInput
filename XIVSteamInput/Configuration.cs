using Dalamud.Configuration;

using System;

namespace XIVSteamInput;

[Serializable]
public class Configuration : IPluginConfiguration {
    /// <summary>
    /// Plugin configuration version, increment when ever a variable is removed. 
    /// 
    /// New variables do not need an incrementation as they will will cause any regression.
    /// Type changes and removals will cause regressions and thus require an increment.
    /// </summary>
    public int Version { get; set; } = 0;

#region Changelog
    public string CurrentPluginVersion { get; set; } = string.Empty;
    public bool ShowChangelogOnVersionChange { get; set; } = true; // Default to true
#endregion

    // The below exists just to make saving less cumbersome
    public void Save() {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
