using UnityEngine;

public class RoundIconsPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SimpleTurnManager turnManager;
    [SerializeField] private SimpleGameStateManager gameStateManager;
    [SerializeField] private HudCounterPanelUI roundsPanel;

    [Header("Settings")]
    [SerializeField] private int fallbackMaxRounds = 18;

    private int maxRounds;

    private void Awake()
    {
        if (turnManager == null)
        {
            turnManager = FindObjectOfType<SimpleTurnManager>();
        }

        if (gameStateManager == null)
        {
            gameStateManager = FindObjectOfType<SimpleGameStateManager>();
        }

        maxRounds = gameStateManager != null ? gameStateManager.MaxTurns : fallbackMaxRounds;
    }

    private void OnEnable()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnStarted += HandleTurnStarted;
        }
    }

    private void OnDisable()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnStarted -= HandleTurnStarted;
        }
    }

    private void Start()
    {
        if (roundsPanel == null)
        {
            return;
        }

        roundsPanel.BuildSlots(maxRounds);

        int currentTurn = turnManager != null ? turnManager.CurrentTurn : 1;
        roundsPanel.SetLitCount(Mathf.Clamp(currentTurn, 0, maxRounds));
    }

    private void HandleTurnStarted(int turnNumber)
    {
        if (roundsPanel == null)
        {
            return;
        }

        int litCount = Mathf.Clamp(turnNumber, 0, maxRounds);
        roundsPanel.SetLitCount(litCount);
    }
}
