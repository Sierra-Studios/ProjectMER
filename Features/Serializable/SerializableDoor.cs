using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Attributes;
using ProjectMER.Features.Enums;
using UnityEngine;

namespace ProjectMER.Features.Serializable;

public class SerializableDoor : SerializableObject
{
	public DoorType DoorType { get; set; } = DoorType.Lcz;
	public bool IsOpen { get; set; } = false;
	public bool IsLocked { get; set; } = false;
	public DoorPermissionFlags RequiredPermissions { get; set; } = DoorPermissionFlags.None;
	public bool RequireAll { get; set; } = true;

	[NoModifyProperty]
	public override ToolGunObjectType ObjectType { get; set; } = ToolGunObjectType.Door;
	public override void Setup(string key, MapSchematic map)
	{
		var vanillaDoor = Door.Get(key);
		if (vanillaDoor != null)
		{
			SetupDoor(vanillaDoor.Base);
			return;
		}

		map.SpawnObject(key, this);
	}

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		DoorVariant doorVariant;
		_prevIndex = Index;

		if (instance == null)
		{
			doorVariant = GameObject.Instantiate(DoorPrefab);
			if (doorVariant.TryGetComponent(out DoorRandomInitialStateExtension doorRandomInitialStateExtension))
				GameObject.Destroy(doorRandomInitialStateExtension);
		}
		else
		{
			doorVariant = instance.GetComponent<DoorVariant>();
		}

		SetPosAndRot(doorVariant);
		doorVariant.transform.localScale = Scale;

		_prevType = DoorType;
		SetupDoor(doorVariant);

		NetworkServer.UnSpawn(doorVariant.gameObject);
		NetworkServer.Spawn(doorVariant.gameObject);

		return doorVariant.gameObject;
	}

	public void SetupDoor(DoorVariant doorVariant)
	{
		doorVariant.NetworkTargetState = IsOpen;
		doorVariant.ServerChangeLock(DoorLockReason.SpecialDoorFeature, IsLocked);
		doorVariant.RequiredPermissions = new DoorPermissionsPolicy(RequiredPermissions, RequireAll);
	}

	private DoorVariant DoorPrefab
	{
		get
		{
			var prefab = DoorType switch
			{
				DoorType.Lcz => PrefabManager.DoorLcz,
				DoorType.Hcz => PrefabManager.DoorHcz,
				DoorType.Ez => PrefabManager.DoorEz,
				DoorType.Bulk => PrefabManager.DoorHeavyBulk,
				DoorType.Gate => PrefabManager.DoorGate,
				_ => throw new InvalidOperationException(),
			};

			return prefab;
		}
	}

	public override bool RequiresReloading => DoorType != _prevType || base.RequiresReloading;

	internal DoorType _prevType;
}
