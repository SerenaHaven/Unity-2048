using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : IDisposable
{
    private GameObject _prefab;
    private string _name;
    private Transform _root;
    private bool _disposed;
    private List<GameObject> _activeCollection = new List<GameObject>();
    private List<GameObject> _inactiveCollection = new List<GameObject>();
    private static readonly object _locker = new object();

    public GameObjectPool(GameObject prefab, Transform parent = null)
    {
        if (prefab == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Prefab can not be null when creating a gameobject pool.");
#endif
            _disposed = true;
            return;
        }
        _prefab = prefab;
        _name = prefab.name;

        _root = new GameObject(_name + "Pool").transform;
        _root.SetParent(parent);
        _disposed = false;
    }

    private bool CheckDisposed()
    {
        if (_disposed == true)
        {
#if UNITY_EDITOR
            Debug.LogErrorFormat("Cannot do any operation on a disposed game object pool.");
#endif
            return true;
        }
        else { return false; }
    }

    public GameObject Spawn(Transform parent = null)
    {
        if (CheckDisposed() == true) { return null; }

        lock (_locker)
        {
            GameObject result = null;
            if (_inactiveCollection.Count <= 0)
            {
                result = GameObject.Instantiate(_prefab, parent);
            }
            else
            {
                result = _inactiveCollection[0];
                _inactiveCollection.RemoveAt(0);
            }
            if (_activeCollection.Contains(result) == false)
            {
                _activeCollection.Add(result);
            }
            result.transform.SetParent(parent);
            return result;
        }
    }

    public void Despawn(GameObject target)
    {
        if (CheckDisposed() == true) { return; }

        lock (_locker)
        {
            if (_activeCollection.Contains(target) == true)
            {
                _activeCollection.Remove(target);
            }

            if (_inactiveCollection.Contains(target) == false)
            {
                _inactiveCollection.Add(target);
            }

            target.SetActive(false);
            target.transform.SetParent(_root);
        }
    }

    public void DespawnAll()
    {
        if (CheckDisposed() == true) { return; }

        lock (_locker)
        {
            for (int i = _activeCollection.Count - 1; i >= 0; i--)
            {
                Despawn(_activeCollection[i]);
            }
        }
    }

    public void Clear()
    {
        if (CheckDisposed() == true)
        {
            return;
        }
        lock (_locker)
        {
            if (_activeCollection != null && _activeCollection.Count > 0)
            {
                foreach (var item in _activeCollection)
                {
                    GameObject.Destroy(item);
                }
                _activeCollection.Clear();
            }
            if (_inactiveCollection != null && _inactiveCollection.Count > 0)
            {
                foreach (var item in _inactiveCollection)
                {
                    GameObject.Destroy(item);
                }
                _inactiveCollection.Clear();
            }
        }
    }

    public void Dispose()
    {
        if (CheckDisposed() == true) { return; }
        lock (_locker)
        {
            Clear();
            _prefab = null;
            _name = null;
            _inactiveCollection = null;
            GameObject.Destroy(_root.gameObject);
            _root = null;
            _disposed = true;
        }
    }
}