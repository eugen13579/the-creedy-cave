using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controls individual inventory cell behavior, including item display, selection, and interaction.
/// </summary>
public class CellController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private GameObject highlight;
    [SerializeField] private Image itemIcon;

    public bool selected = false;
    public ItemData currentItem;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (highlight != null)
        {
            highlight.SetActive(false);
        }
    }

    public void SetHighlight(bool show)
    {
        if (highlight != null)
            highlight.SetActive(show);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        selected = true;
        animator.SetBool("selected", true);
        GameController.Instance.SetHoverCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        selected = false;
        animator.SetBool("selected", false);
        GameController.Instance.SetDefaultCursor();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryController.Instance.SelectCell(this);
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;
        if (itemIcon != null && item != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.color = Color.white;
            itemIcon.enabled = true;
        }
    }

    public void ClearItem()
    {
        currentItem = null;
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
    }
}
