using UnityEngine;

public class MovementIconsPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SimpleTurnManager turnManager;
    [SerializeField] private HudCounterPanelUI movementPanel;

    [Header("Settings")]
    [SerializeField] private int maxMovementSlots = 6;

    private void Awake()
    {
        if (turnManager == null)
        {
            turnManager = FindObjectOfType<SimpleTurnManager>();
        }
    }

    private void OnEnable()
    {
        if (turnManager == null)
        {
            return;
        }

        turnManager.OnTurnStarted += HandleTurnStarted;
        turnManager.OnDiceRolled += HandleDiceRolled;
        turnManager.OnMovementSpent += HandleMovementSpent;
    }

    private void OnDisable()
    {
        if (turnManager == null)
        {
            return;
        }

        turnManager.OnTurnStarted -= HandleTurnStarted;
        turnManager.OnDiceRolled -= HandleDiceRolled;
        turnManager.OnMovementSpent -= HandleMovementSpent;
    }

    private void Start()
    {
        if (movementPanel == null)
        {
            return;
        }

        movementPanel.BuildSlots(maxMovementSlots);
        movementPanel.SetLitCount(0);
    }

    private void HandleTurnStarted(int turnNumber)
    {
        if (movementPanel == null)
        {
            return;
        }

        movementPanel.SetLitCount(0);
    }

    private void HandleDiceRolled(int diceResult)
    {
        if (movementPanel == null)
        {
            return;
        }

        int litCount = Mathf.Clamp(diceResult, 0, maxMovementSlots);
        movementPanel.SetLitCount(litCount);
    }

    private void HandleMovementSpent(string key, int movementsRemaining)
    {
        if (movementPanel == null)
        {
            return;
        }

        int litCount = Mathf.Clamp(movementsRemaining, 0, maxMovementSlots);
        movementPanel.SetLitCount(litCount);
    }
}
