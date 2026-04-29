using UnityEngine;

public class SimpleGridTurnMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SimpleTurnManager turnManager;
    [SerializeField] private SimpleGridMapGenerator mapGenerator;
    [SerializeField] private Transform playerTransform;

    [Header("Movement")]
    [SerializeField] private float playerHeightOffset = 1f;

    private Vector2Int currentGridPosition;

    public Vector2Int CurrentGridPosition => currentGridPosition;

    private void Awake()
    {
        if (turnManager == null)
        {
            turnManager = FindObjectOfType<SimpleTurnManager>();
        }

        if (mapGenerator == null)
        {
            mapGenerator = FindObjectOfType<SimpleGridMapGenerator>();
        }

        if (turnManager != null)
        {
            turnManager.SetUseExternalMovementInput(true);
        }
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        if (turnManager != null)
        {
            turnManager.SetUseExternalMovementInput(true);
        }

        currentGridPosition = Vector2Int.zero;

        if (mapGenerator != null)
        {
            mapGenerator.GenerateMap();
            mapGenerator.PlacePlayerAtStart();

            Vector3 startPosition;
            if (mapGenerator.TryGetWorldPosition(Vector2Int.zero, out startPosition))
            {
                playerTransform.position = startPosition + Vector3.up * playerHeightOffset;
            }
        }
    }

    private void Update()
    {
        if (turnManager == null || mapGenerator == null || playerTransform == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            TryMove(Vector2Int.up, "W");
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            TryMove(Vector2Int.right, "A");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            TryMove(Vector2Int.down, "S");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            TryMove(Vector2Int.left, "D");
        }
    }

    private void TryMove(Vector2Int direction, string keyName)
    {
        Vector2Int nextGridPosition = currentGridPosition + direction;

        if (!mapGenerator.IsWalkable(nextGridPosition))
        {
            Debug.Log($"Movimiento bloqueado hacia {nextGridPosition}. No hay casilla disponible.");
            return;
        }

        bool movementConsumed = turnManager.TryConsumeMovementExternal(keyName);
        if (!movementConsumed)
        {
            return;
        }

        Vector3 nextWorldPosition;
        if (!mapGenerator.TryGetWorldPosition(nextGridPosition, out nextWorldPosition))
        {
            turnManager.CompleteExternalMovementStep();
            return;
        }

        currentGridPosition = nextGridPosition;
        playerTransform.position = nextWorldPosition + Vector3.up * playerHeightOffset;
        Debug.Log($"Jugador movido a casilla {currentGridPosition.x},{currentGridPosition.y}");
        turnManager.CompleteExternalMovementStep();
    }
}
