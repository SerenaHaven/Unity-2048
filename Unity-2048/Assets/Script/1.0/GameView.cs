using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{
    private GridLayoutGroup _gridLayoutGroup;

    private GameObject _cellPrefab;
    private GameObject[,] _cells;
    private RectTransform _cellRoot;

    private GameObject _blockPrefab;
    private BlockView[,] _blockMap;
    private Transform _blockRoot;

    private GameObjectPool _pool;

    private int _score;
    private int score
    {
        get { return _score; }
        set
        {
            _textScore.text = value.ToString();
            _score = value;
            if (_score > _best)
            {
                _best = _score;
                PlayerPrefs.SetInt("Best", value);
                _textBest.text = value.ToString();
            }
        }
    }
    private Text _textScore;

    private int _best;
    private Text _textBest;

    private float _cellSize;
    private bool _moving = false;
    private float _lerp = 0.0f;

    public int resolution { get; private set; } = 4;

    private readonly List<int> _empty = new List<int>();


    void Awake()
    {
        _textScore = transform.Find("Score/TextScore").GetComponent<Text>();
        _textBest = transform.Find("Best/TextBest").GetComponent<Text>();

        _cellRoot = transform.Find("Grid") as RectTransform;
        _gridLayoutGroup = _cellRoot.GetComponent<GridLayoutGroup>();
        _cellPrefab = _cellRoot.Find("Cell").gameObject;
        _cellPrefab.SetActive(false);

        _blockRoot = transform.Find("Blocks");
        _blockPrefab = transform.Find("Blocks/Block").gameObject;
        _blockPrefab.SetActive(false);

        _pool = new GameObjectPool(_blockPrefab, _blockRoot);

        _cells = new GameObject[Config.MaxResolution, Config.MaxResolution];
        _blockMap = new BlockView[Config.MaxResolution, Config.MaxResolution];
        for (int i = 0; i < Config.MaxResolution; i++)
        {
            for (int j = 0; j < Config.MaxResolution; j++)
            {
                var cell = Instantiate(_cellPrefab, _cellRoot);
                cell.SetActive(true);
                _cells[i, j] = cell;
            }
        }

        _score = 0;
        _best = PlayerPrefs.GetInt("Best", 0);
        _textBest.text = _best.ToString();

        ResetGame(resolution);
    }

    private void Generate()
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
        if (_empty.Count > 0)
        {
            var index = _empty[Random.Range(0, _empty.Count)];
            var value = Config.BlockValues[Random.Range(0, Config.BlockValues.Length)];
            int row = index / resolution;
            int column = index % resolution;

            var block = SpawnBlock(row, column);
            block.gameObject.SetActive(true);
            _blockMap[row, column] = block;
            block.transform.position = _cells[row, column].transform.position;
            block.Set(row, column, value);
            block.SetNext(row, column, value);
            block.SetSize(_cellSize);
            block.AnimateAppear();
        }
    }

    private BlockView SpawnBlock(int row, int column)
    {
        if (row < 0 || row >= Config.MaxResolution || column < 0 || column > Config.MaxResolution) { return null; }
        var go = _pool.Spawn(_blockRoot);
        var block = go.GetComponent<BlockView>();
        if (block == null) { block = go.AddComponent<BlockView>(); }
        _blockMap[row, column] = block;
        return block;
    }

    private void DespawnBlock(int row, int column)
    {
        if (row < 0 || row >= Config.MaxResolution || column < 0 || column > Config.MaxResolution) { return; }
        var block = _blockMap[row, column];
        if (block != null)
        {
            _pool.Despawn(block.gameObject);
            _blockMap[row, column] = null;
        }
    }

    private void Clear()
    {
        for (int i = 0; i < Config.MaxResolution; i++)
        {
            for (int j = 0; j < Config.MaxResolution; j++) { _blockMap[i, j] = null; }
        }
        _pool.DespawnAll();
    }

    private void ResetGame(int resolution)
    {
        Clear();
        this.resolution = Mathf.Clamp(resolution, Config.MinResolution, Config.MaxResolution);

        var width = _cellRoot.rect.width;
        _cellSize = (width - (resolution + 1) * Config.CellSpacing) / resolution;
        _gridLayoutGroup.cellSize = new Vector2(_cellSize, _cellSize);
        _gridLayoutGroup.spacing = Vector2.one * Config.CellSpacing;
        _gridLayoutGroup.padding = new RectOffset(Config.CellSpacing, Config.CellSpacing, Config.CellSpacing, Config.CellSpacing);
        _gridLayoutGroup.constraintCount = resolution;

        for (int i = 0; i < Config.MaxResolution; i++)
        {
            for (int j = 0; j < Config.MaxResolution; j++)
            {
                var active = i < resolution && j < resolution;
                var cell = _cells[i, j];
                cell.gameObject.SetActive(active);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_cellRoot);
        score = 0;
        Generate();
    }

    void Update()
    {
        if (_moving == true)
        {
            _lerp += Time.deltaTime * Config.BlockMoveSpeed;

            if (_lerp < 1.0f)
            {
                for (int i = 0; i < resolution; i++)
                {
                    for (int j = 0; j < resolution; j++)
                    {
                        var block = _blockMap[i, j];
                        if (block == null) { continue; }
                        var from = _cells[block.row, block.column].transform.position;
                        var to = _cells[block.nextRow, block.nextColumn].transform.position;
                        block.transform.position = Vector3.Lerp(from, to, _lerp);
                    }
                }
            }
            else
            {
                for (int i = 0; i < resolution; i++)
                {
                    for (int j = 0; j < resolution; j++)
                    {
                        var block = _blockMap[i, j];
                        if (block == null) { continue; }
                        if (block.merged == true)
                        {
                            score += block.nextValue;
                            block.AnimateAppear();
                        }
                        block.Apply();
                        block.transform.position = _cells[i, j].transform.position;
                    }
                }
                if (_lerp > 1.5f)
                {
                    Generate();
                    _moving = false;
                }
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Space) == true) { Generate(); }

            if (Input.GetKey(KeyCode.A) == true) { Left(); }

            if (Input.GetKey(KeyCode.D) == true) { Right(); }

            if (Input.GetKey(KeyCode.W) == true) { Up(); }

            if (Input.GetKey(KeyCode.S) == true) { Down(); }

            if (Input.GetKeyDown(KeyCode.Alpha1) == true) { ResetGame(4); }

            if (Input.GetKeyDown(KeyCode.Alpha2) == true) { ResetGame(5); }

            if (Input.GetKeyDown(KeyCode.Alpha3) == true) { ResetGame(6); }
        }
    }

    private bool Merge(int a, int b, out int result)
    {
        if (a == b) { result = a + b; return true; }
        else { result = 0; return false; }
    }

    private bool Remap()
    {
        var changed = false;
        var datas = new List<BlockView>();

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                var block = _blockMap[i, j];
                if (block == null) { continue; }
                datas.Add(block);

                if (block.moved == true) { changed = true; }
                _blockMap[i, j] = null;
            }
        }
        if (datas.Count > 0)
        {
            foreach (var item in datas) { _blockMap[item.nextRow, item.nextColumn] = item; }
        }

        return changed;
    }

    private void Move()
    {
        _moving = true;
        _lerp = 0.0f;
    }

    private void Left()
    {
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
                        DespawnBlock(i, k);
                        block.nextColumn = targetBlock.nextColumn;
                        block.nextValue = result;
                    }
                    else
                    {
                        block.nextColumn = targetBlock.nextColumn + 1;
                        break;
                    }
                }
            }
        }
        if (Remap() == true) { Move(); };
    }

    private void Right()
    {
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
                        DespawnBlock(i, k);
                        block.nextColumn = targetBlock.nextColumn;
                        block.nextValue = result;
                    }
                    else
                    {
                        block.nextColumn = targetBlock.nextColumn - 1;
                        break;
                    }
                }
            }
        }
        if (Remap() == true) { Move(); };
    }

    private void Up()
    {
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
                        DespawnBlock(k, j);
                        block.nextRow = targetBlock.nextRow;
                        block.nextValue = result;
                    }
                    else
                    {
                        block.nextRow = targetBlock.nextRow + 1;
                        break;
                    }
                }
            }
        }
        if (Remap() == true) { Move(); };
    }

    private void Down()
    {
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
                        DespawnBlock(k, j);
                        block.nextRow = targetBlock.nextRow;
                        block.nextValue = result;
                    }
                    else
                    {
                        block.nextRow = targetBlock.nextRow - 1;
                        break;
                    }
                }
            }
        }
        if (Remap() == true) { Move(); };
    }
}
