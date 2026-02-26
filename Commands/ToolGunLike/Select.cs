using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.ToolGunLike;

/// <summary>
/// Command used for selecting the objects.
/// </summary>
public class Select : ICommand
{
	/// <inheritdoc/>
	public string Command => "select";

	/// <inheritdoc/>
	public string[] Aliases { get; } = ["sel", "choose"];

	/// <inheritdoc/>
	public string Description => "Selects the object which you are looking at.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission($"mpr.{Command}"))
		{
			response = $"You don't have permission to execute this command. Required permission: mpr.{Command}";
			return false;
		}

		var player = Player.Get(sender);
		if (player is null)
		{
			response = "This command can't be run from the server console.";
			return false;
		}

		if (arguments.Count > 0)
		{
			var id = arguments.At(0);
			if (ToolGunHandler.TryGetObjectById(id, out var idObject))
			{
				ToolGunHandler.SelectObject(player, idObject);
				response = "You've successfully selected the object!";
				return true;
			}

			response = $"Unable to find object with ID of {id}!";
			return false;
		}

		// Try getting and selecting the object.
		if (ToolGunHandler.TryGetMapObject(player, out var mapEditorObject))
		{
			ToolGunHandler.SelectObject(player, mapEditorObject);
			response = "You've successfully selected the object!";
			return true;
		}

		// If object wasn't found deselect currently selected object.
		if (ToolGunHandler.TryGetSelectedMapObject(player, out var _))
		{
			ToolGunHandler.SelectObject(player, null!);
			response = "You've successfully unselected the object!";
			return false;
		}

		response = "You aren't looking at any object!";
		return false;
	}
}
