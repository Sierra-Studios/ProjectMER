using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Events.Arguments;
using ProjectMER.Events.Handlers;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable.Schematics;

public class SerializableSchematic : SerializableObject
{
	public string SchematicName { get; set; } = "None";

	public override ToolGunObjectType ObjectType { get; set; } = ToolGunObjectType.Schematic;
	public override void Setup(string key, MapSchematic map)
	{
		map.SpawnObject(key, this);
	}

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		var schematic = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject) : instance.GetComponent<PrimitiveObjectToy>();
		schematic.NetworkPrimitiveFlags = PrimitiveFlags.None;
		schematic.NetworkMovementSmoothing = 60;

		var position = room.GetAbsolutePosition(Position);
		var rotation = room.GetAbsoluteRotation(Rotation);
		_prevIndex = Index;

		schematic.name = $"CustomSchematic-{SchematicName}";
		schematic.transform.SetPositionAndRotation(position, rotation);
		schematic.transform.localScale = Scale;

		if (instance == null)
		{
			_ = MapUtils.TryGetSchematicDataByName(SchematicName, out var data) ? data : null;

			if (data == null)
			{
				GameObject.Destroy(schematic.gameObject);
				return null;
			}

			SchematicSpawningEventArgs ev = new(data, SchematicName);
			Schematic.OnSchematicSpawning(ev);
			data = ev.Data;

			if (!ev.IsAllowed)
			{
				GameObject.Destroy(schematic.gameObject);
				return null;
			}
			
			NetworkServer.Spawn(schematic.gameObject);
			schematic.gameObject.AddComponent<SchematicObject>().Init(data);
		}

		return schematic.gameObject;
	}
}
