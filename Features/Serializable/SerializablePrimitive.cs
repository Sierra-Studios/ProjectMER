
using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Attributes;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable;

public class SerializablePrimitive : SerializableObject
{
	/// <summary>
	/// Gets or sets the <see cref="UnityEngine.PrimitiveType"/>.
	/// </summary>
	public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Cube;

	/// <summary>
	/// Gets or sets the <see cref="SerializablePrimitive"/>'s color.
	/// </summary>
	public string Color { get; set; } = "#FF0000";

	/// <summary>
	/// Gets or sets the <see cref="SerializablePrimitive"/>'s flags.
	/// </summary>
	public PrimitiveFlags PrimitiveFlags { get; set; } = (PrimitiveFlags)3;

	[NoModifyProperty]
	public override ToolGunObjectType ObjectType { get; set; } = ToolGunObjectType.Primitive;
	public override void Setup(string key, MapSchematic map)
	{
		map.SpawnObject(key, this);
	}

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		var primitive = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject) : instance.GetComponent<PrimitiveObjectToy>();
		_prevIndex = Index;

		SetPosAndRot(primitive);
		primitive.transform.localScale = Scale;
		primitive.NetworkMovementSmoothing = 60;

		primitive.NetworkMaterialColor = Color.GetColorFromString();
		primitive.NetworkPrimitiveType = PrimitiveType;
		primitive.NetworkPrimitiveFlags = PrimitiveFlags;

		if (instance == null)
			NetworkServer.Spawn(primitive.gameObject);

		return primitive.gameObject;
	}
}
