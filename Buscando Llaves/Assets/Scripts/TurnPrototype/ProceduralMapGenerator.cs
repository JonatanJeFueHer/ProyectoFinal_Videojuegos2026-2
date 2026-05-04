using System.Collections.Generic;
using UnityEngine;

public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RegionPatternLibrary patternLibrary;
    [SerializeField] private SimpleGridMapGenerator mapGenerator;
    [SerializeField] private MapPostProcessController postProcessController;
    [SerializeField] private TileHiderAI tileHiderAI;

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool applyTileHiderAfterGenerate = true;

    [Header("Debug")]
    [SerializeField] private bool logResult = true;

    private const int RegionSize = 5;
    private const int GridRegions = 3;
    private const int FinalSize = RegionSize * GridRegions;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateMap();
        }
    }

    [ContextMenu("Generate Procedural Map")]
    public void GenerateMap()
    {
        if (patternLibrary == null || mapGenerator == null)
        {
            Debug.LogWarning("ProceduralMapGenerator: Missing references.");
            return;
        }

        int[,] finalMap = new int[FinalSize, FinalSize];
        FillMatrix(finalMap, patternLibrary.WallCode);

        List<int[,]> corners = patternLibrary.GetRandomCornerSet(4);
        List<int[,]> corridors = patternLibrary.GetRandomCorridorSet(4);
        int[,] center = patternLibrary.GetRandomCenter();

        if (corners.Count < 4 || corridors.Count < 4 || center == null)
        {
            Debug.LogWarning("ProceduralMapGenerator: Pattern set is incomplete.");
            return;
        }

        // Corners
        PlaceRegion(finalMap, RegionPatternLibrary.RotateClockwise(corners[0], 0), 0, 0);
        PlaceRegion(finalMap, RegionPatternLibrary.RotateClockwise(corners[1], 1), 0, 2);
        PlaceRegion(finalMap, RegionPatternLibrary.RotateClockwise(corners[2], 2), 2, 2);
        PlaceRegion(finalMap, RegionPatternLibrary.RotateClockwise(corners[3], 3), 2, 0);

        // Corridors (catalog base orientation = left side)
        // top = +90, right = +180, bottom = +270, left = +0
        PlaceRegion(finalMap, RegionPatternLibrary.RotateClockwise(corridors[0], 1), 0, 1);
        PlaceRegion(finalMap, RegionPatternLibrary.RotateClockwise(corridors[1], 2), 1, 2);
        PlaceRegion(finalMap, RegionPatternLibrary.RotateClockwise(corridors[2], 3), 2, 1);
        PlaceRegion(finalMap, RegionPatternLibrary.RotateClockwise(corridors[3], 0), 1, 0);

        // Center
        PlaceRegion(finalMap, center, 1, 1);

        List<string> csvRows = ToCsvRows(finalMap);
        mapGenerator.SetMatrixRows(csvRows, true);

        if (logResult)
        {
            Debug.Log("ProceduralMapGenerator: 15x15 map generated.");
        }

        if (applyTileHiderAfterGenerate && tileHiderAI != null)
        {
            tileHiderAI.HideContentOnCurrentMap();
        }
        else if (postProcessController != null)
        {
            postProcessController.NotifyMapUpdated();
        }
    }

    private void FillMatrix(int[,] matrix, int value)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                matrix[y, x] = value;
            }
        }
    }

    private void PlaceRegion(int[,] finalMap, int[,] region, int regionRow, int regionCol)
    {
        int startY = regionRow * RegionSize;
        int startX = regionCol * RegionSize;

        for (int y = 0; y < RegionSize; y++)
        {
            for (int x = 0; x < RegionSize; x++)
            {
                finalMap[startY + y, startX + x] = region[y, x];
            }
        }
    }

    private List<string> ToCsvRows(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        List<string> result = new List<string>(rows);

        for (int y = 0; y < rows; y++)
        {
            string[] rowValues = new string[cols];
            for (int x = 0; x < cols; x++)
            {
                rowValues[x] = matrix[y, x].ToString();
            }

            result.Add(string.Join(",", rowValues));
        }

        return result;
    }
}
