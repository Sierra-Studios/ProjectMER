using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Attributes;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;
using WaypointToy = AdminToys.WaypointToy;

namespace ProjectMER.Features.Serializable;

public class SerializableWaypoint : SerializableObject, IIndicatorDefinition
{
	public const float ScaleMultiplier = 1 / 256f;

	[NoModifyProperty]
	public override ToolGunObjectType ObjectType { get; set; } = ToolGunObjectType.Waypoint;
	public override void Setup(string key, MapSchematic map)
	{
		map.SpawnObject(key, this);
	}

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		var waypoint = instance == null ? GameObject.Instantiate(PrefabManager.Waypoint) : instance.GetComponent<WaypointToy>();
		
		_prevIndex = Index;

		SetPosAndRot(waypoint);
		waypoint.transform.localScale = Scale * ScaleMultiplier;
		waypoint.NetworkMovementSmoothing = 60;
		waypoint.NetworkVisualizeBounds = true;

		if (instance == null)
			NetworkServer.Spawn(waypoint.gameObject);

		return waypoint.gameObject;
	}

	public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
	{
		var primitive = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject) : instance.GetComponent<PrimitiveObjectToy>();
		var position = room.GetAbsolutePosition(Position);
		var rotation = room.GetAbsoluteRotation(Rotation);

		primitive.NetworkPrimitiveFlags = PrimitiveFlags.None;
		primitive.NetworkPrimitiveType = PrimitiveType.Cube;
		primitive.transform.localScale = Scale;
		primitive.transform.SetPositionAndRotation(position, rotation);

		return primitive.gameObject;
	}
}
