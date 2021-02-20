using System.Collections.Generic;

public class ObjectPool<T> where T : new()
{
    private readonly object _locker = new object();

    private List<T> _actives = new List<T>();
    private List<T> _inactives = new List<T>();

    public int count { get { return _actives.Count + _inactives.Count; } }
    public int activeCount { get { return _actives.Count; } }
    public int inactiveCount { get { return _inactives.Count; } }

    public T Spawn()
    {
        lock (_locker)
        {
            T result;
            if (_inactives.Count <= 0) { result = new T(); }
            else
            {
                result = _inactives[0];
                _inactives.RemoveAt(0);
            }

            if (_actives.Contains(result) == false) { _actives.Add(result); }
            return result;
        }
    }

    public void Despawn(T item)
    {
        lock (_locker)
        {
            if (_actives.Contains(item) == true) { _actives.Remove(item); }

            if (_inactives.Contains(item) == false) { _inactives.Add(item); }
        }
    }

    public void DespawnAll()
    {
        lock (_locker)
        {
            for (int i = _actives.Count - 1; i >= 0; i--) { Despawn(_actives[i]); }
        }
    }
}