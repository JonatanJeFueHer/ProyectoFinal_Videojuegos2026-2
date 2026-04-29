using TMPro;
using UnityEngine;

public class CardPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SimpleTurnManager turnManager;
    [SerializeField] private CardItemUI cardItemPrefab;
    [SerializeField] private Transform contentParent;

    [Header("Options")]
    [SerializeField] private bool showOnlyNumericCards = true;
    [SerializeField] private bool clearOnStart = true;

    private void Awake()
    {
        if (turnManager == null)
        {
            turnManager = FindObjectOfType<SimpleTurnManager>();
        }

        if (contentParent == null)
        {
            contentParent = transform;
        }
    }

    private void OnEnable()
    {
        if (turnManager != null)
        {
            turnManager.OnCardChosen += HandleCardChosen;
        }
    }

    private void OnDisable()
    {
        if (turnManager != null)
        {
            turnManager.OnCardChosen -= HandleCardChosen;
        }
    }

    private void Start()
    {
        if (clearOnStart)
        {
            ClearPanel();
        }
    }

    public void ClearPanel()
    {
        if (contentParent == null)
        {
            return;
        }

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }

    private void HandleCardChosen(string cardValue)
    {
        if (showOnlyNumericCards && !IsNumeric(cardValue))
        {
            return;
        }

        if (cardItemPrefab == null)
        {
            Debug.LogWarning("CardPanelUI: falta asignar cardItemPrefab.");
            return;
        }

        Transform parent = contentParent != null ? contentParent : transform;
        CardItemUI item = Instantiate(cardItemPrefab, parent);
        item.SetCardValue(cardValue);

        
        TMP_Text fallbackText = item.GetComponentInChildren<TMP_Text>();
        if (fallbackText != null)
        {
            fallbackText.text = cardValue;
        }
    }

    private bool IsNumeric(string value)
    {
        int number;
        return int.TryParse(value, out number);
    }
}
