using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using System.IO;
using System.Reflection;

using XIVSteamInput.Windows;

namespace XIVSteamInput;

public sealed class Plugin : IDalamudPlugin {
    private const string CommandName = "/psi";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;


    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("XIVSteamInput");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private ChangelogWindow ChangelogWindow { get; init; }

    public Plugin() {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        var currentVersion = PluginInterface.GetPlugin(Assembly.GetExecutingAssembly())!.Version.ToString();


        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        ChangelogWindow = new ChangelogWindow(this, currentVersion);

        WindowSystem.AddWindow(ChangelogWindow);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        HandleCommandIntegration(isRegistration: true);

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;

        ChangelogWindow.OpenIfNeeded();

        Log.Information($"=== Start initialization complete for {PluginInterface.Manifest.Name} ===");
    }

    public void Dispose() {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        HandleCommandIntegration(isRegistration: false);
    }

    private void OnMainChatCommand(string command, string args) {
        if (!string.IsNullOrWhiteSpace(args)) {
            switch (args) {
            case "changelog" or "cl":
                OpenChangelogUI();
                return;
            }
        }

        // In response to the slash command, toggle the display status of our main ui
        MainWindow.Toggle();
    }

    private void HandleCommandIntegration(bool isRegistration) {
        if (isRegistration) {
            CommandManager.AddHandler(CommandName, new CommandInfo(OnMainChatCommand) {
                HelpMessage = "Show the Steam Input configuration window"
            });
        }
        else {
            CommandManager.RemoveHandler(CommandName);
        }
    }

    public void OpenConfigUi() => ConfigWindow.IsOpen = true;
    public void OpenMainUi()   => MainWindow.IsOpen = true;
    internal void OpenChangelogUI() => ChangelogWindow.IsOpen = true;

}
