using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using UnityEngine;

using Room = LabApi.Features.Wrappers.Room;
using LabApiLocker = LabApi.Features.Wrappers.Locker;

namespace ProjectMER.Features.Serializable.Lockers;

public class SerializableLocker : SerializableObject
{
	public LockerType LockerType { get; set; } = LockerType.PedestalScp500;

	public List<SerializableLockerLoot> Loot { get; set; } = [];

	public List<SerializableLockerChamber> Chambers { get; set; } = [];

	public override ToolGunObjectType ObjectType { get; set; } = ToolGunObjectType.Locker;
	public override void Setup(string key, MapSchematic map)
	{
		_prevType = LockerType;
		map.SpawnObject(key, this);
	}

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		var locker = instance == null ? UnityEngine.Object.Instantiate(LockerPrefab) : instance.GetComponent<Locker>();
		var position = room.GetAbsolutePosition(Position);
		var rotation = room.GetAbsoluteRotation(Rotation);
		_prevIndex = Index;

		locker.transform.SetPositionAndRotation(position, rotation);
		locker.transform.localScale = Scale;

		if (locker.TryGetComponent(out StructurePositionSync structurePositionSync))
		{
			structurePositionSync.Network_position = locker.transform.position;
			structurePositionSync.Network_rotationY = (sbyte)Mathf.RoundToInt(locker.transform.rotation.eulerAngles.y / 5.625f);
		}

		var labApiLocker = LabApiLocker.Get(locker);
		if (LockerType != _prevType)
			SetDefaultSettings(labApiLocker);

		labApiLocker.ClearLockerLoot();
		foreach (var loot in Loot)
		{
			labApiLocker.AddLockerLoot(loot.TargetItem, loot.RemainingUses, loot.ProbabilityPoints, loot.MinPerChamber, loot.MaxPerChamber);
		}

		var i = 0;
		labApiLocker.ClearAllChambers();
		foreach (var chamber in labApiLocker.Chambers)
		{
			if (i > Chambers.Count - 1)
				break;

			chamber.AcceptableItems = Chambers[i].AcceptableItems.ToArray();
			chamber.RequiredPermissions = Chambers[i].RequiredPermissions;
			i++;
		}

		_prevType = LockerType;
		NetworkServer.UnSpawn(locker.gameObject);
		NetworkServer.Spawn(locker.gameObject);

		Timing.CallDelayed(0.25f, () =>
		{
			foreach (var itemPickupBase in locker.GetComponentsInChildren<ItemPickupBase>())
			{
				if (itemPickupBase.TryGetComponent(out Rigidbody rigidbody))
					rigidbody.isKinematic = false;
			}

			var i = 0;
			foreach (var chamber in labApiLocker.Chambers)
			{
				chamber.IsOpen = Chambers[i].IsOpen;
				i++;
			}
		});

		return locker.gameObject;
	}

	private void SetDefaultSettings(LabApiLocker labApiLocker)
	{
		Loot.Clear();
		Chambers.Clear();

		foreach (var loot in labApiLocker.Loot)
		{
			Loot.Add(new SerializableLockerLoot(loot.TargetItem, loot.RemainingUses, loot.MaxPerChamber, loot.ProbabilityPoints, loot.MinPerChamber));
		}

		foreach (var chamber in labApiLocker.Chambers)
		{
			Chambers.Add(new SerializableLockerChamber(chamber.AcceptableItems, chamber.IsOpen, chamber.RequiredPermissions));
		}
	}

	private Locker LockerPrefab
	{
		get
		{
			var prefab = LockerType switch
			{
				LockerType.PedestalScp500 => PrefabManager.PedestalScp500,
				LockerType.LargeGun => PrefabManager.LockerLargeGun,
				LockerType.RifleRack => PrefabManager.LockerRifleRack,
				LockerType.Misc => PrefabManager.LockerMisc,
				LockerType.Medkit => PrefabManager.LockerRegularMedkit,
				LockerType.Adrenaline => PrefabManager.LockerAdrenalineMedkit,
				LockerType.PedestalScp018 => PrefabManager.PedestalScp018,
				LockerType.PedestalScp207 => PrefabManager.PedstalScp207,
				LockerType.PedestalScp244 => PrefabManager.PedestalScp244,
				LockerType.PedestalScp268 => PrefabManager.PedestalScp268,
				LockerType.PedestalScp1853 => PrefabManager.PedstalScp1853,
				LockerType.PedestalScp2176 => PrefabManager.PedestalScp2176,
				LockerType.PedestalScpScp1576 => PrefabManager.PedestalScp1576,
				LockerType.PedestalAntiScp207 => PrefabManager.PedestalAntiScp207,
				LockerType.PedestalScp1344 => PrefabManager.PedestalScp1344,
				LockerType.ExperimentalWeapon => PrefabManager.LockerExperimentalWeapon,
				_ => throw new InvalidOperationException(),
			};

			return prefab;
		}
	}

	public override bool RequiresReloading => true;

	internal LockerType _prevType = LockerType.None;
}
