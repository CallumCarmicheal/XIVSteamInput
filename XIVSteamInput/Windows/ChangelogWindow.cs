using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

using SamplePlugin;

using Serilog;

using System;
using System.Collections.Generic;
using System.Numerics;

namespace XIVSteamInput.Windows;

public sealed class ChangelogWindow : Window, IDisposable {
    private string CurrentVersion = string.Empty;

    private bool showOlderVersions = true;

    private Plugin plugin { get; init; }

    public ChangelogWindow(Plugin plugin, string currentVersion) : base(
        "Steam Input Integration - Changelog###XIVSI_CL",
        ImGuiWindowFlags.NoCollapse
    ) {
        this.plugin = plugin;
        this.CurrentVersion = currentVersion;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(520, 420),
            MaximumSize = new Vector2(760, 720),
        };

        this.Size = new Vector2(620, 560);
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() {
        //
    }

    public void OpenIfNeeded() {
        // Check the changelog
        if (plugin.Configuration.ShowChangelogOnVersionChange && plugin.Configuration.CurrentPluginVersion != CurrentVersion) {
            plugin.Configuration.CurrentPluginVersion = CurrentVersion;
            plugin.Configuration.Save();

            Plugin.Log.Info("Updated plugin config version to latest version.");

            this.IsOpen = true;
        }
    }

    public override void Draw() {
        DrawHeader();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DrawContributorThanks();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Checkbox("Show older versions", ref this.showOlderVersions);

        DrawShowChangelogOnVersionChangeCheckbox();

        ImGui.Spacing();

        var footerHeight =
            ImGui.GetFrameHeightWithSpacing()
            + ImGui.GetStyle().ItemSpacing.Y
            + ImGui.GetTextLineHeightWithSpacing();

        var childSize = new Vector2(0, -footerHeight);

        if (ImGui.BeginChild("##ChangelogScrollRegion", childSize, true)) {
            foreach (var entry in Entries) {
                if (!this.showOlderVersions && entry.Version != CurrentVersion)
                    continue;

                DrawVersionEntry(entry);
            }
        }

        ImGui.EndChild();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DrawFooter();
    }

    private void DrawShowChangelogOnVersionChangeCheckbox() {
        var dontShowChangelogEveryUpdate = !plugin.Configuration.ShowChangelogOnVersionChange;

        if (ImGui.Checkbox("Don't show changelog every update", ref dontShowChangelogEveryUpdate)) {
            plugin.Configuration.ShowChangelogOnVersionChange = !dontShowChangelogEveryUpdate;

            if (dontShowChangelogEveryUpdate)
                plugin.Configuration.CurrentPluginVersion = CurrentVersion;

            plugin.Configuration.Save();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("When enabled, this popup will not automatically appear after plugin updates.");
    }

    private void DrawHeader() {
        ImGui.TextUnformatted("The Great Controller HUD Switcher");
        ImGui.SameLine();

        ImGui.TextDisabled($"v{CurrentVersion}");

        ImGui.PushTextWrapPos();
        ImGui.TextWrapped("Thanks for updating! Here are the latest changes and recent plugin history.");
        ImGui.PopTextWrapPos();
    }

    private static void DrawContributorThanks() {
        ImGui.TextUnformatted("Special thanks");

        ImGui.PushTextWrapPos();
        ImGui.BulletText("@Aida-Enna: Updating the plugin to be Dawntrail compatible.");
        ImGui.BulletText("@grecaun, @imvaskel: Updating the plugin Dalamud API and to FFXIV patch.");
        ImGui.BulletText("@TheThirdDoor: Quality of life improvements and WASD binding.");
        ImGui.PopTextWrapPos();
    }

    private void DrawVersionEntry(ChangelogEntry entry) {
        ImGui.PushID(entry.Version);

        var isCurrentVersion = entry.Version == CurrentVersion;

        if (isCurrentVersion) {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.35f, 0.85f, 1.0f, 1.0f));
        }

        ImGui.TextUnformatted($"The Great Controller HUD Switcher {entry.Version}");

        if (isCurrentVersion) {
            ImGui.SameLine();
            ImGui.TextDisabled("Latest");
            ImGui.PopStyleColor();
        }

        ImGui.Spacing();

        ImGui.PushTextWrapPos();

        foreach (var item in entry.Items) {
            ImGui.Bullet();
            ImGui.SameLine();
            ImGui.TextWrapped(item);
        }

        ImGui.PopTextWrapPos();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.PopID();
    }

    private void DrawFooter() {
        if (ImGui.Button("Close")) {
            this.IsOpen = false;
        }

        ImGui.SameLine();

        ImGui.TextDisabled("You can show this again from the settings menu.");
    }

    private sealed record ChangelogEntry(string Version, IReadOnlyList<string> Items);

    private static readonly IReadOnlyList<ChangelogEntry> Entries = new List<ChangelogEntry> {
        new("1.0.0.0", new[] {
            "Started development and steam integration.",
        }),
    };
}