using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimpleGameStateManager : MonoBehaviour
{
    public event System.Action<bool, string> OnGameEnded;
    public event System.Action<int, int> OnLivesChanged;
    public event System.Action<int, int> OnKeysChanged;

    [Header("References")]
    [SerializeField] private SimpleTurnManager turnManager;
    [SerializeField] private SimpleGridMapGenerator mapGenerator;
    [SerializeField] private SimpleGridTurnMovement gridMovement;

    [Header("UI")]
    [SerializeField] private TMP_Text diceResultText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text keysText;
    [SerializeField] private TMP_Text turnLimitText;
    [SerializeField] private TMP_Text handText;
    [SerializeField] private TMP_Text statusText;

    [Header("Game Rules")]
    [SerializeField] private int initialLives = 3;
    [SerializeField] private int keysToWin = 2;
    [SerializeField] private int maxTurns = 18;

    [Header("Fallback Tile Codes")]
    [SerializeField] private List<int> fallbackTileCodes = new List<int> { 6, 2, 3, 7, 1, 6, 4, 8, 6, 5, 9 };

    private readonly List<int> numericHand = new List<int>();

    private int currentLives;
    private int keysFound;
    private int fallbackTileIndex;
    private bool gameEnded;
    private bool movementSpentThisTurn;

    public int CurrentLives => currentLives;
    public int InitialLives => initialLives;
    public int KeysFound => keysFound;
    public int KeysToWin => keysToWin;
    public int MaxTurns => maxTurns;

    private void Awake()
    {
        if (turnManager == null)
        {
            turnManager = GetComponent<SimpleTurnManager>();
        }

        if (mapGenerator == null)
        {
            mapGenerator = FindObjectOfType<SimpleGridMapGenerator>();
        }

        if (gridMovement == null)
        {
            gridMovement = FindObjectOfType<SimpleGridTurnMovement>();
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
        turnManager.OnCardChosen += HandleCardChosen;
        turnManager.OnMovementSpent += HandleMovementSpent;
        turnManager.OnMovementPhaseEnded += HandleMovementPhaseEnded;
    }

    private void OnDisable()
    {
        if (turnManager == null)
        {
            return;
        }

        turnManager.OnTurnStarted -= HandleTurnStarted;
        turnManager.OnDiceRolled -= HandleDiceRolled;
        turnManager.OnCardChosen -= HandleCardChosen;
        turnManager.OnMovementSpent -= HandleMovementSpent;
        turnManager.OnMovementPhaseEnded -= HandleMovementPhaseEnded;
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        currentLives = initialLives;
        keysFound = 0;
        fallbackTileIndex = 0;
        gameEnded = false;
        movementSpentThisTurn = false;
        numericHand.Clear();

        UpdateLivesUI();
        UpdateKeysUI();
        UpdateHandUI();
        UpdateDiceResultUI(-1);

        int currentTurn = turnManager != null ? turnManager.CurrentTurn : 1;
        UpdateTurnLimitUI(currentTurn);

        SetStatus("Juego iniciado. Lanza el dado para comenzar.", false);
    }

    private void HandleTurnStarted(int turnNumber)
    {
        if (gameEnded)
        {
            return;
        }

        movementSpentThisTurn = false;
        UpdateTurnLimitUI(turnNumber);

        if (turnNumber > maxTurns && keysFound < keysToWin)
        {
            EndGame("Derrota: se alcanzo el limite de turnos sin conseguir todas las llaves.");
            return;
        }

        SetStatus($"Inicia turno {turnNumber}", false);
    }

    private void HandleDiceRolled(int diceResult)
    {
        if (gameEnded)
        {
            return;
        }

        UpdateDiceResultUI(diceResult);

        if (diceResult == 0)
        {
            SetStatus("Turno sin movimientos por efecto de Stop.", false);
            return;
        }

        SetStatus($"Resultado del dado: {diceResult}", false);
    }

    private void HandleCardChosen(string card)
    {
        if (gameEnded)
        {
            return;
        }

        int numericValue;
        if (int.TryParse(card, out numericValue))
        {
            numericHand.Add(numericValue);
            UpdateHandUI();
            SetStatus($"Carta numerica obtenida: {card}");
            return;
        }

        ApplySpecialCard(card);
    }

    private void HandleMovementSpent(string key, int remaining)
    {
        if (gameEnded)
        {
            return;
        }

        movementSpentThisTurn = true;
    }

    private void HandleMovementPhaseEnded()
    {
        if (gameEnded)
        {
            return;
        }

        if (!movementSpentThisTurn)
        {
            SetStatus("Turno sin desplazamiento. No se resolvio casilla.", false);
            return;
        }

        if (mapGenerator != null && gridMovement != null)
        {
            Vector2Int gridPosition = gridMovement.CurrentGridPosition;
            int mapCode = mapGenerator.GetTileCode(gridPosition);
            ResolveTileFromCode(mapCode, gridPosition, true);
            return;
        }

        ResolveFallbackTile();
    }

    private void ResolveFallbackTile()
    {
        if (fallbackTileCodes == null || fallbackTileCodes.Count == 0)
        {
            SetStatus("No hay mapa ni lista fallback de casillas.", false);
            return;
        }

        int code = fallbackTileCodes[fallbackTileIndex];
        Vector2Int fallbackPosition = new Vector2Int(fallbackTileIndex, 0);
        ResolveTileFromCode(code, fallbackPosition, false);
        fallbackTileIndex = (fallbackTileIndex + 1) % fallbackTileCodes.Count;
    }

    private void ResolveTileFromCode(int code, Vector2Int gridPosition, bool fromMap)
    {
        switch (code)
        {
            case 0:
                SetStatus($"Casilla ({gridPosition.x},{gridPosition.y}) sin tile.");
                break;

            case 1:
                AddKey($"Casilla ({gridPosition.x},{gridPosition.y}): llave encontrada.");
                if (fromMap && mapGenerator != null)
                {
                    mapGenerator.SetTileCode(gridPosition, 6);
                }
                break;

            case 2:
                ChangeLives(-1, $"Casilla ({gridPosition.x},{gridPosition.y}): trampa Rayo. Pierdes 1 vida.");
                break;

            case 3:
                ResolveChestWithoutKey(gridPosition, "Azul", 10, fromMap);
                break;

            case 4:
                ResolveChestWithoutKey(gridPosition, "Naranja", 15, fromMap);
                break;

            case 5:
                ResolveChestWithoutKey(gridPosition, "Rosa", 20, fromMap);
                break;

            case 7:
                ResolveChestWithKey(gridPosition, "Azul con llave", 10, fromMap);
                break;

            case 8:
                ResolveChestWithKey(gridPosition, "Naranja con llave", 15, fromMap);
                break;

            case 9:
                ResolveChestWithKey(gridPosition, "Rosa con llave", 20, fromMap);
                break;

            case 6:
                SetStatus($"Casilla ({gridPosition.x},{gridPosition.y}): vacia.");
                break;

            default:
                SetStatus($"Casilla ({gridPosition.x},{gridPosition.y}) con codigo desconocido: {code}.");
                break;
        }
    }

    private bool TryOpenChest(Vector2Int gridPosition, string chestName, int cost, bool fromMap)
    {
        List<int> usedCards;
        int usedTotal;
        if (!TryConsumeNumericCards(cost, out usedCards, out usedTotal))
        {
            int availableTotal = GetHandTotal();
            SetStatus($"Cofre {chestName} en ({gridPosition.x},{gridPosition.y}) costo {cost}. No alcanza. Total disponible: {availableTotal}.");
            return false;
        }

        UpdateHandUI();

        string cardsText = usedCards.Count > 0 ? string.Join(", ", usedCards) : "ninguna";
        SetStatus($"Cofre {chestName} abierto en ({gridPosition.x},{gridPosition.y}) usando {cardsText} (total {usedTotal}).");

        if (fromMap && mapGenerator != null)
        {
            mapGenerator.SetTileCode(gridPosition, 6);
        }

        return true;
    }

    private void ResolveChestWithoutKey(Vector2Int gridPosition, string chestName, int cost, bool fromMap)
    {
        bool wasOpened = TryOpenChest(gridPosition, chestName, cost, fromMap);
        if (!wasOpened)
        {
            return;
        }

        SetStatus($"El cofre {chestName} no contenia llave.");
    }

    private void ResolveChestWithKey(Vector2Int gridPosition, string chestName, int cost, bool fromMap)
    {
        bool wasOpened = TryOpenChest(gridPosition, chestName, cost, fromMap);
        if (!wasOpened)
        {
            return;
        }

        AddKey($"El cofre {chestName} contenia una llave.");
    }

    private bool TryConsumeNumericCards(int cost, out List<int> usedCards, out int usedTotal)
    {
        usedCards = new List<int>();
        usedTotal = 0;

        if (numericHand.Count == 0)
        {
            return false;
        }

        List<int> sortedCards = new List<int>(numericHand);
        sortedCards.Sort((a, b) => b.CompareTo(a));

        for (int i = 0; i < sortedCards.Count; i++)
        {
            int value = sortedCards[i];
            usedCards.Add(value);
            usedTotal += value;

            if (usedTotal >= cost)
            {
                break;
            }
        }

        if (usedTotal < cost)
        {
            usedCards.Clear();
            usedTotal = 0;
            return false;
        }

        for (int i = 0; i < usedCards.Count; i++)
        {
            numericHand.Remove(usedCards[i]);
        }

        return true;
    }

    private void ApplySpecialCard(string card)
    {
        switch (card)
        {
            case "Tristeza":
                ChangeLives(-1, "Carta rara Tristeza: pierdes 1 vida.");
                break;

            case "Retorno":
                SetStatus("Carta rara Retorno: efecto registrado. El retroceso queda pendiente para una version avanzada.");
                break;

            case "Stop":
                if (turnManager != null)
                {
                    turnManager.BlockNextTurnMovements();
                }
                SetStatus("Carta rara Stop: en el siguiente turno no podras gastar movimientos.");
                break;

            default:
                SetStatus($"Carta rara no reconocida: {card}");
                break;
        }
    }

    private int GetHandTotal()
    {
        int total = 0;
        for (int i = 0; i < numericHand.Count; i++)
        {
            total += numericHand[i];
        }

        return total;
    }

    private void AddKey(string message)
    {
        keysFound++;
        UpdateKeysUI();
        SetStatus(message);

        if (keysFound >= keysToWin)
        {
            EndGame("Victoria: conseguiste todas las llaves.");
        }
    }

    private void ChangeLives(int delta, string message)
    {
        currentLives += delta;
        if (currentLives < 0)
        {
            currentLives = 0;
        }

        UpdateLivesUI();
        SetStatus(message);

        if (currentLives <= 0)
        {
            EndGame("Derrota: te quedaste sin vidas.");
        }
    }

    private void EndGame(string message)
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        bool isVictory = keysFound >= keysToWin;

        if (turnManager != null)
        {
            turnManager.SetGameLocked(true);
        }

        SetStatus(message);
        OnGameEnded?.Invoke(isVictory, message);
    }

    private void UpdateDiceResultUI(int value)
    {
        if (diceResultText == null)
        {
            return;
        }

        if (value < 0)
        {
            diceResultText.text = "Dado: -";
            return;
        }

        diceResultText.text = $"Dado: {value}";
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"Vidas: {currentLives}";
        }

        OnLivesChanged?.Invoke(currentLives, initialLives);
    }

    private void UpdateKeysUI()
    {
        if (keysText != null)
        {
            keysText.text = $"Llaves: {keysFound}/{keysToWin}";
        }

        OnKeysChanged?.Invoke(keysFound, keysToWin);
    }

    private void UpdateTurnLimitUI(int currentTurn)
    {
        if (turnLimitText == null)
        {
            return;
        }

        int remainingTurns = Mathf.Max(0, maxTurns - currentTurn + 1);
        turnLimitText.text = $"Turnos: {currentTurn}/{maxTurns} (Restantes: {remainingTurns})";
    }

    private void UpdateHandUI()
    {
        if (handText == null)
        {
            return;
        }

        if (numericHand.Count == 0)
        {
            handText.text = "Cartas: (vacia)";
            return;
        }

        handText.text = $"Cartas: {string.Join(", ", numericHand)}";
    }

    private void SetStatus(string message, bool logToConsole = true)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        if (logToConsole)
        {
            Debug.Log(message);
        }
    }
}
