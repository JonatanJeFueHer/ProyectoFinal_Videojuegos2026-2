using System.Collections.Generic;
using UnityEngine;

public class HudCounterPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HudIconItemUI iconPrefab;
    [SerializeField] private Transform contentParent;

    [Header("Visual")]
    [SerializeField] private float litAlpha = 1f;
    [SerializeField] private float dimAlpha = 0.25f;

    private readonly List<HudIconItemUI> icons = new List<HudIconItemUI>();

    public int SlotCount => icons.Count;

    private void Awake()
    {
        if (contentParent == null)
        {
            contentParent = transform;
        }
    }

    public void BuildSlots(int slotCount)
    {
        ClearSlots();

        if (iconPrefab == null || contentParent == null)
        {
            return;
        }

        for (int i = 0; i < slotCount; i++)
        {
            HudIconItemUI icon = Instantiate(iconPrefab, contentParent);
            icon.SetLit(false, litAlpha, dimAlpha);
            icons.Add(icon);
        }
    }

    public void SetLitCount(int litCount)
    {
        int clamped = Mathf.Clamp(litCount, 0, icons.Count);
        for (int i = 0; i < icons.Count; i++)
        {
            bool isLit = i < clamped;
            icons[i].SetLit(isLit, litAlpha, dimAlpha);
        }
    }

    public void ClearSlots()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        icons.Clear();
    }
}
