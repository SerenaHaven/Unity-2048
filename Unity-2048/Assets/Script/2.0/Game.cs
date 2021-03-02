using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class Game : MonoBehaviour
{
    private readonly Puzzle _puzzle = new Puzzle();
    private Map _map;

    private GameObject _blockPrefab;
    private Transform _blockRoot;
    private Block[,] _blockMap;
    private GameObjectPool _pool;

    private Text _textScore;
    private Text _textBest;

    private Button _buttonRestart;
    private Button _buttonMenu;

    private Menu _menu;
    private Dialog _dialog;

    private int _best;
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

    private bool _moving = false;
    private float _lerp = 0.0f;

    private Vector2 _touchBeganPosition;
    private bool _slide = false;
    private RectTransform _rectTransform;

    void Awake()
    {
        _textScore = transform.Find("Score/TextScore").GetComponent<Text>();
        _textBest = transform.Find("Best/TextBest").GetComponent<Text>();
        _buttonRestart = transform.Find("ButtonRestart").GetComponent<Button>();
        _buttonRestart.onClick.AddListener(OnButtonRestart);
        _buttonMenu = transform.Find("ButtonMenu").GetComponent<Button>();
        _buttonMenu.onClick.AddListener(OnButtonMenu);

        _map = transform.Find("Map").gameObject.AddComponent<Map>();

        _menu = transform.Find("Menu").gameObject.AddComponent<Menu>();
        _menu.onButton4 = OnButton4;
        _menu.onButton5 = OnButton5;
        _menu.onButton6 = OnButton6;
        _menu.onButtonQuit = OnButtonQuit;
        _menu.Show(false);

        _dialog = transform.Find("Dialog").gameObject.AddComponent<Dialog>();
        _dialog.Hide();

        _blockRoot = transform.Find("Blocks");
        _blockPrefab = transform.Find("Blocks/Block").gameObject;
        _blockPrefab.SetActive(false);
        _pool = new GameObjectPool(_blockPrefab, _blockRoot);
        _blockMap = new Block[Config.MaxResolution, Config.MaxResolution];

        _puzzle.onMove += OnMove;
        _puzzle.onGenerate += OnGenerate;

        score = 0;
        _best = PlayerPrefs.GetInt("Best", 0);
        _textBest.text = _best.ToString();

        _rectTransform = transform as RectTransform;
    }

    private bool IsPlaying()
    {
        return _menu.gameObject.activeSelf == false && _dialog.gameObject.activeSelf == false;
    }

    private void OnButtonMenu() { _menu.Show(true); }

    private void OnButtonRestart()
    {
        _dialog.Show("Restart Game with current resolution?", ResetGame);
    }

    private void OnButton4()
    {
        if (IsPlaying() == true) { Initialize(4); }
        else { _dialog.Show("Restart Game with resolution 4?", () => { Initialize(4); }); }
    }

    private void OnButton5()
    {
        if (IsPlaying() == true) { Initialize(5); }
        else { _dialog.Show("Restart Game with resolution 5?", () => { Initialize(5); }); }
    }

    private void OnButton6()
    {
        if (IsPlaying() == true) { Initialize(6); }
        else { _dialog.Show("Restart Game with resolution 6?", () => { Initialize(6); }); }
    }

    private void OnButtonQuit()
    {
        _dialog.Show("Quit Game?", QuitGame);
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

    private void Initialize(int resolution)
    {
        _menu.Hide();
        _dialog.Hide();
        Clear();
        resolution = Mathf.Clamp(resolution, Config.MinResolution, Config.MaxResolution);
        _map.Initialize(resolution);
        _puzzle.Initialize(resolution);
        score = 0;
    }

    private void ResetGame()
    {
        Initialize(_puzzle.resolution);
    }

    private void GameOver()
    {
        _dialog.Show("Game Over. Restart?", ResetGame, () => { _menu.Show(false); });
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 point;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform, Input.mousePosition, null, out point);

            Debug.Log(point);

        }

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
                        if (blockData.merged == true)
                        {
                            score += blockData.nextValue;
                            block.AnimateAppear();
                        }
                        blockData.value = blockData.nextValue;
                        blockData.row = blockData.nextRow;
                        blockData.column = blockData.nextColumn;
                        block.transform.position = _map[i, j].transform.position;
                    }
                }

                if (_lerp > 1.5f)
                {
                    if (_puzzle.Generate() == 0)
                    {
                        GameOver();
                    }

                    _moving = false;
                }
            }
        }
        else
        {
            if (IsPlaying() == true)
            {
                if (Input.GetKey(KeyCode.Space) == true) { _puzzle.Generate(); }

                if (Input.GetKey(KeyCode.A) == true) { _puzzle.Left(); }

                if (Input.GetKey(KeyCode.D) == true) { _puzzle.Right(); }

                if (Input.GetKey(KeyCode.W) == true) { _puzzle.Up(); }

                if (Input.GetKey(KeyCode.S) == true) { _puzzle.Down(); }

                if (Input.GetKeyDown(KeyCode.Alpha1) == true) { Initialize(4); }

                if (Input.GetKeyDown(KeyCode.Alpha2) == true) { Initialize(5); }

                if (Input.GetKeyDown(KeyCode.Alpha3) == true) { Initialize(6); }

                if (Input.touchCount > 0)
                {
                    var touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began)
                    {
                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            _rectTransform, touch.position, null, out localPoint);
                        if (localPoint.y < _rectTransform.rect.height * 0.5f - 420.0f)
                        {
                            _touchBeganPosition = touch.position;
                            _slide = false;
                        }
                    }
                    else
                    {
                        if (_slide == false && touch.phase == TouchPhase.Moved)
                        {
                            var delta = touch.position - _touchBeganPosition;
                            var x = Mathf.Abs(delta.x);
                            var y = Mathf.Abs(delta.y);
                            if (Mathf.Max(x, y) >= Config.TouchThreshold)
                            {
                                if (x > y)
                                {
                                    if (Mathf.Sign(delta.x) > 0) { _puzzle.Right(); } else { _puzzle.Left(); }
                                }
                                else
                                {
                                    if (Mathf.Sign(delta.y) > 0) { _puzzle.Up(); } else { _puzzle.Down(); }
                                }
                                _slide = true;
                            }
                        }
                    }
                }
            }
        }
    }
}