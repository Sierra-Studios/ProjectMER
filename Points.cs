using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Paths;
using MEC;
using ProjectMER.Configs;
using ProjectMER.Events.Handlers.Internal;
using ProjectMER.Features;
using ProjectMER.Features.ToolGun;

namespace ProjectMER;

public class Points : CustomEventsHandler
{
    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        if(!Started) Start();
        base.OnPlayerJoined(ev);
    }

    public override void OnServerWaitingForPlayers()
    {
        if(!Started) Start();
        base.OnServerWaitingForPlayers();
    }

    public override void OnServerRoundRestarted()
    {
        End();
        base.OnServerRoundRestarted();
    }

    public static bool Started { get; set; }

    public static void Init(ProjectMer mer)
    {
        ProjectMer.Singleton = mer;
        
        mer.Harmony = new Harmony($"michal78900.mapEditorReborn-{DateTime.Now.Ticks}");
        mer.Harmony.PatchAll();
        
        ProjectMer.PluginDir = Path.Combine(PathManager.Configs.FullName, "ProjectMER");
        ProjectMer.MapsDir = Path.Combine(ProjectMer.PluginDir, "Maps");
        ProjectMer.SchematicsDir = Path.Combine(ProjectMer.PluginDir, "Schematics");
        
        if (!Directory.Exists(ProjectMer.PluginDir))
        {
            Logger.Warn("Plugin directory does not exist. Creating...");
            Directory.CreateDirectory(ProjectMer.PluginDir);
        }

        if (!Directory.Exists(ProjectMer.MapsDir))
        {
            Logger.Warn("Maps directory does not exist. Creating...");
            Directory.CreateDirectory(ProjectMer.MapsDir);
        }

        if (!Directory.Exists(ProjectMer.SchematicsDir))
        {
            Logger.Warn("Schematics directory does not exist. Creating...");
            Directory.CreateDirectory(ProjectMer.SchematicsDir);
        }
        
        if (ProjectMer.Singleton.Config!.EnableFileSystemWatcher)
        {
            ProjectMer.Singleton.MapFileSystemWatcher = new FileSystemWatcher(ProjectMer.MapsDir)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.yml",
                EnableRaisingEvents = true,
            };

            ProjectMer.Singleton.MapFileSystemWatcher.Changed += ProjectMer.Singleton.OnMapFileChanged;

            Logger.Debug("FileSystemWatcher enabled!");
        }
        
        Start();
    }
    
    public static void Start()
    {
        if (Started)
        {
            Logger.Error("For some reason called Started");
            return;
        }

        PrefabManager.RegisterPrefabs();

        MapUtils.LoadedMaps.Clear();
        ToolGunItem.ItemDictionary.Clear();
        ToolGunHandler.PlayerSelectedObjectDict.Clear();
        PickupEventsHandler.ButtonPickups.Clear();
        PickupEventsHandler.PickupUsesLeft.Clear();
        
        if (ProjectMer.Singleton == null)
        {
            Logger.Error("[Attempted registration] ProjectMer.Singleton is null, consider reporting to developer");
        }
        else
        {
            Timing.CallDelayed(0.1f,
                () =>
                {
                    ProjectMer.Singleton.AcionOnEventHandlers
                        .HandleActionList(ProjectMer.Singleton.Config?.OnWaitingForPlayers ?? []);
                });
            ProjectMer.Singleton.ToolGunEventsHandler.StartCoroutine();
            CustomHandlersManager.RegisterEventsHandler(ProjectMer.Singleton.GenericEventsHandler);
            CustomHandlersManager.RegisterEventsHandler(ProjectMer.Singleton.ToolGunEventsHandler);
            CustomHandlersManager.RegisterEventsHandler(ProjectMer.Singleton.AcionOnEventHandlers);
            CustomHandlersManager.RegisterEventsHandler(ProjectMer.Singleton.PickupEventsHandler);
        }
    }

    public static void End()
    {
        if (!Started)
        {
            Logger.Error("Attempted to End when not started");
            return;
        }
        Started = false;
        
        PrefabManager.UnregisterPrefabs();
        
        if (ProjectMer.Singleton == null)
        {
            Logger.Error("[Attempted unregistration] ProjectMer.Singleton is null, consider reporting to developer");
        }
        else
        {
            ProjectMer.Singleton.ToolGunEventsHandler.EndCoroutine();
            CustomHandlersManager.UnregisterEventsHandler(ProjectMer.Singleton.GenericEventsHandler);
            CustomHandlersManager.UnregisterEventsHandler(ProjectMer.Singleton.ToolGunEventsHandler);
            CustomHandlersManager.UnregisterEventsHandler(ProjectMer.Singleton.AcionOnEventHandlers);
            CustomHandlersManager.UnregisterEventsHandler(ProjectMer.Singleton.PickupEventsHandler);
        }
    }

    public static void Kill()
    {
        End();

        if (ProjectMer.Singleton == null)
        {
            Logger.Error("Wanted to kill, but it seems to not be started, proceeding anyway.");
        }
        else
        {
            if (ProjectMer.Singleton.MapFileSystemWatcher == null &&
                ProjectMer.Singleton.Config!.EnableFileSystemWatcher)
            {
                Logger.Error("File system watcher is dead even when configured to be true.");
            }
            else if(ProjectMer.Singleton.MapFileSystemWatcher != null)
            {
                ProjectMer.Singleton.MapFileSystemWatcher.Dispose();
            }
        }
        
        
        ProjectMer.PluginDir = "";
        ProjectMer.MapsDir = "";
        ProjectMer.SchematicsDir = "";
        
        if (ProjectMer.Singleton?.Harmony is not null)
        {
            ProjectMer.Singleton.Harmony.UnpatchAll();
            ProjectMer.Singleton.Harmony = null;
        }
        ProjectMer.Singleton = null;
    }
}