using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;

namespace ProjectMER.Events.Handlers.Internal;

public class GenericEventsHandler : CustomEventsHandler
{
	public override void OnPlayerSpawning(PlayerSpawningEventArgs ev)
	{
		if (!ev.UseSpawnPoint)
			return;

		List<MapEditorObject> list = [];
		foreach (var map in MapUtils.LoadedMaps.Values)
		{
			foreach (var spawnPoint in map.PlayerSpawnpoints)
			{
				if (!spawnPoint.Value.Roles.Contains(ev.Role.RoleTypeId))
					continue;

				list.AddRange(map.SpawnedObjects.Where(x => x.Id == spawnPoint.Key));
			}
		}

		if (list.Count == 0)
			return;

		var randomElement = list[UnityEngine.Random.Range(0, list.Count)];
		
		ev.SpawnLocation = randomElement.transform.position;
		Timing.CallDelayed(0.05f, () =>
		{
			try
			{
				ev.Player.LookRotation = randomElement.transform.eulerAngles;
			}
			catch (Exception e)
			{
				Logger.Error(e);
			}
		});
	}

	public override void OnPlayerInteractingShootingTarget(PlayerInteractingShootingTargetEventArgs ev)
	{
		//Created Get method to attempt not using slower methods, uses more memory, which is not in this case bad as this is small plugin.
		if (MapEditorObject.Get(ev.ShootingTarget.GameObject) != null)
		{
			ev.IsAllowed = false;
		}
	}
}
