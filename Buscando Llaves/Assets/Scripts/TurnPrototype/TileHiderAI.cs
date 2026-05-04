using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TileHiderAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProceduralMapGenerator proceduralMapGenerator;
    [SerializeField] private SimpleGridMapGenerator mapGenerator;
    [SerializeField] private MapPostProcessController postProcessController;

    [Header("Spawn Counts")]
    [SerializeField] private int keyCount = 2;
    [SerializeField] private int rayTrapCount = 4;
    [SerializeField] private int chestWithKeyCount = 2;
    [SerializeField] private int chestWithoutKeyCount = 3;

    [Header("Run")]
    [SerializeField] private bool runOnStart;
    [SerializeField] private bool regenerateMapBeforeHide;
    [SerializeField] private bool clearOldSpecialTiles = true;
    [SerializeField] private bool waitEndOfFrameOnStart = true;

    [Header("Codes")]
    [SerializeField] private int emptyTileCode = 6;
    [SerializeField] private int keyTileCode = 1;
    [SerializeField] private int rayTrapTileCode = 2;
    [SerializeField] private int chestWithKeyTileCode = 7;
    [SerializeField] private int chestWithoutKeyTileCode = 8;

    private readonly List<int> specialCodes = new List<int> { 1, 2, 7, 8 };
    private bool isApplying;

    private void Start()
    {
        if (runOnStart)
        {
            if (waitEndOfFrameOnStart)
            {
                StartCoroutine(HideAtEndOfFrame());
            }
            else
            {
                HideContent();
            }
        }
    }

    [ContextMenu("Hide Content On Map")]
    public void HideContent()
    {
        HideContentInternal(regenerateMapBeforeHide);
    }

    public void HideContentOnCurrentMap()
    {
        HideContentInternal(false);
    }

    private IEnumerator HideAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        HideContent();
    }

    private void HideContentInternal(bool shouldRegenerateBeforeHide)
    {
        if (isApplying)
        {
            return;
        }

        isApplying = true;

        if (mapGenerator == null)
        {
            Debug.LogWarning("TileHiderAI: Missing mapGenerator reference.");
            isApplying = false;
            return;
        }

        if (shouldRegenerateBeforeHide && proceduralMapGenerator != null)
        {
            proceduralMapGenerator.GenerateMap();
        }

        int[,] matrix = ReadMatrixFromGenerator();
        if (matrix == null)
        {
            Debug.LogWarning("TileHiderAI: no se pudo leer la matriz del mapa.");
            isApplying = false;
            return;
        }

        if (clearOldSpecialTiles)
        {
            ClearSpecialTilesInMatrix(matrix);
        }

        PlaceRandomInMatrix(matrix, keyTileCode, keyCount);
        PlaceRandomInMatrix(matrix, rayTrapTileCode, rayTrapCount);
        PlaceRandomInMatrix(matrix, chestWithKeyTileCode, chestWithKeyCount);
        PlaceRandomInMatrix(matrix, chestWithoutKeyTileCode, chestWithoutKeyCount);

        mapGenerator.SetMatrixRows(MatrixToRows(matrix), true);

        if (postProcessController != null)
        {
            postProcessController.NotifyMapUpdated();
        }

        int keys = CountCode(matrix, keyTileCode);
        int rays = CountCode(matrix, rayTrapTileCode);
        int chestWithKey = CountCode(matrix, chestWithKeyTileCode);
        int chestWithoutKey = CountCode(matrix, chestWithoutKeyTileCode);
        int totalSpecial = keys + rays + chestWithKey + chestWithoutKey;
        Debug.Log("TileHiderAI: especiales colocados en matriz = " + totalSpecial + " (K=" + keys + ", R=" + rays + ", CK=" + chestWithKey + ", C=" + chestWithoutKey + ")");

        isApplying = false;
    }

    private int[,] ReadMatrixFromGenerator()
    {
        List<string> rows = mapGenerator.GetMatrixRowsCopy();
        if (rows == null || rows.Count == 0)
        {
            return null;
        }

        int h = rows.Count;
        int w = rows[0].Split(',').Length;
        int[,] matrix = new int[h, w];

        for (int y = 0; y < h; y++)
        {
            string[] cols = rows[y].Split(',');
            for (int x = 0; x < w; x++)
            {
                int value = 0;
                if (x < cols.Length)
                {
                    int.TryParse(cols[x].Trim(), out value);
                }

                matrix[y, x] = value;
            }
        }

        return matrix;
    }

    private List<string> MatrixToRows(int[,] matrix)
    {
        int h = matrix.GetLength(0);
        int w = matrix.GetLength(1);
        List<string> rows = new List<string>(h);

        for (int y = 0; y < h; y++)
        {
            string[] cols = new string[w];
            for (int x = 0; x < w; x++)
            {
                cols[x] = matrix[y, x].ToString();
            }

            rows.Add(string.Join(",", cols));
        }

        return rows;
    }

    private void ClearSpecialTilesInMatrix(int[,] matrix)
    {
        int h = matrix.GetLength(0);
        int w = matrix.GetLength(1);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (specialCodes.Contains(matrix[y, x]))
                {
                    matrix[y, x] = emptyTileCode;
                }
            }
        }
    }

    private void PlaceRandomInMatrix(int[,] matrix, int tileCode, int count)
    {
        int h = matrix.GetLength(0);
        int w = matrix.GetLength(1);

        for (int i = 0; i < count; i++)
        {
            List<Vector2Int> candidates = new List<Vector2Int>();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (matrix[y, x] == emptyTileCode)
                    {
                        candidates.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (candidates == null || candidates.Count == 0)
            {
                return;
            }

            int index = UnityEngine.Random.Range(0, candidates.Count);
            Vector2Int pos = candidates[index];
            matrix[pos.y, pos.x] = tileCode;
        }
    }

    private int CountCode(int[,] matrix, int code)
    {
        int count = 0;
        int h = matrix.GetLength(0);
        int w = matrix.GetLength(1);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (matrix[y, x] == code)
                {
                    count++;
                }
            }
        }

        return count;
    }
}
