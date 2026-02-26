using UnityEngine;

namespace ProjectMER.Features.Utility;

public class CachedMonobehaviour<TSelf> : MonoBehaviour where TSelf : CachedMonobehaviour<TSelf>
{
    public static Dictionary<GameObject, TSelf> HasObject { get; } = [];

    public static bool TryGet(GameObject? gameObject, bool fromChildren, out TSelf self)
    {
        self = Get(gameObject, fromChildren);
        return self;
    }
    
    public static TSelf? Get(GameObject? gameObject, bool fromChildren = false)
    {
        if (gameObject == null)
            return null;

        // 1️⃣ check the object itself
        if (HasObject.TryGetValue(gameObject, out var mapEditorObject))
            return mapEditorObject;

        if (!fromChildren) return null;
        
        // 2️⃣ check children recursively
        var transform = gameObject.transform;

        for (int i = 0; i < transform.childCount; i++)
        {
            var result = GetFromChildren(transform.GetChild(i));
            if (result != null)
                return result;
        }

        return null;
    }

    private static TSelf? GetFromChildren(Transform parent)
    {
        var go = parent.gameObject;

        if (HasObject.TryGetValue(go, out var obj))
            return obj;

        for (int i = 0; i < parent.childCount; i++)
        {
            var result = GetFromChildren(parent.GetChild(i));
            if (result != null)
                return result;
        }

        return null;
    }
    
    private void Awake()
    {
        HasObject[gameObject] = (TSelf)this;
    }
    
    public void Destroy()
    {
        HasObject.Remove(gameObject);
    }
}