using TMPro;
using UnityEngine;

public class CardItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text cardValueText;

    public void SetCardValue(string cardValue)
    {
        if (cardValueText == null)
        {
            return;
        }

        cardValueText.text = cardValue;
    }
}
