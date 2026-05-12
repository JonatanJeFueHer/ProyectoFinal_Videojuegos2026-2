using TMPro;
using UnityEngine;

public class GamePanelsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SimpleTurnManager turnManager;
    [SerializeField] private SimpleGameStateManager gameStateManager;
    [SerializeField] private SoundManager soundManager;

    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject endPanel;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text endTitleText;
    [SerializeField] private TMP_Text endMessageText;
    [SerializeField] private GameObject pauseButtonObject;

    private bool gameStarted;
    private bool isPaused;
    private bool gameEnded;

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

        if (soundManager == null)
        {
            soundManager = SoundManager.Instance;
        }
    }

    private void OnEnable()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnGameEnded += HandleGameEnded;
        }
    }

    private void OnDisable()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnGameEnded -= HandleGameEnded;
        }

        Time.timeScale = 1f;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        gameStarted = false;
        isPaused = false;
        gameEnded = false;

        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }

        if (pauseButtonObject != null)
        {
            pauseButtonObject.SetActive(false);
        }

        if (turnManager != null)
        {
            turnManager.SetGameLocked(true);
        }
    }

    public void StartGame()
    {
        if (gameStarted)
        {
            return;
        }

        gameStarted = true;
        isPaused = false;
        gameEnded = false;

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }

        if (pauseButtonObject != null)
        {
            pauseButtonObject.SetActive(true);
        }

        Time.timeScale = 1f;
        soundManager?.SetMusicPaused(false);

        if (turnManager != null)
        {
            turnManager.SetGameLocked(false);
        }
    }

    public void TogglePause()
    {
        if (!gameStarted || gameEnded)
        {
            return;
        }

        if (isPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    private void PauseGame()
    {
        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;
        soundManager?.SetMusicPaused(true);

        if (turnManager != null)
        {
            turnManager.SetGameLocked(true);
        }
    }

    private void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
        soundManager?.SetMusicPaused(false);

        if (turnManager != null)
        {
            turnManager.SetGameLocked(false);
        }
    }

    private void HandleGameEnded(bool isVictory, string message)
    {
        gameEnded = true;
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }

        if (pauseButtonObject != null)
        {
            pauseButtonObject.SetActive(false);
        }

        if (endTitleText != null)
        {
            endTitleText.text = isVictory ? "Victoria" : "Derrota";
        }

        if (endMessageText != null)
        {
            endMessageText.text = message;
        }

        Time.timeScale = 0f;

        if (turnManager != null)
        {
            turnManager.SetGameLocked(true);
        }
    }
}
