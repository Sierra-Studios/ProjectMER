using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Attributes;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.ToolGun;
using ProjectMER.Features.Utility;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ProjectMER.Features.Serializable;

public class SerializableObject : EasyOwnedInheritance<SerializableObject>
{
	[YamlIgnore]
	[NoModifyProperty]
	public override bool IsDebug { get; } = false;

	[YamlIgnore]
	[NoModifyProperty]
	public override bool ShouldRegister { get; } = true;

	[NoModifyProperty]
	public virtual ToolGunObjectType ObjectType { get; set; }

	public static Dictionary<ToolGunObjectType, SerializableObject> ObjectToObjectType { get; } = [];
	
	public virtual void Setup(string key, MapSchematic map)
	{
		map.SpawnObject(key, this);
	}
	
	protected override void OnRegistered()
	{
		ToolGunItem.TypesDictionary[ObjectType] = GetType();
		ObjectToObjectType[ObjectType] = this;
		base.OnRegistered();
	}

	public static SerializableObject Get(ToolGunObjectType objectType)
	{
		return ObjectToObjectType[objectType];
	}
	
	protected override void OnUnregistered()
	{
		ToolGunItem.TypesDictionary.Remove(ObjectType);
		ObjectToObjectType.Remove(ObjectType);
		base.OnUnregistered();
	}
	
	/// <summary>
	/// Gets or sets the object's position.
	/// </summary>
	public virtual Vector3 Position { get; set; } = Vector3.zero;

	/// <summary>
	/// Gets or sets the object's rotation.
	/// </summary>
	public virtual Vector3 Rotation { get; set; } = Vector3.zero;

	/// <summary>
	/// Gets or sets the object's scale.
	/// </summary>
	public virtual Vector3 Scale { get; set; } = Vector3.one;

	public virtual string Room { get; set; } = "Unknown";

	public virtual int Index { get; set; } = -1;

	public Room? RoomUsed;
	public GameObject? InstanceUsed = null;

	public Vector3 AbsolutePosition;
	public Quaternion AbsoluteRotation;

	protected GameObject SetPosAndRot(NetworkBehaviour spawn)
	{
		return SetPosAndRot(spawn.gameObject);
	}
	
	protected GameObject SetPosAndRot(AdminToyBase spawn)
	{
		return SetPosAndRot(spawn.gameObject);
	}
	
	protected GameObject SetPosAndRot(GameObject spawn)
	{
		if (!spawn) return spawn;
		spawn.transform.SetPositionAndRotation(AbsolutePosition, AbsoluteRotation);
		return spawn;
	}
	
	public GameObject? SafeSpawn(Room? room = null, GameObject? instance = null)
	{
		if (PrefabManager.PrimitiveObject == null)
		{
			PrefabManager.RegisterPrefabs();
		}
		
		AbsolutePosition = room.GetAbsolutePosition(Position);
		AbsoluteRotation = room.GetAbsoluteRotation(Rotation);
		
		RoomUsed = room;
		InstanceUsed = instance;

		return SpawnOrUpdateObject(room, instance);
	}

	public virtual GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		Logger.Info("Did default method");
		return null;
	}

	[YamlIgnore]
	public virtual bool RequiresReloading => Index != _prevIndex;

	public int _prevIndex;
}
