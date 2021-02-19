using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoveData
{
    public int fromX;
    public int fromY;
    public int fromValue;
    public int toX;
    public int toY;
    public int toValue;
}

public class Puzzle
{
    public const int MaxResolution = 6;
    public const int MinResolution = 4;
    public int resolution { get; private set; } = 4;

    public event Action<int> onMerge;
    public event Action<int, int, int> onGenerate;
    public event Action<int> onMove;

    private int[,] _map = new int[MaxResolution, MaxResolution];
    private int[,] _mapPrevious = new int[MaxResolution, MaxResolution];

    private readonly List<int> _empty = new List<int>();
    private readonly List<int> _toMove = new List<int>();
    private readonly int[] Values = { 2 };

    public void Initialize(int resolution)
    {
        this.resolution = Mathf.Clamp(resolution, MinResolution, MaxResolution);
        for (int i = 0; i < this.resolution; i++)
        {
            for (int j = 0; j < this.resolution; j++)
            {
                _map[i, j] = 0;
                _mapPrevious[i, j] = 0;
            }
        }

        _empty.Clear();
        _toMove.Clear();

        int x = Random.Range(0, this.resolution - 1);
        int y = Random.Range(0, this.resolution - 1);
        _map[x, y] = 2;
    }

    public int GetValue(int i, int j)
    {
        if (i >= 0 && i < resolution && j >= 0 && j < resolution)
        {
            return _map[i, j];
        }
        else { return 0; }
    }

    public void Left()
    {
        Clone();
        MoveLeft();
        MergeLeft();
        MoveLeft();
        TryGenerate();
    }

    public void Right()
    {
        Clone();
        MoveRight();
        MergeRight();
        MoveRight();
        TryGenerate();
    }

    public void Up()
    {
        Clone();
        MoveUp();
        MergeUp();
        MoveUp();
        TryGenerate();
    }

    public void Down()
    {
        Clone();
        MoveDown();
        MergeDown();
        MoveDown();
        TryGenerate();
    }

    private void Clone()
    {
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                _mapPrevious[i, j] = _map[i, j];
            }
        }
    }

    private bool CheckMoved()
    {
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                if (_mapPrevious[i, j] != _map[i, j])
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void TryGenerate()
    {
        if (CheckMoved() == false) { return; }

        _empty.Clear();
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                var value = _map[i, j];
                if (value == 0) { _empty.Add(i * resolution + j); }
            }
        }
        if (_empty.Count > 0)
        {
            var index = _empty[Random.Range(0, _empty.Count)];
            var value = Values[Random.Range(0, Values.Length)];
            int row = index / resolution;
            int column = index % resolution;
            _map[row, column] = value;
            if (onGenerate != null) { onGenerate.Invoke(row, column, value); }
        }
        else
        {
            if (onMove != null) { onMove.Invoke(_empty.Count); }
        }
    }

    private bool Merge(int a, int b, out int result)
    {
        if (a == b)
        {
            result = a + b;
            return true;
        }
        else
        {
            result = 0;
            return false;
        }
    }

    private void MoveLeft()
    {
        for (int i = 0; i < resolution; i++)
        {
            _toMove.Clear();
            for (int j = 0; j < resolution; j++)
            {
                var value = _map[i, j];
                if (value != 0) { _toMove.Add(value); }
            }

            var moveCount = _toMove.Count;
            if (moveCount > 0 && moveCount < resolution)
            {
                for (int j = 0; j < resolution; j++)
                {
                    _map[i, j] = (j < moveCount ? _toMove[j] : 0);
                }
            }
        }
    }

    private void MergeLeft()
    {
        int a, b, result = 0;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution - 1; j++)
            {
                a = _map[i, j];
                if (a == 0) { continue; }
                b = _map[i, j + 1];
                if (Merge(a, b, out result) == true)
                {
                    if (onMerge != null) { onMerge.Invoke(result); }
                    _map[i, j] = result;
                    _map[i, j + 1] = 0;
                }
            }
        }
    }

    private void MoveRight()
    {
        for (int i = 0; i < resolution; i++)
        {
            _toMove.Clear();
            for (int j = resolution - 1; j >= 0; j--)
            {
                var value = _map[i, j];
                if (value != 0) { _toMove.Add(value); }
            }

            var moveCount = _toMove.Count;
            if (moveCount > 0 && moveCount < resolution)
            {
                for (int j = resolution - 1; j >= 0; j--)
                {
                    var index = resolution - j - 1;
                    _map[i, j] = (index < moveCount ? _toMove[index] : 0);
                }
            }
        }
    }

    private void MergeRight()
    {
        int a, b, result = 0;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = resolution - 1; j > 0; j--)
            {
                a = _map[i, j];
                if (a == 0) { continue; }
                b = _map[i, j - 1];
                if (Merge(a, b, out result) == true)
                {
                    if (onMerge != null) { onMerge.Invoke(result); }
                    _map[i, j] = result;
                    _map[i, j - 1] = 0;
                }
            }
        }
    }

    private void MoveUp()
    {
        for (int j = 0; j < resolution; j++)
        {
            _toMove.Clear();
            for (int i = 0; i < resolution; i++)
            {
                var value = _map[i, j];
                if (value != 0) { _toMove.Add(value); }
            }

            var moveCount = _toMove.Count;
            if (moveCount > 0 && moveCount < resolution)
            {
                for (int i = 0; i < resolution; i++)
                {
                    _map[i, j] = (i < moveCount ? _toMove[i] : 0);
                }
            }
        }
    }

    private void MergeUp()
    {
        int a, b, result = 0;
        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution - 1; i++)
            {
                a = _map[i, j];
                if (a == 0) { continue; }
                b = _map[i + 1, j];
                if (Merge(a, b, out result) == true)
                {
                    if (onMerge != null) { onMerge.Invoke(result); }
                    _map[i, j] = result;
                    _map[i + 1, j] = 0;
                }
            }
        }
    }

    private void MoveDown()
    {
        for (int j = 0; j < resolution; j++)
        {
            _toMove.Clear();
            for (int i = resolution - 1; i >= 0; i--)
            {
                var value = _map[i, j];
                if (value != 0) { _toMove.Add(value); }
            }

            var moveCount = _toMove.Count;
            if (moveCount > 0 && moveCount < resolution)
            {
                for (int i = resolution - 1; i >= 0; i--)
                {
                    var index = resolution - i - 1;
                    _map[i, j] = (index < moveCount ? _toMove[index] : 0);
                }
            }
        }
    }

    private void MergeDown()
    {
        int a, b, result = 0;
        for (int j = 0; j < resolution; j++)
        {
            for (int i = resolution - 1; i > 0; i--)
            {
                a = _map[i, j];
                if (a == 0) { continue; }
                b = _map[i - 1, j];
                if (Merge(a, b, out result) == true)
                {
                    if (onMerge != null) { onMerge.Invoke(result); }
                    _map[i, j] = result;
                    _map[i - 1, j] = 0;
                }
            }
        }
    }
}