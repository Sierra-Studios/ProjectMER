using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Paths;
using MEC;
using ProjectMER.Events.Handlers.Internal;
using ProjectMER.Features;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Serializable;
using ProjectMER.Features.ToolGun;

namespace ProjectMER;

public class Points : CustomEventsHandler
{
    public static Points Instance;
    public static bool Started { get; set; } = false;
    
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

    public static MEROptimizer.Application.MEROptimizer Application;

    public static void Init(ProjectMer mer)
    {
        Instance = new Points();
        ProjectMer.Singleton = mer;
        
        SerializableObject.RegisterAll();
        
        CustomHandlersManager.RegisterEventsHandler(Instance);
        
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

        Application = new MEROptimizer.Application.MEROptimizer();
        Application.Load(ProjectMer.Singleton.Config.OptimizerConfig);
        
        Start();
    }
    
    public static void Start()
    {
        if (Started)
        {
            Logger.Error("For some reason called Started");
            return;
        }

        Timing.CallDelayed(1f, PrefabManager.RegisterPrefabs);

        MapUtils.LoadedMaps.Clear();
        ToolGunItem.ItemDictionary.Clear();
        ToolGunHandler.PlayerSelectedObjectDict.Clear();
        PickupEventsHandler.ButtonPickups.Clear();
        PickupEventsHandler.PickupUsesLeft.Clear();

        Started = true;
    }

    public static void End()
    {
        if (!Started)
        {
            Logger.Error("Attempted to End when not started");
            return;
        }
        Started = false;

        RoomExtensions.RoomIndex = [];
        PrefabManager.UnregisterPrefabs();
    }

    public static void Kill()
    {
        End();
        
        Application.Unload();
        Application = null;
        
        SerializableObject.UnregisterAll();
        SerializableObject.ObjectToObjectType.Clear();
        
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
        
        CustomHandlersManager.UnregisterEventsHandler(Instance);
        Instance = null;
        
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