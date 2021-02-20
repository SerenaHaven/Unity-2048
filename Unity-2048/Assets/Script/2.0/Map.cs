using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    private RectTransform _rectTransform;
    private GameObject _cellPrefab;
    private GameObject[,] _cells;
    private GridLayoutGroup _gridLayoutGroup;

    public Transform this[int i, int j]
    {
        get
        {
            if (_cells == null || i < 0 || i >= Config.MaxResolution
            || j < 0 || j >= Config.MaxResolution)
            {
                return null;
            }
            return _cells[i, j].transform;
        }
    }

    private int _resolution = 0;

    public float cellSize { get; private set; } = 0;

    void Awake()
    {
        _rectTransform = transform as RectTransform;
        _cellPrefab = _rectTransform.Find("Cell").gameObject;
        _cellPrefab.SetActive(false);

        _gridLayoutGroup = _rectTransform.GetComponent<GridLayoutGroup>();

        _cells = new GameObject[Config.MaxResolution, Config.MaxResolution];
        for (int i = 0; i < Config.MaxResolution; i++)
        {
            for (int j = 0; j < Config.MaxResolution; j++)
            {
                var cell = Instantiate(_cellPrefab, _rectTransform);
                cell.SetActive(false);
                _cells[i, j] = cell;
            }
        }
    }

    public void Initialize(int resolution)
    {
        resolution = Mathf.Clamp(resolution, Config.MinResolution, Config.MaxResolution);
        if (_resolution == resolution) { return; }
        _resolution = resolution;

        cellSize = (_rectTransform.rect.width - (resolution + 1) * Config.CellSpacing) / resolution;
        _gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
        _gridLayoutGroup.spacing = Vector2.one * Config.CellSpacing;
        _gridLayoutGroup.padding = new RectOffset(Config.CellSpacing, Config.CellSpacing, Config.CellSpacing, Config.CellSpacing);
        _gridLayoutGroup.constraintCount = resolution;

        for (int i = 0; i < Config.MaxResolution; i++)
        {
            for (int j = 0; j < Config.MaxResolution; j++)
            {
                _cells[i, j].gameObject.SetActive(i < resolution && j < resolution);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }
}