using AdminToys;
using Exiled.API.Enums;
using Exiled.API.Features;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using ProjectMER.Features.Serializable;
using UnityEngine;
using Object = UnityEngine.Object;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;
using Room = LabApi.Features.Wrappers.Room;

namespace ProjectMER.Features.Objects;

public class PrefabObject : MonoBehaviour
{
    private MapEditorObject _mapEditorObject;
    public SerializablePrefabObject Base;
    private PrefabType _lastType = PrefabType.PrimitiveObjectToy;
    private bool _spawned = false;
    private GameObject _instantiated;
    private void Start()
    {
        _mapEditorObject = MapEditorObject.Get(gameObject)!;
        Base = (SerializablePrefabObject)_mapEditorObject.Base;
        RemoveAndSpawnPrefab();
    }

    private void Update()
    {
        if (_lastType != Base.PrefabType)
        {
            _lastType = Base.PrefabType;
            RemoveAndSpawnPrefab();
        }
    }

    private void RemoveAndSpawnPrefab()
    {
        RemoveOld();
        SpawnPrefab();
    }
    
    private void RemoveOld()
    {
        if (!_spawned) return;
        MapEditorObject.Remove(_instantiated);
        Destroy(_instantiated);
    }
    
    private void SpawnPrefab()
    {
        if (!_spawned) _spawned = true;
        _instantiated = PrefabHelper.Spawn(_lastType, transform.localPosition, transform.localRotation);
        
        _instantiated.transform.parent = transform;
        
        _instantiated.transform.localPosition = transform.localPosition;
        _instantiated.transform.localRotation = transform.localRotation;
        _instantiated.transform.localScale = transform.localScale;
        
        MapEditorObject.Set(_instantiated, _mapEditorObject);
    }
}

public class SerializablePrefabObject: SerializableObject
{
    public PrefabType PrefabType { get; set; } = PrefabType.PrimitiveObjectToy;
    
    public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
    {
        AdminToys.PrimitiveObjectToy primitive = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject) : instance.GetComponent<PrimitiveObjectToy>();
        Vector3 position = room.GetAbsolutePosition(Position);
        Quaternion rotation = room.GetAbsoluteRotation(Rotation);
        _prevIndex = Index;

        primitive.transform.SetPositionAndRotation(position, rotation);
        primitive.transform.localScale = Scale;
        primitive.NetworkMovementSmoothing = 60;

        primitive.NetworkMaterialColor = Color.white;
        primitive.NetworkPrimitiveType = PrimitiveType.Cube;
        primitive.NetworkPrimitiveFlags = PrimitiveFlags.None;

        if (instance == null)
            NetworkServer.Spawn(primitive.gameObject);

        return primitive.gameObject;
    }
}