global using Logger = LabApi.Features.Console.Logger;

using HarmonyLib;
using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Plugins;
using MEC;
using ProjectMER.Configs;
using ProjectMER.Events.Handlers.Internal;
using ProjectMER.Features;

namespace ProjectMER;

// ReSharper disable once ClassNeverInstantiated.Global
public class ProjectMer : Plugin<Config>
{
	public override string Name => "ProjectMER";

	public override string Description => "Map Editor for the Exiled framework.";

	public override string Author => "Michal78900";

	public override Version Version => new Version(1, 0, 0);

	public override Version RequiredApiVersion => new Version(1, 1, 5);
	
	internal Harmony? Harmony;
	internal FileSystemWatcher? MapFileSystemWatcher;

	public static ProjectMer? Singleton { get; internal set; }

	/// <summary>
	/// Gets the MapEditorReborn parent folder path.
	/// </summary>
	public static string PluginDir { get; internal set; }

	/// <summary>
	/// Gets the folder path in which the maps are stored.
	/// </summary>
	public static string MapsDir { get; internal set; }

	/// <summary>
	/// Gets the folder path in which the schematics are stored.
	/// </summary>
	public static string SchematicsDir { get; internal set; }

	public GenericEventsHandler GenericEventsHandler { get; } = new();

	public ToolGunEventsHandler ToolGunEventsHandler { get; } = new();

	public ActionOnEventHandlers AcionOnEventHandlers { get; } = new();

	public PickupEventsHandler PickupEventsHandler { get; } = new();

	public override void Enable()
	{
		Points.Init(this);
	}
	
	public override void Disable()
	{
		Points.Kill();
	}

	internal void OnMapFileChanged(object _, FileSystemEventArgs ev)
	{
		string mapName = ev.Name.Split('.')[0];
		if (!MapUtils.LoadedMaps.ContainsKey(mapName))
			return;

		Timing.CallDelayed(0.01f, () =>
		{
			try
			{
				MapUtils.LoadMap(mapName);
			}
			catch (Exception e)
			{
				Logger.Error(e);
			}
		});
	}
}
