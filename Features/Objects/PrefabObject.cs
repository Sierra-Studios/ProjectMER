using AdminToys;
using Exiled.API.Enums;
using Exiled.API.Features;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using ProjectMER.Features.Serializable;
using ProjectMER.Features.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;
using Room = LabApi.Features.Wrappers.Room;

namespace ProjectMER.Features.Objects;

public class PrefabObject : CachedMonobehaviour<PrefabObject>
{
    private MapEditorObject _mapEditorObject;
    public SerializablePrefabObject Base;
    private PrefabType _lastType = PrefabType.PrimitiveObjectToy;
    private bool _spawned = false;
    public GameObject instantiated;
    public NetworkBehaviour networkBehaviour;
    private void Start()
    {
        _mapEditorObject = MapEditorObject.Get(gameObject)!;
        Base = (SerializablePrefabObject)_mapEditorObject.Base;
        RemoveAndSpawnPrefab();
    }

    private bool first = false;
    
    private void Update()
    {
        if (!instantiated)
        {
            SpawnPrefab();
        }
        
        if (!first)
        {
            RemoveAndSpawnPrefab();
            first = true;
        }
        
        if (_lastType != Base.Type)
        {
            _lastType = Base.Type;
            RemoveAndSpawnPrefab();
        }

        if (_spawned && (instantiated.transform.position != transform.position || instantiated.transform.rotation != transform.rotation))
        {
            RemoveAndSpawnPrefab();
        }
    }

    public void RemoveAndSpawnPrefab()
    {
        RemoveOld();
        SpawnPrefab();
    }
    
    private void RemoveOld()
    {
        try
        {
            if (!_spawned || !instantiated) return;
            _spawned = false;
            MapEditorObject.HasObject.Remove(instantiated);
            Destroy(instantiated);
        }
        catch (Exception e)
        {
            // ignored
        }
    }
    
    private void SpawnPrefab()
    {
        if (!_spawned) _spawned = true;
        instantiated = PrefabManager.Spawn(_lastType, transform.position, transform.rotation, false);
        if (!instantiated)
        {
            return;
            
        }
        
        instantiated.transform.localScale = transform.localScale;
        NetworkServer.Spawn(instantiated);
        MapEditorObject.HasObject[instantiated] = _mapEditorObject;
    }

    private void OnDestroy()
    {
        RemoveOld();
    }
}

public class SerializablePrefabObject: SerializableObject, IIndicatorDefinition
{
    public PrefabType Type { get; set; } = PrefabType.PrimitiveObjectToy;
    
    public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
    {
        var prefabObject = PrefabObject.Get(instance);
        //PrimitiveObjectToy primitive = instance == null ? Object.Instantiate(PrefabManager.PrimitiveObject) : instance.GetComponent<PrimitiveObjectToy>();
        GameObject primitive = instance == null ? new GameObject("prefab object") : instance;
        Vector3 position = room.GetAbsolutePosition(Position);
        Quaternion rotation = room.GetAbsoluteRotation(Rotation);
        _prevIndex = Index;

        primitive.transform.SetPositionAndRotation(position, rotation);
        primitive.transform.localScale = Scale;
        
        prefabObject?.RemoveAndSpawnPrefab();
        
        if (instance == null)
        {
            NetworkServer.Spawn(primitive.gameObject);
            primitive.gameObject.AddComponent<PrefabObject>();
        }
        
        return primitive.gameObject;
    }

    public SerializablePrimitive Indicator { get; set; } = new SerializablePrimitive();
    
    public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
    {
        Indicator.Scale = new Vector3(0.2f, 0.2f, 0.2f);
        Indicator.Color = "#292f56";
        instance = Indicator.SpawnOrUpdateObject(room, instance);
        return instance;
    }
}