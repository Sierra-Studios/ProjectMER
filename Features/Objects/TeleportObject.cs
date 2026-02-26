using LabApi.Features.Wrappers;
using ProjectMER.Features.Serializable;
using UnityEngine;

namespace ProjectMER.Features.Objects;

public class TeleportObject : MonoBehaviour
{
	private void Start()
	{
		_mapEditorObject = MapEditorObject.Get(gameObject)!;
		Base = (SerializableTeleport)_mapEditorObject.Base;
	}

	public SerializableTeleport Base;
	private MapEditorObject _mapEditorObject;

	public DateTime NextTimeUse;

	public TeleportObject? GetRandomTarget()
	{
		var targetId = Base.Targets.RandomItem();

		foreach (var teleportObject in FindObjectsByType<TeleportObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			if (teleportObject._mapEditorObject.Id != targetId)
				continue;

			return teleportObject;
		}

		return null;
	}

	public void OnTriggerEnter(Collider other)
	{
		var player = Player.Get(other.gameObject);
		if (player is null)
			return;

		if (NextTimeUse > DateTime.Now)
			return;

		var target = GetRandomTarget();
		if (target == null)
			return;

		var dateTime = DateTime.Now.AddSeconds(Base.Cooldown);
		NextTimeUse = dateTime;
		target.NextTimeUse = dateTime;

		player.Position = target.gameObject.transform.position;
		player.LookRotation = target.gameObject.transform.eulerAngles;
	}
}
