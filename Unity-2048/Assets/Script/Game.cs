using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    private GridLayoutGroup _gridLayoutGroup;

    private GameObject _cellPrefab;
    private RectTransform _grid;
    private GameObject[,] _cells;

    private GameObject _blockPrefab;
    private Transform _blocksRoot;
    private Block[,] _blocks;

    private Puzzle _puzzle;

    private int _score;
    private Text _textScore;

    private const float CellSpacing = 5;

    void Awake()
    {
        _textScore = transform.Find("TextScore").GetComponent<Text>();

        _grid = transform.Find("Grid") as RectTransform;
        _gridLayoutGroup = _grid.GetComponent<GridLayoutGroup>();
        _cellPrefab = _grid.Find("Cell").gameObject;
        _cellPrefab.SetActive(false);

        _blocksRoot = transform.Find("Blocks");
        _blockPrefab = transform.Find("Blocks/Block").gameObject;
        _blockPrefab.SetActive(false);

        _puzzle = new Puzzle();
        _puzzle.onMerge += OnMerge;
        _puzzle.onGenerate += OnGenerate;

        _cells = new GameObject[Puzzle.MaxResolution, Puzzle.MaxResolution];
        _blocks = new Block[Puzzle.MaxResolution, Puzzle.MaxResolution];
        for (int i = 0; i < Puzzle.MaxResolution; i++)
        {
            for (int j = 0; j < Puzzle.MaxResolution; j++)
            {
                var cell = Instantiate(_cellPrefab, _grid);
                cell.SetActive(true);
                _cells[i, j] = cell;

                var go = Instantiate(_blockPrefab, _blocksRoot);
                var block = go.AddComponent<Block>();
                _blocks[i, j] = block;

                block.transform.position = cell.transform.position;
            }
        }

        ResetGame(4);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) == true)
        {
            _puzzle.Left();
            UpdateView();
        }

        if (Input.GetKeyDown(KeyCode.D) == true)
        {
            _puzzle.Right();
            UpdateView();
        }

        if (Input.GetKeyDown(KeyCode.W) == true)
        {
            _puzzle.Up();
            UpdateView();
        }

        if (Input.GetKeyDown(KeyCode.S) == true)
        {
            _puzzle.Down();
            UpdateView();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) == true)
        {
            ResetGame(4);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) == true)
        {
            ResetGame(5);

        }
        if (Input.GetKeyDown(KeyCode.Alpha3) == true)
        {
            ResetGame(6);
        }
    }

    void OnMerge(int value)
    {
        _score += value;
        _textScore.text = string.Format("Score : " + _score);
    }

    void OnGenerate(int row, int column, int value)
    {
        var block = _blocks[row, column];
        block.Set(value);
        block.AnimateAppear();
    }

    private void ResetGame(int resolution)
    {
        _puzzle.Initialize(resolution);

        var width = _grid.rect.width;
        var cellSize = (width - (resolution + 1) * CellSpacing) / resolution;
        _gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_grid);

        for (int i = 0; i < Puzzle.MaxResolution; i++)
        {
            for (int j = 0; j < Puzzle.MaxResolution; j++)
            {
                var active = i < _puzzle.resolution && j < _puzzle.resolution;
                var cell = _cells[i, j];
                cell.gameObject.SetActive(active);

                var block = _blocks[i, j];
                block.SetSize(cellSize);
                block.gameObject.SetActive(false);

                block.transform.position = cell.transform.position;
            }
        }

        UpdateView();
    }

    private void UpdateView()
    {
        for (int i = 0; i < _puzzle.resolution; i++)
        {
            for (int j = 0; j < _puzzle.resolution; j++)
            {
                var block = _blocks[i, j];
                var value = _puzzle.GetValue(i, j);
                block.Set(_puzzle.GetValue(i, j));
                block.gameObject.SetActive(value != 0);
                block.transform.position = _cells[i, j].transform.position;
            }
        }
    }
}