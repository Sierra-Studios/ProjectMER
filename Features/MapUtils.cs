using ProjectMER.Features.Serializable;
using ProjectMER.Features.Serializable.Schematics;
using Utf8Json;
using YamlDotNet.Core;

namespace ProjectMER.Features;

public static class MapUtils
{
	/// <summary>
	/// The name of Untitled map, doesn't require change in the future.
	/// </summary>
	public const string UntitledMapName = "Untitled";

	/// <summary>
	/// Untitled map (default when editing)
	/// </summary>
	public static MapSchematic UntitledMap => LoadedMaps.GetOrAdd(UntitledMapName, () => new(UntitledMapName));

	/// <summary>
	/// Current Loaded maps.
	/// </summary>
	public static Dictionary<string, MapSchematic> LoadedMaps { get; private set; } = [];

	/// <summary>
	/// Saves map that can later be loaded.
	/// </summary>
	/// <param name="mapName">The name of map saved.</param>
	/// <exception cref="InvalidOperationException">Attempted to save map as Untitled</exception>
	public static void SaveMap(string mapName)
	{
		if (mapName == UntitledMapName)
			throw new InvalidOperationException("This map name is reserved for internal use!");

		if (LoadedMaps.TryGetValue(mapName, out var map)) // The Map is already loaded
		{
			map.Merge(UntitledMap);
		}
		else if (TryGetMapData(mapName, out map)) // Map isn't loaded but map file exists
		{
			map.Merge(UntitledMap);
		}
		else // Map isn't loaded and map file doesn't exist
		{
			map = new MapSchematic(mapName).Merge(UntitledMap);
		}

		var mapPath = Path.Combine(ProjectMer.MapsDir, $"{mapName}.yml");
		File.WriteAllText(mapPath, YamlParser.Serializer.Serialize(map));
		map.IsDirty = false;

		UnloadMap(UntitledMapName);
		LoadOrReload(mapName);
	}

	/// <summary>
	/// Firstly, unloads the map which is Inputted & then it's reloaded
	/// </summary>
	/// <param name="mapName"></param>
	public static void LoadOrReload(string mapName)
	{
		var map = GetMapData(mapName);
		UnloadMap(mapName);
		map.Reload();

		LoadedMaps.Add(mapName, map);
	}

	/// <summary>
	/// Destroys all object contained in map if it was already loaded and removes from Loaded Maps.
	/// </summary>
	/// <param name="mapName">The map that should be unloaded</param>
	/// <returns>true if success, false if not</returns>
	public static bool UnloadMap(string mapName)
	{
		if (!LoadedMaps.ContainsKey(mapName))
			return false;

		foreach (var mapEditorObject in LoadedMaps[mapName].SpawnedObjects)
			mapEditorObject.Destroy();

		LoadedMaps.Remove(mapName);
		return true;
	}

	/// <summary>
	/// Attempts to get map data.
	/// </summary>
	/// <param name="mapName">The mapa you want data of</param>
	/// <param name="mapSchematic">MapSchematic object</param>
	/// <returns>true if successful</returns>
	public static bool TryGetMapData(string mapName, out MapSchematic mapSchematic)
	{
		try
		{
			mapSchematic = GetMapData(mapName);
			return true;
		}
		catch (Exception)
		{
			mapSchematic = null!;
			return false;
		}
	}

	/// <summary>
	/// Method that returns map data.
	/// </summary>
	/// <param name="mapName">The map you want to get data of.</param>
	/// <returns>MapSchematic object</returns>
	/// <exception cref="FileNotFoundException">If map was not found</exception>
	/// <exception cref="YamlException">Caused when map was failed to load</exception>
	public static MapSchematic GetMapData(string mapName)
	{
		MapSchematic map;

		var path = Path.Combine(ProjectMer.MapsDir, $"{mapName}.yml");
		if (!File.Exists(path))
		{
			var error = $"Failed to load map data: File {mapName}.yml does not exist!";
			throw new FileNotFoundException(error);
		}

		try
		{
			map = YamlParser.Deserializer.Deserialize<MapSchematic>(File.ReadAllText(path));
			map.Name = mapName;
		}
		catch (YamlException e)
		{
			var error = $"Failed to load map data: File {mapName}.yml has YAML errors!\n{e.ToString().Split('\n')[0]}";
			throw new YamlException(error);
		}

		return map;
	}

	public static bool TryGetSchematicDataByName(string schematicName, out SchematicObjectDataList data)
	{
		try
		{
			data = GetSchematicDataByName(schematicName);
			return true;
		}
		catch (Exception)
		{
			data = null!;
			return false;
		}
	}

	public static SchematicObjectDataList GetSchematicDataByName(string schematicName)
	{
		SchematicObjectDataList data;
		var schematicDirPath = Path.Combine(ProjectMer.SchematicsDir, schematicName);
		var schematicJsonPath = Path.Combine(schematicDirPath, $"{schematicName}.json");
		var misplacedSchematicJsonPath = schematicDirPath + ".json";

		if (!Directory.Exists(schematicDirPath))
		{
			// Some users may throw a single JSON file into Schematics folder, this automatically creates and moved the file to the correct schematic directory.
			if (File.Exists(misplacedSchematicJsonPath))
			{
				Directory.CreateDirectory(schematicDirPath);
				File.Move(misplacedSchematicJsonPath, schematicJsonPath);
				return GetSchematicDataByName(schematicName);
			}

			var error = $"Failed to load schematic data: Directory {schematicName} does not exist!";
			Logger.Error(error);
			throw new DirectoryNotFoundException(error);
		}

		if (!File.Exists(schematicJsonPath))
		{
			// Same as above but with the folder existing and file not being there for some reason.
			if (File.Exists(misplacedSchematicJsonPath))
			{
				File.Move(misplacedSchematicJsonPath, schematicJsonPath);
				return GetSchematicDataByName(schematicName);
			}

			var error = $"Failed to load schematic data: File {schematicName}.json does not exist!";
			Logger.Error(error);
			throw new FileNotFoundException(error);
		}

		try
		{
			data = JsonSerializer.Deserialize<SchematicObjectDataList>(File.ReadAllText(schematicJsonPath));
			data.Path = schematicDirPath;
		}
		catch (JsonParsingException e)
		{
			var error = $"Failed to load schematic data: File {schematicName}.json has JSON errors!\n{e.ToString().Split('\n')[0]}";
			Logger.Error(error);
			throw new JsonParsingException(error);
		}

		return data;
	}

	public static string[] GetAvailableSchematicNames() => Directory.GetFiles(ProjectMer.SchematicsDir, "*.json", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension).Where(x => !x.Contains('-')).ToArray();

	public static string GetColoredMapName(string mapName)
	{
		if (mapName == UntitledMapName)
			return $"<color=grey><b><i>{UntitledMapName}</i></b></color>";

		var isDirty = false;
		if (LoadedMaps.TryGetValue(mapName, out var mapSchematic))
			isDirty = mapSchematic.IsDirty;

		return isDirty ? $"<i>{GetColoredString(mapName)}</i>" : GetColoredString(mapName);
	}

	public static string GetColoredString(string s)
	{
		var value = Math.Min(((uint)s.GetHashCode()) / 255, 16777215);
		var colorHex = value.ToString("X6");
		return $"<color=#{colorHex}><b>{s}</b></color>";
	}
}
