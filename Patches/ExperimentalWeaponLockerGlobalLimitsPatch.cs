using HarmonyLib;
using MapGeneration.Distributors;
using NorthwoodLib.Pools;
using ProjectMER.Features.Objects;

using Random = UnityEngine.Random;

namespace ProjectMER.Patches;

[HarmonyPatch(typeof(ExperimentalWeaponLocker), nameof(ExperimentalWeaponLocker.FillChamber))]
public static class ExperimentalWeaponLockerGlobalLimitsPatch
{
	public static bool Prefix(ExperimentalWeaponLocker __instance, LockerChamber ch)
	{
		// Ignore if it's naturaly spawned locker.
		if (!__instance.transform.TryGetComponentInParent(out MapEditorObject _))
			return true;

		var compatibleLoot = ListPool<int>.Shared.Rent();

		for (var i = 0; i < __instance.Loot.Length; i++)
		{
			var loot = __instance.Loot[i];

			if (loot.RemainingUses <= 0)
				continue;

			if (ch.AcceptableItems.Length > 0 && !ch.AcceptableItems.Contains(loot.TargetItem))
				continue;

			for (var x = 0; x <= loot.ProbabilityPoints; x++)
				compatibleLoot.Add(i);
		}

		if (compatibleLoot.Count > 0)
		{
			var randLoot = compatibleLoot[Random.Range(0, compatibleLoot.Count)];
			var loot = __instance.Loot[randLoot];

			ch.SpawnItem(loot.TargetItem, Random.Range(loot.MinPerChamber, loot.MaxPerChamber + 1));
			loot.RemainingUses--;
		}

		ListPool<int>.Shared.Return(compatibleLoot);

		return false;
	}
}
