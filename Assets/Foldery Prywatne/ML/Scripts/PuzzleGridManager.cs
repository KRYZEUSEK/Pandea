using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGridManager : MonoBehaviour
{
    [SerializeField] private float maxBoardWidth = 500f;
    [SerializeField] private float maxBoardHeight = 500f;

    [Header("grid setup")]
    [SerializeField] private PuzzleTileView tilePrefab;
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;

    [Header("level source")]
    [SerializeField] private PuzzleLevelData currentLevel;
    [SerializeField] private PuzzleLevelSet currentLevelSet;
    [SerializeField] private bool useRandomLevelFromSet = false;

    [Header("grid visuals")]
    [SerializeField] private UnityEngine.UI.GridLayoutGroup gridLayoutGroup;
    [SerializeField] private RectTransform gridRectTransform;
    [SerializeField] private float tileSize = 100f;
    [SerializeField] private float spacing = 0f;

    private PuzzleTileView[,] spawnedTiles;
    private bool[,] poweredTiles;

    private int[,] initialRotations;
    private PuzzleTileView.TileShape[,] initialShapes;

    private void Start()
    {
        LoadSelectedLevel();
        ResetPoweredTiles();
        ApplyPoweredStateToTiles();
    }

    private void GenerateLevel(PuzzleLevelData level)
    {
        ClearGrid();

        width = level.width;
        height = level.height;

        UpdateGridLayout();

        int expectedCount = width * height;

        if (level.tiles == null || level.tiles.Count != expectedCount)
        {
            Debug.LogError($"Level {level.levelId} ma złą liczbę pól. Oczekiwano {expectedCount}, jest {level.tiles?.Count ?? 0}.");
            return;
        }

        spawnedTiles = new PuzzleTileView[width, height];
        poweredTiles = new bool[width, height];
        initialRotations = new int[width, height];
        initialShapes = new PuzzleTileView.TileShape[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                PuzzleTileData tileData = level.tiles[index];

                PuzzleTileView tile = Instantiate(tilePrefab, transform);

                tile.SetShape(tileData.shape);
                tile.SetRotation(tileData.rotation);
                tile.SetRotatable(tileData.rotatable);

                initialShapes[x, y] = tileData.shape;
                initialRotations[x, y] = tileData.rotation;

                spawnedTiles[x, y] = tile;
            }
        }
    }

    private void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private bool AreConnected(PuzzleTileView a, PuzzleTileView b, Direction dirFromA)
    {
        if (a == null || b == null)
            return false;

        Direction opposite = GetOpposite(dirFromA);

        return a.HasConnection(dirFromA) && b.HasConnection(opposite);
    }

    private Direction GetOpposite(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Direction.Down;
            case Direction.Right: return Direction.Left;
            case Direction.Down: return Direction.Up;
            case Direction.Left: return Direction.Right;
            default: return Direction.Up;
        }
    }

    private PuzzleTileView GetTile(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
            return null;

        return spawnedTiles[x, y];
    }
    private Vector2Int FindSource()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (spawnedTiles[x, y].Shape == PuzzleTileView.TileShape.Source)
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(-1, -1);
    }

    private bool CheckConnectionToTarget()
    {
        Vector2Int sourcePos = FindSource();

        if (sourcePos.x == -1)
        {
            Debug.Log("brak source");
            return false;
        }

        bool[,] visited = new bool[width, height];

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(sourcePos);
        visited[sourcePos.x, sourcePos.y] = true;
        poweredTiles[sourcePos.x, sourcePos.y] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            PuzzleTileView tile = GetTile(current.x, current.y);

            // jeśli dotarliśmy do target
            if (tile.Shape == PuzzleTileView.TileShape.Target)
            {
                Debug.Log("TARGET OSIĄGNIĘTY");
                return true;
            }

            // sprawdzamy 4 kierunki
            TryMove(current, Direction.Up, 0, -1, queue, visited);
            TryMove(current, Direction.Right, 1, 0, queue, visited);
            TryMove(current, Direction.Down, 0, 1, queue, visited);
            TryMove(current, Direction.Left, -1, 0, queue, visited);
        }

        Debug.Log("brak połączenia");
        return false;
    }

    private void TryMove(Vector2Int pos, Direction dir, int dx, int dy, Queue<Vector2Int> queue, bool[,] visited)
    {
        int newX = pos.x + dx;
        int newY = pos.y + dy;

        PuzzleTileView current = GetTile(pos.x, pos.y);
        PuzzleTileView next = GetTile(newX, newY);

        if (next == null)
            return;

        if (visited[newX, newY])
            return;

        if (AreConnected(current, next, dir))
        {
            visited[newX, newY] = true;
            poweredTiles[newX, newY] = true;
            queue.Enqueue(new Vector2Int(newX, newY));
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteFlow();
        }
    }
    private void ExecuteFlow()
    {
        ResetPoweredTiles();

        bool success = CheckConnectionToTarget();

        ApplyPoweredStateToTiles();

        if (success)
        {
            HandleSuccess();
        }
        else
        {
            StartCoroutine(HandleFailureSequence());
        }
    }

    private void ApplyPoweredStateToTiles()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PuzzleTileView tile = spawnedTiles[x, y];

                if (tile != null)
                {
                    tile.SetPowered(poweredTiles[x, y]);
                }
            }
        }
    }
    private void ResetPoweredTiles()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                poweredTiles[x, y] = false;
            }
        }
    }
    private void DebugPoweredTiles()
    {
        for (int y = 0; y < height; y++)
        {
            string row = "";

            for (int x = 0; x < width; x++)
            {
                row += poweredTiles[x, y] ? "[X]" : "[ ]";
            }

            Debug.Log(row);
        }
    }
    private void ResetBoardToInitialState()
    {
        ResetPoweredTiles();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PuzzleTileView tile = spawnedTiles[x, y];

                if (tile == null)
                    continue;

                tile.SetShape(initialShapes[x, y]);
                tile.SetRotation(initialRotations[x, y]);
                tile.SetPowered(false);
                tile.SetFailureFlash(false);
                tile.SetSuccessState(false);
            }
        }
    }
    private void HandleSuccess()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PuzzleTileView tile = spawnedTiles[x, y];
                if (tile == null)
                    continue;

                if (tile.Shape == PuzzleTileView.TileShape.Target)
                {
                    tile.SetSuccessState(true);
                }
            }
        }

        Debug.Log("execute: sukces");
    }
    private IEnumerator HandleFailureSequence()
    {
        Debug.Log("execute: błąd");

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PuzzleTileView tile = spawnedTiles[x, y];
                if (tile == null)
                    continue;

                tile.SetFailureFlash(true);
            }
        }

        // odśwież wygląd po ustawieniu failure
        ApplyPoweredStateToTiles();

        yield return new WaitForSeconds(0.35f);

        ResetBoardToInitialState();
    }

    private void LoadSelectedLevel()
    {
        PuzzleLevelData levelToLoad = null;

        if (useRandomLevelFromSet && currentLevelSet != null && currentLevelSet.levels.Count > 0)
        {
            int randomIndex = Random.Range(0, currentLevelSet.levels.Count);
            levelToLoad = currentLevelSet.levels[randomIndex];
        }
        else
        {
            levelToLoad = currentLevel;
        }

        if (levelToLoad == null)
        {
            Debug.LogError("Brak poziomu do załadowania.");
            return;
        }

        GenerateLevel(levelToLoad);
    }

    private void UpdateGridLayout()
    {
        if (gridLayoutGroup == null || gridRectTransform == null)
            return;

        gridLayoutGroup.cellSize = new Vector2(tileSize, tileSize);
        gridLayoutGroup.spacing = new Vector2(spacing, spacing);
        gridLayoutGroup.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = width;

        float totalWidth = width * tileSize + (width - 1) * spacing;
        float totalHeight = height * tileSize + (height - 1) * spacing;

        gridRectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);
    }
}
