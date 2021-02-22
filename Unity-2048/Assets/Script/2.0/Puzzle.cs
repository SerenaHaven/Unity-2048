using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Puzzle
{
    private readonly List<int> _empty = new List<int>();
    private readonly List<BlockData> _blocks = new List<BlockData>();
    private readonly BlockData[,] _blockMap = new BlockData[Config.MaxResolution, Config.MaxResolution];
    private readonly ObjectPool<BlockData> _pool = new ObjectPool<BlockData>();

    private bool _waitingForRemap = false;

    public int resolution { get; private set; } = 4;
    public event Action<int, int, int, int> onGenerate;
    public event Action onMove;

    public BlockData this[int i, int j]
    {
        get
        {
            if (_blockMap == null || i < 0 || i >= Config.MaxResolution
            || j < 0 || j >= Config.MaxResolution) { return null; }
            return _blockMap[i, j];
        }
    }

    public int GetPoolCount() { return _pool.count; }
    public int GetPoolActiveCount() { return _pool.activeCount; }
    public int GetPoolInactiveCount() { return _pool.inactiveCount; }

    public void Initialize(int resolution)
    {
        Clear();
        this.resolution = Mathf.Clamp(resolution, Config.MinResolution, Config.MaxResolution);
        Generate();
    }

    public int Generate()
    {
        _empty.Clear();

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                var block = _blockMap[i, j];
                if (block == null) { _empty.Add(i * resolution + j); }
            }
        }
        var count = _empty.Count;
        if (count > 0)
        {
            var index = _empty[Random.Range(0, count)];
            var value = Config.BlockValues[Random.Range(0, Config.BlockValues.Length)];
            int row = index / resolution;
            int column = index % resolution;
            var blockData = Spawn(row, column);
            blockData.row = blockData.nextRow = row;
            blockData.column = blockData.nextColumn = column;
            blockData.value = blockData.nextValue = value;
            if (onGenerate != null) { onGenerate.Invoke(row, column, value, count - 1); }
            return count - 1;
        }
        else { return 0; }
    }

    public void Left()
    {
        if (_waitingForRemap == true) { return; }
        var changed = false;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 1; j < resolution; j++)
            {
                var block = _blockMap[i, j];
                if (block == null) { continue; }
                block.nextColumn = 0;
                for (int k = j - 1; k >= 0; k--)
                {
                    var targetBlock = _blockMap[i, k];
                    if (targetBlock == null) { continue; }
                    int result;
                    if (targetBlock.merged == false && Merge(block.value, targetBlock.value, out result) == true)
                    {
                        Despawn(i, k);
                        block.nextColumn = targetBlock.nextColumn;
                        block.nextValue = result;
                    }
                    else
                    {
                        block.nextColumn = targetBlock.nextColumn + 1;
                        break;
                    }
                }
                if (block.moved == true || block.merged == true) { changed = true; }
            }
        }
        if (changed == true && onMove != null)
        {
            _waitingForRemap = true;
            onMove.Invoke();
        }
    }

    public void Right()
    {
        if (_waitingForRemap == true) { return; }
        var changed = false;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = resolution - 2; j >= 0; j--)
            {
                var block = _blockMap[i, j];
                if (block == null) { continue; }
                block.nextColumn = resolution - 1;
                for (int k = j + 1; k < resolution; k++)
                {
                    var targetBlock = _blockMap[i, k];
                    if (targetBlock == null) { continue; }
                    int result;
                    if (targetBlock.merged == false && Merge(block.value, targetBlock.value, out result) == true)
                    {
                        Despawn(i, k);
                        block.nextColumn = targetBlock.nextColumn;
                        block.nextValue = result;
                    }
                    else
                    {
                        block.nextColumn = targetBlock.nextColumn - 1;
                        break;
                    }
                }
                if (block.moved == true || block.merged == true) { changed = true; }
            }
        }
        if (changed == true && onMove != null)
        {
            _waitingForRemap = true;
            onMove.Invoke();
        }
    }

    public void Up()
    {
        if (_waitingForRemap == true) { return; }
        var changed = false;
        for (int j = 0; j < resolution; j++)
        {
            for (int i = 1; i < resolution; i++)
            {
                var block = _blockMap[i, j];
                if (block == null) { continue; }
                block.nextRow = 0;
                for (int k = i - 1; k >= 0; k--)
                {
                    var targetBlock = _blockMap[k, j];
                    if (targetBlock == null) { continue; }
                    int result;
                    if (targetBlock.merged == false && Merge(block.value, targetBlock.value, out result) == true)
                    {
                        Despawn(k, j);
                        block.nextRow = targetBlock.nextRow;
                        block.nextValue = result;
                    }
                    else
                    {
                        block.nextRow = targetBlock.nextRow + 1;
                        break;
                    }
                }
                if (block.moved == true || block.merged == true) { changed = true; }
            }
        }
        if (changed == true && onMove != null)
        {
            _waitingForRemap = true;
            onMove.Invoke();
        }
    }

    public void Down()
    {
        if (_waitingForRemap == true) { return; }
        var changed = false;
        for (int j = 0; j < resolution; j++)
        {
            for (int i = resolution - 2; i >= 0; i--)
            {
                var block = _blockMap[i, j];
                if (block == null) { continue; }
                block.nextRow = resolution - 1;
                for (int k = i + 1; k < resolution; k++)
                {
                    var targetBlock = _blockMap[k, j];
                    if (targetBlock == null) { continue; }
                    int result;
                    if (targetBlock.merged == false && Merge(block.value, targetBlock.value, out result) == true)
                    {
                        Despawn(k, j);
                        block.nextRow = targetBlock.nextRow;
                        block.nextValue = result;
                    }
                    else
                    {
                        block.nextRow = targetBlock.nextRow - 1;
                        break;
                    }
                }
                if (block.moved == true || block.merged == true) { changed = true; }
            }
        }
        if (changed == true && onMove != null)
        {
            _waitingForRemap = true;
            onMove.Invoke();
        }
    }

    public void Remap()
    {
        _blocks.Clear();
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                var block = _blockMap[i, j];
                if (block == null) { continue; }
                _blocks.Add(block);
                _blockMap[i, j] = null;
            }
        }
        if (_blocks.Count > 0)
        {
            foreach (var item in _blocks) { _blockMap[item.nextRow, item.nextColumn] = item; }
        }
        _waitingForRemap = false;
    }

    private BlockData Spawn(int row, int column)
    {
        if (row < 0 || row >= Config.MaxResolution
        || column < 0 || column > Config.MaxResolution) { return null; }
        return _blockMap[row, column] = _pool.Spawn();
    }

    private void Despawn(int row, int column)
    {
        if (row < 0 || row >= Config.MaxResolution
        || column < 0 || column > Config.MaxResolution) { return; }
        var block = _blockMap[row, column];
        if (block != null) { _pool.Despawn(block); _blockMap[row, column] = null; }
    }

    private void Clear()
    {
        _empty.Clear();
        _blocks.Clear();
        _pool.DespawnAll();
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++) { _blockMap[i, j] = null; }
        }
    }

    private bool Merge(int a, int b, out int result)
    {
        if (a == b) { result = a + b; return true; }
        else { result = 0; return false; }
    }
}