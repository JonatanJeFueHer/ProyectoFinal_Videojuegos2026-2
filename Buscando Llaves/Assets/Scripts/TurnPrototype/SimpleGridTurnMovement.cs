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
    private SoundManager soundManager;

    public Vector2Int CurrentGridPosition => currentGridPosition;

    public void ForceGridPosition(Vector2Int gridPosition)
    {
        currentGridPosition = gridPosition;
    }

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

        soundManager = SoundManager.Instance;
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

            currentGridPosition = mapGenerator.StartGridPosition;
            Vector3 startPosition;
            if (mapGenerator.TryGetWorldPosition(currentGridPosition, out startPosition))
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
            if (soundManager == null)
            {
                soundManager = SoundManager.Instance;
            }

            soundManager?.PlayMoveDenied();
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
        if (soundManager == null)
        {
            soundManager = SoundManager.Instance;
        }

        soundManager?.PlayFootstep();
        turnManager.CompleteExternalMovementStep();
    }
}
