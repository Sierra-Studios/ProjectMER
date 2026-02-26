#nullable enable
namespace ProjectMER.Features.Utility;

public class WatchableList<T> : List<T>
{
    /// <summary>
    /// Action that is invoked when an element is added
    /// </summary>
    public Action<T>? OnAdd { get; set; }
    
    /// <summary>
    /// Action that is invoked when an element is removed
    /// </summary>
    public Action<T>? OnRemove { get; set; }
    
    /// <summary>
    /// Action that is invoked when list is cleared
    /// </summary>
    public Action? OnClear { get; set; }

    /// <summary>
    /// Default constructor, do not remove, used so Activator.CreateInstance() may work on this class.
    /// </summary>
    public WatchableList()
    {
        
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="onAdd"></param>
    /// <param name="onRemove"></param>
    /// <param name="onClear"></param>
    
    public WatchableList(Action<T> onAdd, Action<T> onRemove, Action onClear)
    {
        OnAdd = onAdd;
        OnRemove = onRemove;
        OnClear = onClear;
    }

    /// <summary>
    /// Adds an object to the end of the List{T}.
    /// OnAdd event invoked before it's added.
    /// </summary>
    /// <param name="item">The item added at the end of list</param>
    public new void Add(T item)
    {
        OnAdd?.Invoke(item);
        base.Add(item);
    }
    
    public new bool Remove(T item)
    {
        OnRemove?.Invoke(item);
        return base.Remove(item);
    }

    public new void Clear()
    {
        OnClear?.Invoke();
        foreach (var i in this)
        {
            OnRemove?.Invoke(i);
        }
        base.Clear();
    }

    public new void RemoveAll(Predicate<T> match)
    {
        foreach (var item in this)
        {
            if(match(item)) OnRemove?.Invoke(item);
        }
        base.RemoveAll(match);
    }
}