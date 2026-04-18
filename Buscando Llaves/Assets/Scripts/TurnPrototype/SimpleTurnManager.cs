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

    private void Start()
    {
        UpdateUI();
        UpdateButtons();
        Debug.Log($"Inicia turno {currentTurn}");
    }

    private void Update()
    {
        if (waitingForDice || waitingForCard || movementsRemaining <= 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            ConsumeMovement("W");
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            ConsumeMovement("A");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ConsumeMovement("S");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ConsumeMovement("D");
        }
    }

    public void RollDice()
    {
        if (!waitingForDice)
        {
            return;
        }

        int diceResult = Random.Range(1, 7);
        movementsRemaining = diceResult;
        waitingForDice = false;
        waitingForCard = false;

        Debug.Log($"Dado lanzado: {diceResult}");

        UpdateUI();
        UpdateButtons();
    }

    public void ChooseCard()
    {
        if (!waitingForCard)
        {
            return;
        }

        string card = availableCards[Random.Range(0, availableCards.Count)];
        playerCards.Add(card);

        Debug.Log($"Carta obtenida: {card}");
        Debug.Log($"Cartas del jugador: {string.Join(", ", playerCards)}");
        Debug.Log("Turno terminado");

        currentTurn++;
        movementsRemaining = 0;
        waitingForDice = true;
        waitingForCard = false;

        UpdateUI();
        UpdateButtons();

        Debug.Log($"Inicia turno {currentTurn}");
    }

    private void ConsumeMovement(string key)
    {
        if (movementsRemaining <= 0)
        {
            return;
        }

        movementsRemaining--;

        Debug.Log($"Tecla {key} presionada");
        Debug.Log($"Movimientos restantes: {movementsRemaining}");

        UpdateUI();

        if (movementsRemaining == 0)
        {
            waitingForCard = true;
            UpdateButtons();
            Debug.Log("Ya no quedan movimientos. Puedes elegir una carta.");
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
            rollDiceButton.interactable = waitingForDice;
        }

        if (chooseCardButton != null)
        {
            chooseCardButton.interactable = waitingForCard;
        }
    }
}
