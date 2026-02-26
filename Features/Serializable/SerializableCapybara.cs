using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Attributes;
using ProjectMER.Features.Enums;
using UnityEngine;
using CapybaraToy = AdminToys.CapybaraToy;

namespace ProjectMER.Features.Serializable;

public class SerializableCapybara : SerializableObject
{
	[NoModifyProperty]
	public override ToolGunObjectType ObjectType { get; set; } = ToolGunObjectType.Capybara;
	public override void Setup(string key, MapSchematic map)
	{
		map.SpawnObject(key, this);
	}

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		var capybara = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.Capybara) : instance.GetComponent<CapybaraToy>();
		_prevIndex = Index;

		SetPosAndRot(capybara);
		capybara.transform.localScale = Scale;

		capybara.NetworkCollisionsEnabled = true;

		if (instance == null)
			NetworkServer.Spawn(capybara.gameObject);

		return capybara.gameObject;
	}
}
