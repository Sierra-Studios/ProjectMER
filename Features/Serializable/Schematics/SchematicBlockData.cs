using AdminToys;
using LabApi.Features.Wrappers;
using ProjectMER.Events.Handlers.Internal;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace ProjectMER.Features.Serializable.Schematics;

public class SchematicBlockData
{
	public virtual string Name { get; set; }
	
	public virtual int ObjectId { get; set; }

	public virtual int ParentId { get; set; }
	
	public virtual string AnimatorName { get; set; }
	
	public virtual Vector3 Position { get; set; }

	public virtual Vector3 Rotation { get; set; }

	public virtual Vector3 Scale { get; set; }

	public virtual BlockType BlockType { get; set; }

	public virtual Dictionary<string, object> Properties { get; set; }

	public GameObject Create(SchematicObject schematicObject, Transform parentTransform)
	{
		var gameObject = BlockType switch
		{
			BlockType.Empty => CreateEmpty(),
			BlockType.Primitive => CreatePrimitive(),
			BlockType.Light => CreateLight(),
			BlockType.Pickup => CreatePickup(schematicObject),
			BlockType.Workstation => CreateWorkstation(),
			BlockType.Text => CreateText(),
			BlockType.Interactable => CreateInteractable(),
			BlockType.Waypoint => CreateWaypoint(),
			_ => CreateEmpty(true)
		};

		gameObject.name = Name;

		var transform = gameObject.transform;
		transform.SetParent(parentTransform);
		transform.SetLocalPositionAndRotation(Position, Quaternion.Euler(Rotation));

		transform.localScale = BlockType switch
		{
			BlockType.Empty when Scale == Vector3.zero => Vector3.one,
			BlockType.Waypoint => Scale * SerializableWaypoint.ScaleMultiplier,
			_ => Scale,
		};

		if (gameObject.TryGetComponent(out AdminToyBase adminToyBase))
		{
			if (Properties != null && Properties.TryGetValue("Static", out var isStatic) && Convert.ToBoolean(isStatic))
			{
				adminToyBase.NetworkIsStatic = true;
			}
			else
			{
				adminToyBase.NetworkMovementSmoothing = 60;
			}
		}

		return gameObject;
	}

	private GameObject CreateEmpty(bool fallback = false)
	{
		if (fallback)
			Logger.Warn($"{BlockType} is not yet implemented. Object will be an empty GameObject instead.");

		var primitive = GameObject.Instantiate(PrefabManager.PrimitiveObject);
		primitive.NetworkPrimitiveFlags = PrimitiveFlags.None;

		return primitive.gameObject;
	}

	private GameObject CreatePrimitive()
	{
		var primitive = GameObject.Instantiate(PrefabManager.PrimitiveObject);

		primitive.NetworkPrimitiveType = (PrimitiveType)Convert.ToInt32(Properties["PrimitiveType"]);
		primitive.NetworkMaterialColor = Properties["Color"].ToString().GetColorFromString();

		PrimitiveFlags primitiveFlags;
		if (Properties.TryGetValue("PrimitiveFlags", out var flags))
		{
			primitiveFlags = (PrimitiveFlags)Convert.ToByte(flags);
		}
		else
		{
			// Backward compatibility
			primitiveFlags = PrimitiveFlags.Visible;
			if (Scale.x >= 0f)
				primitiveFlags |= PrimitiveFlags.Collidable;
		}

		primitive.NetworkPrimitiveFlags = primitiveFlags;

		return primitive.gameObject;
	}

	private GameObject CreateLight()
	{
		var light = GameObject.Instantiate(PrefabManager.LightSource);

		light.NetworkLightType = Properties.TryGetValue("LightType", out var lightType) ? (LightType)Convert.ToInt32(lightType) : LightType.Point;
		light.NetworkLightColor = Properties["Color"].ToString().GetColorFromString();
		light.NetworkLightIntensity = Convert.ToSingle(Properties["Intensity"]);
		light.NetworkLightRange = Convert.ToSingle(Properties["Range"]);

		if (Properties.TryGetValue("Shadows", out var shadows))
		{
			// Backward compatibility
			light.NetworkShadowType = Convert.ToBoolean(shadows) ? LightShadows.Soft : LightShadows.None;
		}
		else
		{
			light.NetworkShadowType = (LightShadows)Convert.ToInt32(Properties["ShadowType"]);
			light.NetworkLightShape = (LightShape)Convert.ToInt32(Properties["Shape"]);
			light.NetworkSpotAngle = Convert.ToSingle(Properties["SpotAngle"]);
			light.NetworkInnerSpotAngle = Convert.ToSingle(Properties["InnerSpotAngle"]);
			light.NetworkShadowStrength = Convert.ToSingle(Properties["ShadowStrength"]);
		}

		return light.gameObject;
	}

	private GameObject CreatePickup(SchematicObject schematicObject)
	{
		if (Properties.TryGetValue("Chance", out var property) && UnityEngine.Random.Range(0, 101) > Convert.ToSingle(property))
			return new("Empty Pickup");

		var pickup = Pickup.Create((ItemType)Convert.ToInt32(Properties["ItemType"]), Vector3.zero)!;
		if (Properties.ContainsKey("Locked"))
			PickupEventsHandler.ButtonPickups.Add(pickup.Serial, schematicObject);

		return pickup.GameObject;
	}

	private GameObject CreateWorkstation()
	{
		var workstation = GameObject.Instantiate(PrefabManager.Workstation);
		workstation.NetworkStatus = (byte)(Properties.TryGetValue("IsInteractable", out var isInteractable) && Convert.ToBoolean(isInteractable) ? 0 : 4);

		return workstation.gameObject;
	}

	private GameObject CreateText()
	{
		var text = GameObject.Instantiate(PrefabManager.Text);

		text.TextFormat = Convert.ToString(Properties["Text"]);
		text.DisplaySize = Properties["DisplaySize"].ToVector2() * 20f;

		return text.gameObject;
	}

	private GameObject CreateInteractable()
	{
		var interactable = GameObject.Instantiate(PrefabManager.Interactable);
		interactable.NetworkShape = (InvisibleInteractableToy.ColliderShape)Convert.ToInt32(Properties["Shape"]);
		interactable.NetworkInteractionDuration = Convert.ToSingle(Properties["InteractionDuration"]);
		interactable.NetworkIsLocked = Properties.TryGetValue("IsLocked", out var isLocked) && Convert.ToBoolean(isLocked);

		return interactable.gameObject;
	}

	private GameObject CreateWaypoint()
	{
		var waypoint = GameObject.Instantiate(PrefabManager.Waypoint);
		waypoint.NetworkPriority = byte.MaxValue;

		return waypoint.gameObject;
	}
}
