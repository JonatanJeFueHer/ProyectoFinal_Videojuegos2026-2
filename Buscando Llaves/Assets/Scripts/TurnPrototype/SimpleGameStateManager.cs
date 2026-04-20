using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimpleGameStateManager : MonoBehaviour
{
    public enum TileType
    {
        Empty,
        Chest,
        TrapRayo,
        HiddenKey
    }

    public enum ChestType
    {
        Blue,
        Orange,
        Pink
    }

    [System.Serializable]
    public class TileEntry
    {
        public TileType tileType = TileType.Empty;
        public ChestType chestType = ChestType.Blue;
        [Range(0f, 1f)] public float keyChance = 0.5f;
    }

    [Header("References")]
    [SerializeField] private SimpleTurnManager turnManager;

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

    [Header("Tile Test Sequence")]
    [SerializeField] private List<TileEntry> testTiles = new List<TileEntry>();

    private readonly List<int> numericHand = new List<int>();

    private int currentLives;
    private int keysFound;
    private int currentTileIndex;
    private bool gameEnded;

 

    private void OnEnable()
    {
        if (turnManager == null)
        {
            return;
        }

        turnManager.OnTurnStarted += HandleTurnStarted;
        turnManager.OnDiceRolled += HandleDiceRolled;
        turnManager.OnCardChosen += HandleCardChosen;
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
        currentTileIndex = 0;
        gameEnded = false;
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

        UpdateTurnLimitUI(turnNumber);

        if (turnNumber > maxTurns && keysFound < keysToWin)
        {
            EndGame(false, "Derrota: se alcanzo el limite de turnos sin conseguir todas las llaves.");
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

    private void HandleMovementPhaseEnded()
    {
        if (gameEnded)
        {
            return;
        }

        ResolveCurrentTile();
    }

    private void ApplySpecialCard(string card)
    {
        switch (card)
        {
            case "Tristeza":
                ChangeLives(-1, "Carta Tristeza: pierdes 1 vida.");
                break;

            case "Retorno":
                SetStatus("Carta Retorno: efecto registrado. El retroceso queda pendiente hasta integrar movimiento real.");
                break;

            case "Stop":
                if (turnManager != null)
                {
                    turnManager.BlockNextTurnMovements();
                }
                SetStatus("Carta Stop: en el siguiente turno no podras gastar movimientos.");
                break;

            default:
                SetStatus($"Carta especial no reconocida: {card}");
                break;
        }
    }

    private void ResolveCurrentTile()
    {
        if (testTiles == null || testTiles.Count == 0)
        {
            SetStatus("No hay casillas de prueba configuradas. Se toma casilla vacia.", false);
            return;
        }

        TileEntry tile = testTiles[currentTileIndex];
        int tileNumber = currentTileIndex + 1;

        switch (tile.tileType)
        {
            case TileType.Empty:
                SetStatus($"Casilla {tileNumber}: vacia.");
                break;

            case TileType.TrapRayo:
                ChangeLives(-1, $"Casilla {tileNumber}: trampa Rayo. Pierdes 1 vida.");
                break;

            case TileType.HiddenKey:
                AddKey($"Casilla {tileNumber}: encontraste una llave oculta.");
                break;

            case TileType.Chest:
                ResolveChest(tileNumber, tile.chestType, tile.keyChance);
                break;
        }

        currentTileIndex = (currentTileIndex + 1) % testTiles.Count;
    }

    private void ResolveChest(int tileNumber, ChestType chestType, float keyChance)
    {
        int cost = GetChestCost(chestType);

        List<int> usedCards;
        int usedTotal;
        if (!TryConsumeNumericCards(cost, out usedCards, out usedTotal))
        {
            int availableTotal = GetHandTotal();
            SetStatus($"Casilla {tileNumber}: cofre {ChestName(chestType)} (costo {cost}) no se puede abrir. Total disponible: {availableTotal}.");
            return;
        }

        UpdateHandUI();

        string cardsText = usedCards.Count > 0 ? string.Join(", ", usedCards) : "ninguna";
        SetStatus($"Casilla {tileNumber}: cofre {ChestName(chestType)} abierto usando cartas {cardsText} (total {usedTotal}).");

        bool containsKey = Random.value <= keyChance;
        if (containsKey)
        {
            AddKey($"El cofre {ChestName(chestType)} contenia una llave.");
        }
        else
        {
            SetStatus($"El cofre {ChestName(chestType)} no contenia llave.");
        }
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

    private int GetChestCost(ChestType chestType)
    {
        switch (chestType)
        {
            case ChestType.Blue:
                return 10;
            case ChestType.Orange:
                return 15;
            case ChestType.Pink:
                return 20;
            default:
                return 10;
        }
    }

    private string ChestName(ChestType chestType)
    {
        switch (chestType)
        {
            case ChestType.Blue:
                return "Azul";
            case ChestType.Orange:
                return "Naranja";
            case ChestType.Pink:
                return "Rosa";
            default:
                return "Desconocido";
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
            EndGame(true, "Victoria: conseguiste todas las llaves.");
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
            EndGame(false, "Derrota: te quedaste sin vidas.");
        }
    }

    private void EndGame(bool isVictory, string message)
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;

        if (turnManager != null)
        {
            turnManager.SetGameLocked(true);
        }

        if (isVictory)
        {
            SetStatus(message);
            return;
        }

        SetStatus(message);
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
    }

    private void UpdateKeysUI()
    {
        if (keysText != null)
        {
            keysText.text = $"Llaves: {keysFound}/{keysToWin}";
        }
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
