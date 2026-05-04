using System;
using System.Collections.Generic;
using UnityEngine;

public class RegionPatternLibrary : MonoBehaviour
{
    [Header("Tile Codes")]
    [SerializeField] private int wallCode = 0;
    [SerializeField] private int floorCode = 6;

    [Header("Corner Patterns (5x5, base orientation = top-left corner)")]
    [SerializeField] private List<string[]> cornerPatternsCsv = new List<string[]>
    {
        new[]
        {
            "0,0,0,0,0",
            "0,6,6,6,0",
            "0,6,6,6,0",
            "0,6,6,6,0",
            "0,0,0,6,0"
        },
        new[]
        {
            "0,0,0,0,0",
            "0,6,6,6,0",
            "0,6,0,6,0",
            "0,6,0,6,0",
            "0,0,0,6,0"
        },
        new[]
        {
            "0,0,0,0,0",
            "0,6,6,6,0",
            "0,6,0,6,0",
            "0,0,0,6,0",
            "0,6,6,6,0"
        },
        new[]
        {
            "0,0,0,0,0",
            "0,6,6,0,6",
            "0,6,6,0,6",
            "0,6,0,0,6",
            "0,6,6,6,6"
        }
    };

    [Header("Corridor Patterns (5x5, base orientation = left corridor)")]
    [SerializeField] private List<string[]> corridorPatternsCsv = new List<string[]>
    {
        new[]
        {
            "0,6,6,6,6",
            "0,0,0,0,6",
            "0,0,0,0,6",
            "0,0,0,0,6",
            "0,6,6,6,6"
        },
        new[]
        {
            "0,6,6,6,6",
            "0,6,6,0,6",
            "0,0,0,0,6",
            "0,6,6,0,6",
            "0,6,6,6,6"
        },
        new[]
        {
            "0,6,6,6,6",
            "0,0,0,0,6",
            "0,6,6,6,6",
            "0,0,0,0,6",
            "0,6,6,6,6"
        },
        new[]
        {
            "0,6,6,6,6",
            "0,0,0,0,6",
            "0,6,6,6,6",
            "0,0,0,0,6",
            "0,6,6,6,6"
        }
    };

    [Header("Center Patterns (5x5, all sides open to connect regions)")]
    [SerializeField] private List<string[]> centerPatternsCsv = new List<string[]>
    {
        new[]
        {
            "0,0,6,0,0",
            "0,6,6,6,0",
            "6,6,0,6,6",
            "0,6,6,6,0",
            "0,0,6,0,0"
        },
        new[]
        {
            "0,6,6,6,0",
            "6,6,0,6,6",
            "6,0,6,0,6",
            "6,6,0,6,6",
            "0,6,6,6,0"
        }
    };

    public int WallCode => wallCode;
    public int FloorCode => floorCode;

    public List<int[,]> GetRandomCornerSet(int count)
    {
        return GetRandomSet(cornerPatternsCsv, count);
    }

    public List<int[,]> GetRandomCorridorSet(int count)
    {
        return GetRandomSet(corridorPatternsCsv, count);
    }

    public int[,] GetRandomCenter()
    {
        if (centerPatternsCsv == null || centerPatternsCsv.Count == 0)
        {
            return CreateFilled(5, 5, floorCode);
        }

        int index = UnityEngine.Random.Range(0, centerPatternsCsv.Count);
        return ParseCsvBlock(centerPatternsCsv[index]);
    }

    private List<int[,]> GetRandomSet(List<string[]> source, int count)
    {
        List<int[,]> result = new List<int[,]>();
        if (source == null || source.Count == 0 || count <= 0)
        {
            return result;
        }

        List<int> bag = new List<int>();
        for (int i = 0; i < source.Count; i++)
        {
            bag.Add(i);
        }

        for (int i = 0; i < count; i++)
        {
            if (bag.Count == 0)
            {
                for (int j = 0; j < source.Count; j++)
                {
                    bag.Add(j);
                }
            }

            int pickBagIndex = UnityEngine.Random.Range(0, bag.Count);
            int selected = bag[pickBagIndex];
            bag.RemoveAt(pickBagIndex);

            result.Add(ParseCsvBlock(source[selected]));
        }

        return result;
    }

    private int[,] ParseCsvBlock(string[] rows)
    {
        if (rows == null || rows.Length == 0)
        {
            return CreateFilled(5, 5, floorCode);
        }

        int height = rows.Length;
        string[] first = rows[0].Split(',');
        int width = first.Length;
        int[,] matrix = new int[height, width];

        for (int y = 0; y < height; y++)
        {
            string[] cols = rows[y].Split(',');
            for (int x = 0; x < width; x++)
            {
                int value = floorCode;
                if (x < cols.Length)
                {
                    int.TryParse(cols[x].Trim(), out value);
                }

                matrix[y, x] = value;
            }
        }

        return matrix;
    }

    private int[,] CreateFilled(int rows, int cols, int value)
    {
        int[,] matrix = new int[rows, cols];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                matrix[y, x] = value;
            }
        }

        return matrix;
    }

    public static int[,] RotateClockwise(int[,] source, int quarterTurns)
    {
        int turns = ((quarterTurns % 4) + 4) % 4;
        int[,] result = source;

        for (int i = 0; i < turns; i++)
        {
            int rows = result.GetLength(0);
            int cols = result.GetLength(1);
            int[,] rotated = new int[cols, rows];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    rotated[x, rows - 1 - y] = result[y, x];
                }
            }

            result = rotated;
        }

        return result;
    }
}
