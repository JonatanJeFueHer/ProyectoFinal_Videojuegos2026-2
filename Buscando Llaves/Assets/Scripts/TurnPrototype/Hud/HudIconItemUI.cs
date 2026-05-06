using UnityEngine;
using UnityEngine.UI;

public class HudIconItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public void SetLit(bool isLit, float litAlpha, float dimAlpha)
    {
        if (iconImage == null)
        {
            return;
        }

        Color color = iconImage.color;
        color.a = isLit ? litAlpha : dimAlpha;
        iconImage.color = color;
    }

    public void SetSprite(Sprite sprite)
    {
        if (iconImage == null)
        {
            return;
        }

        iconImage.sprite = sprite;
    }
}
