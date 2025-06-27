using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq; // Added for .ToList()

public class InventoryManager : MonoBehaviour
{
    public GameObject inventorySlotPrefab;
    public Transform inventorySlotsParent;
    public int inventorySize = 20;
    public GameObject inventoryPanel;

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    // private List<LootItem> itemsInInventory = new List<LootItem>(); // This list will now be populated from PersistentGameManager

    public Image itemImageDisplay; // Assign in Inspector
    public TMP_Text itemNameText; // Assign in Inspector
    public TMP_Text itemDescriptionText; // Assign in Inspector

    void Start()
    {
        Debug.Log("InventoryManager: Start called");
        InitializeInventory();
        CloseInventory();
    }

    void InitializeInventory()
    {
        Debug.Log("InventoryManager: InitializeInventory called");
        // Clear existing slots if any, in case of re-initialization
        foreach (Transform child in inventorySlotsParent)
        {
            Destroy(child.gameObject);
        }
        inventorySlots.Clear();

        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObject = Instantiate(inventorySlotPrefab, inventorySlotsParent);
            InventorySlot slot = slotObject.GetComponent<InventorySlot>();
            inventorySlots.Add(slot);
            Debug.Log("InventoryManager: Inventory slot " + i + " created");
        }

        // --- NEW: Load items from PersistentGameManager ---
        if (PersistentGameManager.Instance != null)
        {
            List<LootItem> loadedItems = PersistentGameManager.Instance.GetCollectedInventoryItems();
            
            // Populate the UI with loaded items up to inventorySize
            for (int i = 0; i < loadedItems.Count && i < inventorySize; i++)
            {
                inventorySlots[i].SetItem(loadedItems[i]);
            }
            // Clear any remaining slots if loadedItems are fewer than current inventory size
            for (int i = loadedItems.Count; i < inventorySize; i++)
            {
                inventorySlots[i].ClearSlot();
            }
        }
        else
        {
            Debug.LogError("InventoryManager: PersistentGameManager.Instance is null! Cannot load inventory.");
        }
        // --------------------------------------------------
    }

    public void AddItemToInventory(LootItem item)
    {
        Debug.Log("InventoryManager: AddItemToInventory called with item: " + item.itemName);
        if (PersistentGameManager.Instance != null) // Check PersistentGameManager first
        {
            List<LootItem> currentPersistentItems = PersistentGameManager.Instance.GetCollectedInventoryItems();

            if (currentPersistentItems.Count < inventorySize)
            {
                PersistentGameManager.Instance.AddInventoryItem(item); // Add to persistent storage
                UpdateInventoryUI(); // Refresh UI after adding to persistent storage
            }
            else
            {
                Debug.Log("InventoryManager: Inventory is full (persistent storage limit reached)!");
            }
        }
        else
        {
            Debug.LogError("InventoryManager: PersistentGameManager.Instance is null! Cannot add item to inventory.");
        }
    }

    void UpdateInventoryUI()
    {
        Debug.Log("InventoryManager: UpdateInventoryUI called");
        if (PersistentGameManager.Instance == null)
        {
            Debug.LogError("PersistentGameManager.Instance is null. Cannot update inventory UI.");
            return;
        }

        List<LootItem> currentItems = PersistentGameManager.Instance.GetCollectedInventoryItems();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < currentItems.Count)
            {
                inventorySlots[i].SetItem(currentItems[i]);
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }

    public void ShowItemDetails(LootItem item)
    {
        if (item != null)
        {
            itemImageDisplay.sprite = item.itemSprite;
            itemNameText.text = item.itemName;
            itemDescriptionText.text = item.itemDescription;
        }
        else
        {
            Debug.Log("Item is null, cannot show details.");
        }
    }

    public void OpenInventory()
    {
        Debug.Log("InventoryManager: OpenInventory called");
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            UpdateInventoryUI(); // Ensure UI is updated when opened
        }
        else
        {
            Debug.LogError("InventoryManager: InventoryPanel GameObject not assigned!");
        }
    }

    public void CloseInventory()
    {
        Debug.Log("InventoryManager: CloseInventory called");
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("InventoryManager: InventoryPanel GameObject not assigned!");
        }
    }

    // Add any other inventory-related functionality here
}