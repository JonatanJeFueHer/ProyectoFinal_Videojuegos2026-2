using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MapPostProcessController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SimpleGridMapGenerator mapGenerator;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform markersParent;

    [Header("Run")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool waitOneFrameOnStart = true;

    [Header("Walkable Codes")]
    [SerializeField] private List<int> walkableCodes = new List<int> { 6, 1, 2, 7, 8 };
    [SerializeField] private float playerYOffset = 1f;

    [Header("Special Markers")]
    [SerializeField] private bool drawSpecialMarkers = true;
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private float markerYOffset = 0.1f;
    [SerializeField] private Vector3 markerScale = new Vector3(0.6f, 0.2f, 0.6f);
    [SerializeField] private Color keyColor = new Color(1f, 0.95f, 0.2f, 1f);
    [SerializeField] private Color trapColor = new Color(0.2f, 0.8f, 1f, 1f);
    [SerializeField] private Color chestWithKeyColor = new Color(0.9f, 0.2f, 0.9f, 1f);
    [SerializeField] private Color chestWithoutKeyColor = new Color(1f, 0.5f, 0.1f, 1f);

    private void Start()
    {
        if (!runOnStart)
        {
            return;
        }

        StartCoroutine(RefreshOnStartRoutine());
    }

    private IEnumerator RefreshOnStartRoutine()
    {
        if (waitOneFrameOnStart)
        {
            yield return null;
        }

        NotifyMapUpdated();
    }

    public void NotifyMapUpdated()
    {
        int[,] matrix = ReadMatrixFromGenerator();
        if (matrix == null)
        {
            Debug.LogWarning("MapPostProcessController: Could not read matrix.");
            return;
        }

        PlacePlayerOnFirstWalkable(matrix);

        if (drawSpecialMarkers)
        {
            RebuildSpecialMarkers(matrix);
        }
    }

    private void PlacePlayerOnFirstWalkable(int[,] matrix)
    {
        if (playerTransform == null)
        {
            return;
        }

        if (mapGenerator != null)
        {
            Vector2Int start = mapGenerator.StartGridPosition;
            Vector3 startWorld;
            if (mapGenerator.TryGetWorldPosition(start, out startWorld))
            {
                playerTransform.position = new Vector3(startWorld.x, startWorld.y + playerYOffset, startWorld.z);
                SyncMovementGrid(start);
                return;
            }
        }

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (!walkableCodes.Contains(matrix[y, x]))
                {
                    continue;
                }

                Vector3 world = GetWorldPosition(x, y);
                playerTransform.position = new Vector3(world.x, world.y + playerYOffset, world.z);
                SyncMovementGrid(new Vector2Int(x, y));
                return;
            }
        }
    }

    private void SyncMovementGrid(Vector2Int gridPos)
    {
        if (playerTransform == null)
        {
            return;
        }

        SimpleGridTurnMovement movement = playerTransform.GetComponent<SimpleGridTurnMovement>();
        if (movement != null)
        {
            movement.ForceGridPosition(gridPos);
        }
    }

    private void RebuildSpecialMarkers(int[,] matrix)
    {
        EnsureMarkersParent();

        for (int i = markersParent.childCount - 1; i >= 0; i--)
        {
            Transform child = markersParent.GetChild(i);
            if (child.GetComponent<SpecialTileMarkerTag>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int code = matrix[y, x];
                if (code != 1 && code != 2 && code != 7 && code != 8)
                {
                    continue;
                }

                GameObject marker = CreateMarker();
                marker.transform.SetParent(markersParent, true);

                Vector3 world = GetWorldPosition(x, y);
                marker.transform.position = new Vector3(world.x, world.y + markerYOffset, world.z);
                marker.transform.localScale = markerScale;

                Renderer r = marker.GetComponent<Renderer>();
                if (r != null)
                {
                    r.material.color = GetColorByCode(code);
                }
            }
        }
    }

    private GameObject CreateMarker()
    {
        if (markerPrefab != null)
        {
            GameObject instance = Instantiate(markerPrefab);
            if (instance.GetComponent<SpecialTileMarkerTag>() == null)
            {
                instance.AddComponent<SpecialTileMarkerTag>();
            }

            return instance;
        }

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "SpecialTileMarker";
        cube.AddComponent<SpecialTileMarkerTag>();
        Collider c = cube.GetComponent<Collider>();
        if (c != null)
        {
            Destroy(c);
        }

        return cube;
    }

    private void EnsureMarkersParent()
    {
        if (markersParent != null)
        {
            return;
        }

        GameObject go = new GameObject("SpecialTileMarkers");
        markersParent = go.transform;
    }

    private Color GetColorByCode(int code)
    {
        if (code == 1) return keyColor;
        if (code == 2) return trapColor;
        if (code == 7) return chestWithKeyColor;
        if (code == 8) return chestWithoutKeyColor;
        return Color.white;
    }

    private int[,] ReadMatrixFromGenerator()
    {
        if (mapGenerator == null)
        {
            return null;
        }

        List<string> rows = ReadRowsFieldOrMethod();
        if (rows == null || rows.Count == 0)
        {
            return null;
        }

        int height = rows.Count;
        string[] first = rows[0].Split(',');
        int width = first.Length;
        int[,] matrix = new int[height, width];

        for (int y = 0; y < height; y++)
        {
            string[] cols = rows[y].Split(',');
            for (int x = 0; x < width; x++)
            {
                int v = 0;
                if (x < cols.Length)
                {
                    int.TryParse(cols[x].Trim(), out v);
                }

                matrix[y, x] = v;
            }
        }

        return matrix;
    }

    private List<string> ReadRowsFieldOrMethod()
    {
        object target = mapGenerator;
        System.Type t = target.GetType();

        MethodInfo getter = t.GetMethod("GetMatrixRows", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (getter != null && getter.GetParameters().Length == 0 && typeof(List<string>).IsAssignableFrom(getter.ReturnType))
        {
            return getter.Invoke(target, null) as List<string>;
        }

        string[] candidates = { "matrixRows", "matrixRowsCsv", "rowsCsv", "rows" };
        for (int i = 0; i < candidates.Length; i++)
        {
            FieldInfo f = t.GetField(candidates[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && typeof(List<string>).IsAssignableFrom(f.FieldType))
            {
                return f.GetValue(target) as List<string>;
            }
        }

        return null;
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        if (mapGenerator == null)
        {
            return Vector3.zero;
        }

        object target = mapGenerator;
        System.Type t = target.GetType();

        // First try the concrete API from SimpleGridMapGenerator.
        MethodInfo tryGetWorld = t.GetMethod("TryGetWorldPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (tryGetWorld != null)
        {
            ParameterInfo[] p = tryGetWorld.GetParameters();
            if (p.Length == 2 &&
                p[0].ParameterType == typeof(Vector2Int) &&
                p[1].IsOut &&
                p[1].ParameterType == typeof(Vector3).MakeByRefType() &&
                tryGetWorld.ReturnType == typeof(bool))
            {
                object[] args = { new Vector2Int(x, y), Vector3.zero };
                bool ok = (bool)tryGetWorld.Invoke(target, args);
                if (ok)
                {
                    return (Vector3)args[1];
                }
            }
        }

        string[] methods = { "GetTileWorldPosition", "GetWorldPosition", "GetCellWorldPosition" };
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo m = t.GetMethod(methods[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (m == null) continue;

            ParameterInfo[] p = m.GetParameters();
            if (p.Length == 2 && p[0].ParameterType == typeof(int) && p[1].ParameterType == typeof(int) && m.ReturnType == typeof(Vector3))
            {
                return (Vector3)m.Invoke(target, new object[] { x, y });
            }
        }

        Vector3 origin = ReadVector3Field(t, target, "origin", Vector3.zero);
        float spacing = ReadFloatField(t, target, "tileSpacing", 2f);
        // Match SimpleGridMapGenerator.GridToWorld: Z goes negative with row.
        return origin + new Vector3(x * spacing, 0f, -y * spacing);
    }

    private Vector3 ReadVector3Field(System.Type t, object target, string fieldName, Vector3 fallback)
    {
        FieldInfo f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(Vector3))
        {
            return (Vector3)f.GetValue(target);
        }

        return fallback;
    }

    private float ReadFloatField(System.Type t, object target, string fieldName, float fallback)
    {
        FieldInfo f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(float))
        {
            return (float)f.GetValue(target);
        }

        return fallback;
    }
}
