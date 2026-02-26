using LabApi.Features.Wrappers;
using MapGeneration;
using NorthwoodLib.Pools;
using ProjectMER.Features.Serializable;
using UnityEngine;

namespace ProjectMER.Features.Extensions;

public static class RoomExtensions
{
	public static Room GetRoomAtPosition(Vector3 position) => Room.TryGetRoomAtPosition(position, out var room) ? room : Room.List.First(x => x.Base != null && x.Name == RoomName.Outside);

	public static string GetRoomStringId(this Room room) => $"{room.Zone}_{room.Shape}_{room.Name}";

	public static List<Room> GetRooms(this SerializableObject serializableObject)
	{
		var split = serializableObject.Room.Split('_');
		if (split.Length != 3)
			return ListPool<Room>.Shared.Rent(Room.List.Where(x => x.Base != null && x.Name == RoomName.Outside));

		var facilityZone = (FacilityZone)Enum.Parse(typeof(FacilityZone), split[0], true);
		var roomShape = (RoomShape)Enum.Parse(typeof(RoomShape), split[1], true);
		var roomName = (RoomName)Enum.Parse(typeof(RoomName), split[2], true);

		return ListPool<Room>.Shared.Rent(Room.List.Where(x => x.Base != null && x.Zone == facilityZone && x.Shape == roomShape && x.Name == roomName));
	}

	public static Dictionary<Room, int> RoomIndex = [];
	
	public static int GetRoomIndex(this Room room)
	{
		if (RoomIndex.TryGetValue(room, out var roomIndex)) return roomIndex;
		var list = ListPool<Room>.Shared.Rent(Room.List.Where(x => x.Base != null && x.Zone == room.Zone && x.Shape == room.Shape && x.Name == room.Name));
		var index = list.IndexOf(room);
		RoomIndex[room] = index;
		ListPool<Room>.Shared.Return(list);
		return index;
	}

	public static Vector3 GetAbsolutePosition(this Room? room, Vector3 position)
	{
		if (room is null || room.Name == RoomName.Outside)
			return position;

		return room.Transform.TransformPoint(position);
	}

	public static Quaternion GetAbsoluteRotation(this Room? room, Vector3 eulerAngles)
	{
		if (room is null || room.Name == RoomName.Outside)
			return Quaternion.Euler(eulerAngles);

		return room.Transform.rotation * Quaternion.Euler(eulerAngles);
	}
}
