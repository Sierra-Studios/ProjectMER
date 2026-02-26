using System.Reflection;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;
using ProjectMER.Features.Attributes;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using UserSettings.ServerSpecific;

namespace ProjectMER.Features.ToolGun;

public static class ToolGunUI
{
	public static string GetHintHUD(Player player)
	{
		var sb = StringBuilderPool.Shared.Rent();
		
		object instance = null!;
		List<PropertyInfo> properties = [];
		if (ToolGunHandler.TryGetSelectedMapObject(player, out var mapEditorObject) && mapEditorObject != null)
		{
			instance = mapEditorObject.GetType().GetField("Base").GetValue(mapEditorObject);
			properties = instance.GetType().GetProperties().Where(x => !NoModifyProperty.HasAttribute(x)).ToList();
		}

		if (mapEditorObject)
		{
			sb.Append($"<size=50%>MapName: {MapUtils.GetColoredMapName(mapEditorObject.MapName)}</size>");
			sb.Append("\n");
			sb.Append($"<size=50%>ID: {MapUtils.GetColoredString(mapEditorObject.Id)}</size>");
			sb.Append("\n");
		}

		foreach (var property in properties.GetColoredProperties(instance))
		{
			sb.Append($"<size=50%>");
			sb.Append(property);
			sb.Append("</size>");
			sb.Append("\n");
		}

		if (!player.CurrentItem.IsToolGun(out var toolGun))
			return StringBuilderPool.Shared.ToStringReturn(sb);

		sb.Append($"<size=50%>");
		sb.Append(GetToolGunModeString(player, toolGun));
		sb.Append("</size>");

		sb.Append("\n");


		sb.Append($"<size=50%>");
		sb.Append($"{player.Position:F3}");
		sb.Append("</size>");

		sb.Append("\n");

		sb.Append($"<size=50%>");
		sb.Append(GetRoomString(player));
		sb.Append("</size>");

		return StringBuilderPool.Shared.ToStringReturn(sb);
	}

	private static string GetToolGunModeString(Player player, ToolGunItem toolGun)
	{
		if (toolGun.CreateMode)
		{
			string output;
			if (toolGun.SelectedObjectToSpawn == ToolGunObjectType.Schematic)
			{
				if (ServerSpecificSettingsSync.TryGetSettingOfUser(player.ReferenceHub, 0, out SSDropdownSetting dropdownSetting) && dropdownSetting.TryGetSyncSelectionText(out var schematicName))
					output = schematicName.ToUpper();
				else
					output = "Please select schematic in options";
			}
			else
			{
				output = toolGun.SelectedObjectToSpawn.ToString().ToUpper();
			}

			return $"<color=green>CREATE</color>\n<color=yellow>{output}</color>";
		}

		var name = " ";
		if (ToolGunHandler.Raycast(player, out var hit))
		{
			if (MapEditorObject.TryGet(hit.transform.gameObject, false, out var mapEditorObject))
			{
				if (mapEditorObject is IndicatorObject indicatorObject)
					mapEditorObject = IndicatorObject.Dictionary[indicatorObject];
				
				if (SchematicObject.TryGet(mapEditorObject.transform.root.gameObject, false, out var schematicObject))
				{
					name = schematicObject.Name.ToUpper();
				}
				else
				{
					name = mapEditorObject.Base.ToString().Split('.').Last().Replace("Serializable", "").ToUpper();
				}
			}
		}

		if (toolGun.DeleteMode)
			return $"<color=red>DELETE</color>\n<color=yellow>{name}</color>";

		if (toolGun.SelectMode)
			return $"<color=yellow>SELECT</color>\n<color=yellow>{name}</color>";

		return "\n ";
	}

	private static string GetRoomString(Player player)
	{
		var room = RoomExtensions.GetRoomAtPosition(player.Camera.transform.position);
		var list = ListPool<Room>.Shared.Rent(Room.List.Where(x => x.Base != null && x.Zone == room.Zone && x.Shape == room.Shape && x.Name == room.Name));

		string roomString;
		if (list.Count == 1)
		{
			roomString = room.GetRoomStringId();
		}
		else
		{
			roomString = $"{room.GetRoomStringId()} ({list.IndexOf(room)}) ({list.Count})";
		}

		ListPool<Room>.Shared.Return(list);
		return roomString;
	}
}
