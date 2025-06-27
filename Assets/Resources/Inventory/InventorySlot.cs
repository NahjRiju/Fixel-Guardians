using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image itemImage; // This is now also the Button
    // Removed public Button itemButton;

    private LootItem currentItem;

    void Start()
    {
        itemImage.GetComponent<Button>().onClick.AddListener(OnItemClicked); // Get Button from itemImage
    }

    public void SetItem(LootItem item)
    {
        currentItem = item;
        itemImage.sprite = item.itemSprite;
        itemImage.enabled = true;
        itemImage.GetComponent<Button>().interactable = true; // Get Button from itemImage
    }

    public void ClearSlot()
    {
        currentItem = null;
        itemImage.sprite = null;
        itemImage.enabled = false;
        itemImage.GetComponent<Button>().interactable = false; // Get Button from itemImage
    }

    void OnItemClicked()
    {
        if (currentItem != null)
        {
            InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
            if (inventoryManager != null)
            {
                inventoryManager.ShowItemDetails(currentItem);
            }
            else
            {
                Debug.LogError("InventoryManager not found in the scene.");
            }
        }
    }
}