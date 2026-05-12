using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleTurnManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button rollDiceButton;
    [SerializeField] private Button chooseCardButton;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text movementsText;

    [Header("Input")]
    [SerializeField] private bool useExternalMovementInput = false;

    private readonly List<string> availableCards = new List<string>
    {
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "Tristeza", "Retorno", "Stop"
    };

    private readonly List<string> playerCards = new List<string>();

    private int currentTurn = 1;
    private int movementsRemaining = 0;
    private bool waitingForDice = true;
    private bool waitingForCard = false;
    private bool blockNextTurnMovements = false;
    private bool gameLocked = false;

    public event Action<int> OnTurnStarted;
    public event Action<int> OnDiceRolled;
    public event Action<string, int> OnMovementSpent;
    public event Action OnMovementPhaseEnded;
    public event Action<string> OnCardChosen;
    public event Action<int> OnTurnEnded;

    public int CurrentTurn => currentTurn;
    public int MovementsRemaining => movementsRemaining;

    private void Start()
    {
        AutoConfigureMovementInput();
        UpdateUI();
        UpdateButtons();
        Debug.Log($"Inicia turno {currentTurn}");
        OnTurnStarted?.Invoke(currentTurn);
    }

    private void Update()
    {
        if (useExternalMovementInput || gameLocked || waitingForDice || waitingForCard || movementsRemaining <= 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            TryConsumeMovement("W");
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            TryConsumeMovement("A");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            TryConsumeMovement("S");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            TryConsumeMovement("D");
        }
    }

    public void RollDice()
    {
        if (gameLocked || !waitingForDice)
        {
            return;
        }

        if (blockNextTurnMovements)
        {
            blockNextTurnMovements = false;
            movementsRemaining = 0;
            waitingForDice = false;
            waitingForCard = true;

            Debug.Log("Turno bloqueado por Stop. No puedes gastar movimientos en este turno.");
            Debug.Log("Dado lanzado: 0");

            UpdateUI();
            UpdateButtons();

            OnDiceRolled?.Invoke(0);
            OnMovementPhaseEnded?.Invoke();
            return;
        }

        int diceResult = UnityEngine.Random.Range(1, 7);
        SoundManager.Instance?.PlayDiceRoll();
        movementsRemaining = diceResult;
        waitingForDice = false;
        waitingForCard = false;

        Debug.Log($"Dado lanzado: {diceResult}");

        UpdateUI();
        UpdateButtons();

        OnDiceRolled?.Invoke(diceResult);
    }

    public void ChooseCard()
    {
        if (gameLocked || !waitingForCard)
        {
            return;
        }

        string card = availableCards[UnityEngine.Random.Range(0, availableCards.Count)];
        if (IsNumericCard(card))
        {
            playerCards.Add(card);
        }

        Debug.Log($"Carta obtenida: {card}");
        Debug.Log($"Cartas del jugador: {string.Join(", ", playerCards)}");
        OnCardChosen?.Invoke(card);

        Debug.Log("Turno terminado");
        OnTurnEnded?.Invoke(currentTurn);

        currentTurn++;
        movementsRemaining = 0;
        waitingForDice = true;
        waitingForCard = false;

        UpdateUI();
        UpdateButtons();

        Debug.Log($"Inicia turno {currentTurn}");
        OnTurnStarted?.Invoke(currentTurn);
    }

    public bool TryConsumeMovement(string key)
    {
        return TryConsumeMovementInternal(key, true);
    }

    public bool TryConsumeMovementExternal(string key)
    {
        return TryConsumeMovementInternal(key, false);
    }

    public void CompleteExternalMovementStep()
    {
        if (gameLocked || waitingForDice || waitingForCard)
        {
            return;
        }

        if (movementsRemaining == 0)
        {
            FinishMovementPhase();
        }
    }

    private bool TryConsumeMovementInternal(string key, bool finishPhaseImmediately)
    {
        if (gameLocked || waitingForDice || waitingForCard || movementsRemaining <= 0)
        {
            return false;
        }

        movementsRemaining--;

        Debug.Log($"Tecla {key} presionada");
        Debug.Log($"Movimientos restantes: {movementsRemaining}");
        OnMovementSpent?.Invoke(key, movementsRemaining);

        UpdateUI();

        if (movementsRemaining == 0 && finishPhaseImmediately)
        {
            FinishMovementPhase();
        }

        return true;
    }

    private void FinishMovementPhase()
    {
        waitingForCard = true;
        UpdateButtons();
        Debug.Log("Ya no quedan movimientos. Puedes elegir una carta.");
        OnMovementPhaseEnded?.Invoke();
    }

    public void SetGameLocked(bool value)
    {
        gameLocked = value;
        UpdateButtons();
    }

    public void BlockNextTurnMovements()
    {
        blockNextTurnMovements = true;
    }

    public void SetUseExternalMovementInput(bool value)
    {
        useExternalMovementInput = value;
    }

    private void AutoConfigureMovementInput()
    {
        if (FindObjectOfType<SimpleGridTurnMovement>() != null)
        {
            useExternalMovementInput = true;
        }
    }

    private void UpdateUI()
    {
        if (turnText != null)
        {
            turnText.text = $"Turno: {currentTurn}";
        }

        if (movementsText != null)
        {
            movementsText.text = $"Movimientos: {movementsRemaining}";
        }
    }

    private void UpdateButtons()
    {
        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = waitingForDice && !gameLocked;
        }

        if (chooseCardButton != null)
        {
            chooseCardButton.interactable = waitingForCard && !gameLocked;
        }
    }

    private bool IsNumericCard(string card)
    {
        int number;
        return int.TryParse(card, out number);
    }
}
