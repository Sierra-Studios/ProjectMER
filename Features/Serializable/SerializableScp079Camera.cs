using AdminToys;
using LabApi.Features.Wrappers;
using MapGeneration;
using Mirror;
using ProjectMER.Features.Attributes;
using ProjectMER.Features.Enums;
using UnityEngine;
using CameraType = ProjectMER.Features.Enums.CameraType;

namespace ProjectMER.Features.Serializable;

public class SerializableScp079Camera : SerializableObject
{
	public CameraType CameraType { get; set; } = CameraType.Lcz;
	public string Label { get; set; } = "CustomCamera";

	[NoModifyProperty]
	public override ToolGunObjectType ObjectType { get; set; } = ToolGunObjectType.Scp079Camera;
	public override void Setup(string key, MapSchematic map)
	{
		map.SpawnObject(key, this);
	}

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		Scp079CameraToy cameraVariant;
		_prevIndex = Index;

		if (instance == null)
		{
			cameraVariant = GameObject.Instantiate(CameraPrefab);
		}
		else
		{
			cameraVariant = instance.GetComponent<Scp079CameraToy>();
		}

		SetPosAndRot(cameraVariant);
		cameraVariant.transform.localScale = Scale;
		cameraVariant.NetworkScale = cameraVariant.transform.localScale;

		_prevIndex = Index;
		_prevType = CameraType;

		cameraVariant.NetworkMovementSmoothing = 60;
		cameraVariant.NetworkLabel = Label;
		cameraVariant.NetworkRoom = room == null ? LabApi.Features.Wrappers.Room.Get(RoomName.Outside).First().Base : room.Base;

		if (instance == null)
			NetworkServer.Spawn(cameraVariant.gameObject);

		return cameraVariant.gameObject;
	}


	private Scp079CameraToy CameraPrefab
	{
		get
		{
			var prefab = CameraType switch
			{
				CameraType.Lcz => PrefabManager.CameraLcz,
				CameraType.Hcz => PrefabManager.CameraHcz,
				CameraType.Ez => PrefabManager.CameraEz,
				CameraType.EzArm => PrefabManager.CameraEzArm,
				CameraType.Sz => PrefabManager.CameraSz,
				_ => throw new InvalidOperationException(),
			};

			return prefab;
		}
	}

	public override bool RequiresReloading => CameraType != _prevType || base.RequiresReloading;

	internal CameraType _prevType;
}
