using UnityEngine;

public class StatsIconsHudController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SimpleGameStateManager gameStateManager;
    [SerializeField] private HudCounterPanelUI livesPanel;
    [SerializeField] private HudCounterPanelUI keysPanel;

    private void Awake()
    {
        if (gameStateManager == null)
        {
            gameStateManager = FindObjectOfType<SimpleGameStateManager>();
        }
    }

    private void OnEnable()
    {
        if (gameStateManager == null)
        {
            return;
        }

        gameStateManager.OnLivesChanged += HandleLivesChanged;
        gameStateManager.OnKeysChanged += HandleKeysChanged;
    }

    private void OnDisable()
    {
        if (gameStateManager == null)
        {
            return;
        }

        gameStateManager.OnLivesChanged -= HandleLivesChanged;
        gameStateManager.OnKeysChanged -= HandleKeysChanged;
    }

    private void Start()
    {
        if (gameStateManager == null)
        {
            return;
        }

        if (livesPanel != null)
        {
            livesPanel.BuildSlots(gameStateManager.InitialLives);
            livesPanel.SetLitCount(gameStateManager.CurrentLives);
        }

        if (keysPanel != null)
        {
            keysPanel.BuildSlots(gameStateManager.KeysToWin);
            keysPanel.SetLitCount(gameStateManager.KeysFound);
        }
    }

    private void HandleLivesChanged(int currentLives, int maxLives)
    {
        if (livesPanel == null)
        {
            return;
        }

        if (livesPanel.SlotCount != maxLives)
        {
            livesPanel.BuildSlots(maxLives);
        }

        livesPanel.SetLitCount(currentLives);
    }

    private void HandleKeysChanged(int currentKeys, int maxKeys)
    {
        if (keysPanel == null)
        {
            return;
        }

        if (keysPanel.SlotCount != maxKeys)
        {
            keysPanel.BuildSlots(maxKeys);
        }

        keysPanel.SetLitCount(currentKeys);
    }
}
