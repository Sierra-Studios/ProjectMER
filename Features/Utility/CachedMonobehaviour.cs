using UnityEngine;

namespace ProjectMER.Features.Utility;

public class CachedMonobehaviour<TSelf> : MonoBehaviour where TSelf : CachedMonobehaviour<TSelf>
{
    public static Dictionary<GameObject, TSelf> HasObject { get; } = [];
	
    public static TSelf? Get(GameObject? gameObject)
    {
        if (gameObject == null) return null;
        
        if (HasObject.TryGetValue(gameObject, out var mapEditorObject))
        {
            return mapEditorObject;
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