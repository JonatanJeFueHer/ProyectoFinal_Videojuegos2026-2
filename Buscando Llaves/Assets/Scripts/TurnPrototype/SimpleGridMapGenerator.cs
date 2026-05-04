using System.Collections.Generic;
using UnityEngine;

public class SimpleGridMapGenerator : MonoBehaviour
{
    public enum MapTileCode
    {
        NoTile = 0,
        HiddenKey = 1,
        TrapRayo = 2,
        ChestBlue = 3,
        ChestOrange = 4,
        ChestPink = 5,
        Empty = 6,
        ChestBlueWithKey = 7,
        ChestOrangeWithKey = 8,
        ChestPinkWithKey = 9
    }

    [Header("References")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform tileParent;
    [SerializeField] private Transform playerTransform;

    [Header("Grid")]
    [SerializeField] private float tileSpacing = 2f;
    [SerializeField] private float playerHeightOffset = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    [Header("Matrix Rows (CSV)")]
    [SerializeField] private List<string> matrixRows = new List<string>
    {
        "6,6,6,6,6,6,6,6,6,6",
        "6,0,0,2,0,3,0,1,0,6",
        "6,6,6,6,6,0,6,6,6,6",
        "0,4,0,0,6,0,7,0,5,0",
        "6,6,6,0,6,6,6,0,6,6",
        "6,0,6,0,2,0,6,0,8,6",
        "6,0,6,6,6,0,6,6,6,6",
        "6,0,0,0,6,0,0,0,6,6",
        "6,9,6,1,6,6,5,6,3,6",
        "6,6,6,6,6,6,6,6,6,6"
    };

    [Header("Debug")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool tintTilesByCode = true;

    private readonly Dictionary<Vector2Int, int> tileCodes = new Dictionary<Vector2Int, int>();
    private readonly Dictionary<Vector2Int, Renderer> tileRenderers = new Dictionary<Vector2Int, Renderer>();
    private bool hasGenerated;
    private Vector2Int startGridPosition = Vector2Int.zero;

    public Vector2Int StartGridPosition => startGridPosition;

    private void Awake()
    {
        if (tileParent == null)
        {
            GameObject tileParentObject = GameObject.Find("TileParent");
            if (tileParentObject != null)
            {
                tileParent = tileParentObject.transform;
            }
        }
    }

    private void Start()
    {
        if (generateOnStart && !hasGenerated)
        {
            GenerateMap();
        }
    }

    public void GenerateMap()
    {
        ClearSpawnedTiles();
        ParseAndSpawnMap();
        ResolveStartGridPosition();
        PlacePlayerAtStart();
        hasGenerated = true;
    }

    public void SetMatrixRows(List<string> rows, bool regenerate = true)
    {
        matrixRows = new List<string>(rows);
        hasGenerated = false;

        if (regenerate)
        {
            GenerateMap();
        }
    }

    public List<string> GetMatrixRowsCopy()
    {
        return new List<string>(matrixRows);
    }

    public List<Vector2Int> GetPositionsByCode(int code)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, int> entry in tileCodes)
        {
            if (entry.Value == code)
            {
                positions.Add(entry.Key);
            }
        }

        return positions;
    }

    public List<Vector2Int> GetWalkablePositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, int> entry in tileCodes)
        {
            if (entry.Value != (int)MapTileCode.NoTile)
            {
                positions.Add(entry.Key);
            }
        }

        return positions;
    }

    [ContextMenu("Load Key Chest Test Map")]
    public void LoadKeyChestTestMap()
    {
        matrixRows = new List<string>
        {
            "6,6,6,6,6,6,6,6,6,6",
            "6,0,0,2,0,3,0,1,0,6",
            "6,6,6,6,6,0,6,6,6,6",
            "0,4,0,0,6,0,7,0,5,0",
            "6,6,6,0,6,6,6,0,6,6",
            "6,0,6,0,2,0,6,0,8,6",
            "6,0,6,6,6,0,6,6,6,6",
            "6,0,0,0,6,0,0,0,6,6",
            "6,9,6,1,6,6,5,6,3,6",
            "6,6,6,6,6,6,6,6,6,6"
        };

        GenerateMap();
    }

    public bool IsWalkable(Vector2Int gridPosition)
    {
        int code;
        if (!tileCodes.TryGetValue(gridPosition, out code))
        {
            return false;
        }

        return code != (int)MapTileCode.NoTile;
    }

    public int GetTileCode(Vector2Int gridPosition)
    {
        int code;
        if (!tileCodes.TryGetValue(gridPosition, out code))
        {
            return (int)MapTileCode.NoTile;
        }

        return code;
    }

    public void SetTileCode(Vector2Int gridPosition, int newCode)
    {
        if (!tileCodes.ContainsKey(gridPosition))
        {
            return;
        }

        tileCodes[gridPosition] = newCode;
        UpdateTileTint(gridPosition, newCode);
    }

    public bool TryGetWorldPosition(Vector2Int gridPosition, out Vector3 worldPosition)
    {
        if (!tileCodes.ContainsKey(gridPosition))
        {
            worldPosition = Vector3.zero;
            return false;
        }

        worldPosition = GridToWorld(gridPosition);
        return true;
    }

    public bool PlacePlayerAtStart()
    {
        if (playerTransform == null)
        {
            return false;
        }

        if (!IsWalkable(StartGridPosition))
        {
            Debug.LogError($"No se puede colocar el jugador en ({StartGridPosition.x},{StartGridPosition.y}). Esa casilla no existe o es 0.");
            return false;
        }

        Vector3 startWorld = GridToWorld(StartGridPosition);
        playerTransform.position = startWorld + Vector3.up * playerHeightOffset;
        return true;
    }

    public Vector2Int GetFirstWalkablePosition()
    {
        return FindFirstWalkablePosition();
    }

    private void ParseAndSpawnMap()
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("SimpleGridMapGenerator: tilePrefab no asignado. Se usaran cubos temporales.");
        }

        for (int row = 0; row < matrixRows.Count; row++)
        {
            if (string.IsNullOrWhiteSpace(matrixRows[row]))
            {
                continue;
            }

            string[] columns = matrixRows[row].Split(',');
            for (int col = 0; col < columns.Length; col++)
            {
                string cellText = columns[col].Trim();
                int code;
                if (!int.TryParse(cellText, out code))
                {
                    code = (int)MapTileCode.NoTile;
                }

                Vector2Int gridPosition = new Vector2Int(col, row);
                tileCodes[gridPosition] = code;

                if (code == (int)MapTileCode.NoTile)
                {
                    SpawnWall(gridPosition);
                    continue;
                }

                Vector3 worldPosition = GridToWorld(gridPosition);
                GameObject tile;
                if (tilePrefab != null)
                {
                    tile = Instantiate(tilePrefab, worldPosition, Quaternion.identity, tileParent != null ? tileParent : transform);
                }
                else
                {
                    tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.transform.SetParent(tileParent != null ? tileParent : transform);
                    tile.transform.position = worldPosition;
                    tile.transform.rotation = Quaternion.identity;
                    tile.transform.localScale = new Vector3(tileSpacing * 0.9f, 0.2f, tileSpacing * 0.9f);
                }

                tile.name = $"Tile_{col}_{row}_Code{code}";

                Renderer rendererComponent = tile.GetComponentInChildren<Renderer>();
                if (rendererComponent != null)
                {
                    tileRenderers[gridPosition] = rendererComponent;
                }

                UpdateTileTint(gridPosition, code);
            }
        }
    }

    private void ResolveStartGridPosition()
    {
        if (IsWalkable(startGridPosition))
        {
            if (HasWalkableNeighbor(startGridPosition))
            {
                return;
            }
        }

        Vector2Int connected = FindFirstConnectedWalkablePosition();
        if (connected != Vector2Int.zero || IsWalkable(Vector2Int.zero))
        {
            startGridPosition = connected;
            return;
        }

        startGridPosition = FindFirstWalkablePosition();
    }

    private Vector2Int FindFirstWalkablePosition()
    {
        int bestRow = int.MaxValue;
        int bestCol = int.MaxValue;

        foreach (KeyValuePair<Vector2Int, int> entry in tileCodes)
        {
            if (entry.Value == (int)MapTileCode.NoTile)
            {
                continue;
            }

            int col = entry.Key.x;
            int row = entry.Key.y;

            if (row < bestRow || (row == bestRow && col < bestCol))
            {
                bestRow = row;
                bestCol = col;
            }
        }

        if (bestRow == int.MaxValue)
        {
            return Vector2Int.zero;
        }

        return new Vector2Int(bestCol, bestRow);
    }

    private Vector2Int FindFirstConnectedWalkablePosition()
    {
        int bestRow = int.MaxValue;
        int bestCol = int.MaxValue;
        bool found = false;

        foreach (KeyValuePair<Vector2Int, int> entry in tileCodes)
        {
            if (entry.Value == (int)MapTileCode.NoTile)
            {
                continue;
            }

            Vector2Int p = entry.Key;
            if (!HasWalkableNeighbor(p))
            {
                continue;
            }

            int col = p.x;
            int row = p.y;
            if (row < bestRow || (row == bestRow && col < bestCol))
            {
                bestRow = row;
                bestCol = col;
                found = true;
            }
        }

        if (!found)
        {
            return Vector2Int.zero;
        }

        return new Vector2Int(bestCol, bestRow);
    }

    private bool HasWalkableNeighbor(Vector2Int p)
    {
        return IsWalkable(p + Vector2Int.up)
            || IsWalkable(p + Vector2Int.down)
            || IsWalkable(p + Vector2Int.left)
            || IsWalkable(p + Vector2Int.right);
    }

    private void SpawnWall(Vector2Int gridPosition)
    {
        if (wallPrefab == null)
        {
            return;
        }

        Vector3 worldPosition = GridToWorld(gridPosition);
        GameObject wall = Instantiate(wallPrefab, worldPosition, Quaternion.identity, tileParent != null ? tileParent : transform);
        wall.name = $"Wall_{gridPosition.x}_{gridPosition.y}";
    }

    private void ClearSpawnedTiles()
    {
        tileCodes.Clear();
        tileRenderers.Clear();

        Transform parent = tileParent != null ? tileParent : transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return origin + new Vector3(gridPosition.x * tileSpacing, 0f, -gridPosition.y * tileSpacing);
    }

    private void UpdateTileTint(Vector2Int gridPosition, int code)
    {
        if (!tintTilesByCode)
        {
            return;
        }

        Renderer rendererComponent;
        if (!tileRenderers.TryGetValue(gridPosition, out rendererComponent))
        {
            return;
        }

        if (rendererComponent.material == null)
        {
            return;
        }

        rendererComponent.material.color = GetColorForCode(code);
    }

    private Color GetColorForCode(int code)
    {
        switch (code)
        {
            case (int)MapTileCode.HiddenKey:
                return new Color(1f, 1f, 0.4f);
            case (int)MapTileCode.TrapRayo:
                return new Color(0.6f, 0.8f, 1f);
            case (int)MapTileCode.ChestBlue:
                return new Color(0.4f, 0.6f, 1f);
            case (int)MapTileCode.ChestOrange:
                return new Color(1f, 0.6f, 0.2f);
            case (int)MapTileCode.ChestPink:
                return new Color(1f, 0.5f, 0.8f);
            case (int)MapTileCode.ChestBlueWithKey:
                return new Color(0.2f, 0.35f, 1f);
            case (int)MapTileCode.ChestOrangeWithKey:
                return new Color(1f, 0.4f, 0.1f);
            case (int)MapTileCode.ChestPinkWithKey:
                return new Color(1f, 0.25f, 0.7f);
            case (int)MapTileCode.Empty:
                return new Color(0.8f, 0.8f, 0.8f);
            default:
                return Color.white;
        }
    }
}
