using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Attributes;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;
using TextToy = AdminToys.TextToy;

namespace ProjectMER.Features.Serializable;

public class SerializableText : SerializableObject, IIndicatorDefinition
{
	public string Text { get; set; } = "Custom Text";

	public Vector3 DisplaySize { get; set; } = TextToy.DefaultDisplaySize;

	[NoModifyProperty]
	public override ToolGunObjectType ObjectType { get; set; } = ToolGunObjectType.Text;
	public override void Setup(string key, MapSchematic map)
	{
		map.SpawnObject(key, this);
	}

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		var text = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.Text) : instance.GetComponent<TextToy>();
		
		_prevIndex = Index;

		SetPosAndRot(text);
		text.transform.localScale = Scale;
		text.NetworkMovementSmoothing = 60;

		text.Network_textFormat = Text;
		text.Network_displaySize = DisplaySize;

		if (instance == null)
			NetworkServer.Spawn(text.gameObject);

		return text.gameObject;
	}
	
	public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
	{
		PrimitiveObjectToy cube;

		var position = room.GetAbsolutePosition(Position);
		var rotation = room.GetAbsoluteRotation(Rotation);

		if (instance == null)
		{
			cube = UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject);
			cube.NetworkPrimitiveType = PrimitiveType.Cube;
			cube.NetworkPrimitiveFlags = PrimitiveFlags.Visible;
			cube.NetworkMaterialColor = new Color(1f, 1f, 1f, 0.9f);
			cube.transform.localScale = Vector3.one * 0.25f;
		}
		else
		{
			cube = instance.GetComponent<PrimitiveObjectToy>();
		}

		cube.transform.SetPositionAndRotation(position, rotation);

		return cube.gameObject;
	}
}
