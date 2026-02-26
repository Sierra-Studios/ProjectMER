using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MEC;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.ToolGun;
using RueI.API;
using RueI.API.Elements;
using Player = LabApi.Features.Wrappers.Player;

namespace ProjectMER.Events.Handlers.Internal;

public class ToolGunEventsHandler : CustomEventsHandler
{
	private static CoroutineHandle? _toolGunCoroutine;

	public void StartCoroutine()
	{
		if (_toolGunCoroutine != null)
		{
			Timing.KillCoroutines((CoroutineHandle)_toolGunCoroutine);
		}
		
		_toolGunCoroutine = Timing.RunCoroutine(ToolGunGUI());
	}

	public void EndCoroutine()
	{
		if (_toolGunCoroutine == null)
		{
			Logger.Error("Attempted to end coroutine when didn't exist");
			return;
		}
		Timing.KillCoroutines((CoroutineHandle)_toolGunCoroutine);
		_toolGunCoroutine = null;
	}

	private static IEnumerator<float> ToolGunGUI()
	{
		while (true)
		{
			try
			{
				ToolGunAction();
			}
			catch (Exception e)
			{
				Logger.Error($"Coroutine caused exception {e}");
			}
			
			yield return Timing.WaitForSeconds(0.1f);
		}
	}

	//TODO: Debug this enough, so it actually displays hint.
	private static void ToolGunAction()
	{
		foreach (var player in Player.List)
		{
			if (!player.CurrentItem.IsToolGun(out var _) && !ToolGunHandler.TryGetSelectedMapObject(player, out var _))
				continue;

			string hud;
			try
			{
				hud = ToolGunUI.GetHintHUD(player);
			}
			catch (Exception e)
			{
				Logger.Error(e);
				hud = "ERROR: Check server console";
			}

			var display = RueDisplay.Get(player);
			if (display == null)
			{
				Logger.Error("Display is null");
				return;
			}
			
			display.Show(new Tag("Project MER Hud"), new BasicElement(300, hud + "</b>"), 0.25f);
		}
	}

	public override void OnPlayerDryFiringWeapon(PlayerDryFiringWeaponEventArgs ev)
	{
		if (!ev.Weapon.IsToolGun(out var toolGun))
			return;

		ev.IsAllowed = false;
		toolGun.Shot(ev.Player);
	}

	public override void OnPlayerReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
	{
		if (!ev.Weapon.IsToolGun(out var toolGun))
			return;

		ev.IsAllowed = false;
		toolGun.SelectedObjectToSpawn--;
	}

	public override void OnPlayerDroppingItem(PlayerDroppingItemEventArgs ev)
	{
		if (!ev.Item.IsToolGun(out var toolGun))
			return;

		ev.IsAllowed = false;
		toolGun.SelectedObjectToSpawn++;
	}
}
