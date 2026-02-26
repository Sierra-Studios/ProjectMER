using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Pickups;
using LabApi.Features.Wrappers;
using MEC;
using ProjectMER.Events.Handlers.Internal;
using ProjectMER.Features.Attributes;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable;

public class SerializableItemSpawnpoint : SerializableObject, IIndicatorDefinition
{
	public ItemType ItemType { get; set; } = ItemType.Lantern;
	public float Weight { get; set; } = -1;
	public string AttachmentsCode { get; set; } = "-1";
	public uint NumberOfItems { get; set; } = 1;
	public int NumberOfUses { get; set; } = 1;
	public bool UseGravity { get; set; } = true;
	public bool CanBePickedUp { get; set; } = true;

	[NoModifyProperty]
	public override ToolGunObjectType ObjectType { get; set; } = ToolGunObjectType.ItemSpawnpoint;
	public override void Setup(string key, MapSchematic map)
	{
		map.SpawnObject(key, this);
	}

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		var itemSpawnPoint = instance ?? new GameObject("ItemSpawnpoint");
		var position = room.GetAbsolutePosition(Position);
		var rotation = room.GetAbsoluteRotation(Rotation);
		_prevIndex = Index;

		itemSpawnPoint.transform.SetPositionAndRotation(position, rotation);

		if (instance != null)
		{
			foreach (var pickup in instance.GetComponentsInChildren<ItemPickupBase>())
			{
				PickupEventsHandler.PickupUsesLeft.Remove(pickup.Info.Serial);
				pickup.DestroySelf();
			}
		}

		for (var i = 0; i < NumberOfItems; i++)
		{
			var pickup = Pickup.Create(ItemType, position, rotation, Scale)!;

			pickup.Transform.parent = itemSpawnPoint.transform;
			if (Weight != -1)
				pickup.Weight = Weight;

			pickup.Rigidbody!.isKinematic = !UseGravity;
			pickup.IsLocked = !CanBePickedUp;
			PickupEventsHandler.PickupUsesLeft.Add(pickup.Serial, NumberOfUses);

			pickup.Spawn();

			if (pickup is FirearmPickup firearmPickup)
			{
				Timing.CallDelayed(0.01f, () =>
				{
					firearmPickup.Base.OnDistributed();
					firearmPickup.AttachmentCode = uint.TryParse(AttachmentsCode, out var attachmentsCode) ? attachmentsCode : AttachmentsUtils.GetRandomAttachmentsCode(firearmPickup.Type);
					if (firearmPickup.Base.Template.TryGetModule(out MagazineModule magazineModule))
						magazineModule.ServerResyncData();
				});
			}
		}

		return itemSpawnPoint.gameObject;
	}
	public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
	{
		PrimitiveObjectToy cube;
		

		if (instance == null)
		{
			cube = UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject);
			cube.NetworkPrimitiveType = PrimitiveType.Cube;
			cube.NetworkPrimitiveFlags = AdminToys.PrimitiveFlags.Visible;
			cube.NetworkMaterialColor = new Color(0f, 1f, 0f, 0.9f);
			cube.transform.localScale = Vector3.one * 0.25f;
		}
		else
		{
			cube = instance.GetComponent<PrimitiveObjectToy>();
		}

		SetPosAndRot(cube);

		return cube.gameObject;
	}
}
