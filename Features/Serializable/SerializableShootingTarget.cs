using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Attributes;
using ProjectMER.Features.Enums;
using UnityEngine;

namespace ProjectMER.Features.Serializable;

public class SerializableShootingTarget : SerializableObject
{
	public TargetType TargetType { get; set; } = TargetType.ClassD;

	[NoModifyProperty]
	public override ToolGunObjectType ObjectType { get; set; } = ToolGunObjectType.ShootingTarget;
	public override void Setup(string key, MapSchematic map)
	{
		map.SpawnObject(key, this);
	}

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		var shootingTarget = instance == null ? UnityEngine.Object.Instantiate(TargetPrefab) : instance.GetComponent<ShootingTarget>();
		
		SetPosAndRot(shootingTarget);
		shootingTarget.transform.localScale = Scale;
		
		_prevType = TargetType;
		
		if (instance == null)
			NetworkServer.Spawn(shootingTarget.gameObject);

		return shootingTarget.gameObject;
	}
	
	private ShootingTarget TargetPrefab
	{
		get
		{
			var prefab = TargetType switch
			{
				TargetType.Binary => PrefabManager.ShootingTargetBinary,
				TargetType.ClassD => PrefabManager.ShootingTargetDBoy,
				TargetType.Sport => PrefabManager.ShootingTargetSport,
				_ => throw new InvalidOperationException(),
			};

			return prefab;
		}
	}
	
	public override bool RequiresReloading => TargetType != _prevType || base.RequiresReloading;

	internal TargetType _prevType;
}
