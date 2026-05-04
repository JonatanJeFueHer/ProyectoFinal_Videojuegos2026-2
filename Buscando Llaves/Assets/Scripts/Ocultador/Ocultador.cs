using UnityEngine;
using System.Collections.Generic;

/// Script controlador para distribuir ítems (llaves y trampas) de manera uniforme
/// en el plano del juego al inicio de la partida.
/// Distribuye 2 llaves y 2 trampas en un plano de 9x6.
public class Ocultador : MonoBehaviour
{
    [Header("Configuración del Plano")]
    [Tooltip("Ancho del plano (eje X)")]
    public int planeWidth = 9;

    [Tooltip("Alto del plano (eje Y)")]
    public int planeHeight = 6;

    [Header("Ítems a Distribuir")]
    [Tooltip("Prefab de la llave")]
    public GameObject keyPrefab;

    [Tooltip("Prefab de la trampa")]
    public GameObject trapPrefab;

    [Tooltip("Cantidad de llaves a distribuir")]
    public int keyCount = 2;

    [Tooltip("Cantidad de trampas a distribuir")]
    public int trapCount = 2;

    [Header("Distribución")]
    [Tooltip("Offset Y para ajustar la altura de los ítems en el mundo")]
    public float itemHeightOffset = 0.5f;

    [Tooltip("Offset Z para evitar solapamientos")]
    public float itemZOffset = 0f;

    void Start()
    {
        DistributeItems();
    }

    
    /// Distribuye los ítems (llaves y trampas) de manera uniforme en el plano.
    void DistributeItems()
    {
        // Validar prefabs
        if (keyPrefab == null || trapPrefab == null)
        {
            Debug.LogError("Ocultador: Los prefabs de llave y trampa no están asignados");
            return;
        }

        // Crear lista de posiciones disponibles
        List<Vector2Int> availablePositions = GenerateAvailablePositions();

        // Barajar las posiciones
        ShufflePositions(availablePositions);

        // Verificar que hay suficientes posiciones
        int totalItemsNeeded = keyCount + trapCount;
        if (availablePositions.Count < totalItemsNeeded)
        {
            Debug.LogWarning($"Ocultador: No hay suficientes posiciones. Necesarias: {totalItemsNeeded}, Disponibles: {availablePositions.Count}");
            return;
        }

        // Distribuir llaves
        for (int i = 0; i < keyCount; i++)
        {
            Vector3 worldPos = GridToWorldPosition(availablePositions[i]);
            Instantiate(keyPrefab, worldPos, Quaternion.identity, transform);
        }

        // Distribuir trampas
        for (int i = 0; i < trapCount; i++)
        {
            Vector3 worldPos = GridToWorldPosition(availablePositions[keyCount + i]);
            Instantiate(trapPrefab, worldPos, Quaternion.identity, transform);
        }

        Debug.Log($"Ocultador: Se distribuyeron {keyCount} llaves y {trapCount} trampas de manera uniforme");
    }

    /// Genera una lista con todas las posiciones disponibles en el plano.
    List<Vector2Int> GenerateAvailablePositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int x = 0; x < planeWidth; x++)
        {
            for (int y = 0; y < planeHeight; y++)
            {
                positions.Add(new Vector2Int(x, y));
            }
        }

        return positions;
    }

    /// Baraja la lista de posiciones usando el algoritmo Fisher-Yates.
    void ShufflePositions(List<Vector2Int> positions)
    {
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            // Intercambiar
            Vector2Int temp = positions[i];
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }
    }

    /// Convierte una posición de cuadrícula a posición mundial.
    Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        float worldX = gridPos.x + 0.5f;
        float worldY = itemHeightOffset;
        float worldZ = gridPos.y + 0.5f + itemZOffset;

        return new Vector3(worldX, worldY, worldZ);
    }
}