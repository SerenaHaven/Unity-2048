using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    private readonly Puzzle _puzzle = new Puzzle();
    private Map _map;

    private GameObject _blockPrefab;
    private Transform _blockRoot;
    private Block[,] _blockMap;
    private GameObjectPool _pool;

    private Text _textScore;
    private int _score;

    private bool _moving = false;
    private float _lerp = 0.0f;

    void Awake()
    {
        _textScore = transform.Find("Score/TextScore").GetComponent<Text>();

        _map = transform.Find("Map").GetComponent<Map>();

        _blockRoot = transform.Find("Blocks");
        _blockPrefab = transform.Find("Blocks/Block").gameObject;
        _blockPrefab.SetActive(false);
        _pool = new GameObjectPool(_blockPrefab, _blockRoot);
        _blockMap = new Block[Config.MaxResolution, Config.MaxResolution];

        _puzzle.onMove += OnMove;
        _puzzle.onGenerate += OnGenerate;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        ResetGame(4);
    }

    private void OnGenerate(int row, int column, int value, int remain)
    {
        var block = _blockMap[row, column];
        if (block == null)
        {
            block = Spawn(row, column);
            block.gameObject.SetActive(true);
        }
        block.transform.position = _map[row, column].transform.position; ;
        block.Set(value);
        block.AnimateAppear();
    }

    private void OnMove()
    {
        _moving = true;
        _lerp = 0.0f;
    }

    private void ResetGame(int resolution)
    {
        Clear();
        resolution = Mathf.Clamp(resolution, Config.MinResolution, Config.MaxResolution);
        _map.Initialize(resolution);
        _puzzle.Initialize(resolution);
        _score = 0;
    }


    private Block Spawn(int row, int column)
    {
        if (row < 0 || row >= Config.MaxResolution
        || column < 0 || column > Config.MaxResolution) { return null; }

        var go = _pool.Spawn(_blockRoot);
        var block = go.GetComponent<Block>();
        if (block == null) { block = go.AddComponent<Block>(); }
        block.Reset();
        block.SetSize(_map.cellSize);
        _blockMap[row, column] = block;
        return block;
    }

    private void Despawn(int row, int column)
    {
        if (row < 0 || row >= Config.MaxResolution
        || column < 0 || column > Config.MaxResolution) { return; }

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

    private void Remap()
    {
        _puzzle.Remap();
        for (int i = 0; i < _puzzle.resolution; i++)
        {
            for (int j = 0; j < _puzzle.resolution; j++)
            {
                var blockData = _puzzle[i, j];
                var block = _blockMap[i, j];
                if (blockData == null) { Despawn(i, j); continue; }
                if (block == null) { (block = Spawn(i, j)).gameObject.SetActive(true); }
                block.Set(blockData.value);
            }
        }
    }

    void Update()
    {
        if (_moving == true)
        {
            _lerp += Time.deltaTime * Config.BlockMoveSpeed;

            if (_lerp < 1.0f)
            {
                for (int i = 0; i < _puzzle.resolution; i++)
                {
                    for (int j = 0; j < _puzzle.resolution; j++)
                    {
                        var blockData = _puzzle[i, j];
                        if (blockData == null) { Despawn(i, j); continue; }

                        var block = _blockMap[i, j];
                        if (block == null) { block = Spawn(i, j); }

                        var from = _map[i, j].transform.position;
                        var to = _map[blockData.nextRow, blockData.nextColumn].transform.position;
                        block.transform.position = Vector3.Lerp(from, to, _lerp);
                    }
                }
            }
            else
            {
                Remap();
                for (int i = 0; i < _puzzle.resolution; i++)
                {
                    for (int j = 0; j < _puzzle.resolution; j++)
                    {
                        var block = _blockMap[i, j];
                        if (block == null) { continue; }
                        var blockData = _puzzle[i, j];
                        if (blockData == null) { continue; }
                        block.Set(blockData.nextValue);
                        if (blockData.merged == true) { block.AnimateAppear(); }
                        blockData.value = blockData.nextValue;
                        blockData.row = blockData.nextRow;
                        blockData.column = blockData.nextColumn;
                        block.transform.position = _map[i, j].transform.position;
                    }
                }

                if (_lerp > 1.5f)
                {
                    _puzzle.Generate();
                    _moving = false;
                }
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Space) == true) { _puzzle.Generate(); }

            if (Input.GetKey(KeyCode.A) == true) { _puzzle.Left(); }

            if (Input.GetKey(KeyCode.D) == true) { _puzzle.Right(); }

            if (Input.GetKey(KeyCode.W) == true) { _puzzle.Up(); }

            if (Input.GetKey(KeyCode.S) == true) { _puzzle.Down(); }

            if (Input.GetKeyDown(KeyCode.Alpha1) == true) { ResetGame(4); }

            if (Input.GetKeyDown(KeyCode.Alpha2) == true) { ResetGame(5); }

            if (Input.GetKeyDown(KeyCode.Alpha3) == true) { ResetGame(6); }
        }
    }
}
