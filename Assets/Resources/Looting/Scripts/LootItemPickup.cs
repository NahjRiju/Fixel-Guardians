using UnityEngine;
using System.Collections;

public class LootItemPickup : MonoBehaviour
{
    public LootItem lootItem; // Assign this in the inspector
    public float spriteSize = 1f; // Default size
    public float offsetY = 0.5f; // Default offset

    void Start()
    {
        // Apply the size and offset
        ApplySizeAndOffset();
    }

    void Update()
    {
        // Rotate the item
        transform.Rotate(Vector3.up * Time.deltaTime * 50f); // Adjust rotation speed as needed
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(DelayedPickup()); // Use a coroutine
        }
    }

    IEnumerator DelayedPickup()
    {
        yield return new WaitForSeconds(0.5f); // Adjust delay as needed
        Pickup();
    }

    void Pickup()
    {
        Debug.Log("LootItemPickup: Pickup called for " + lootItem.itemName);

        if (lootItem.isBuff)
        {
            // Handle buff loot via AlmanacManager
            AlmanacManager almanacManager = FindObjectOfType<AlmanacManager>();
            if (almanacManager != null)
            {
                almanacManager.ProcessBuffLoot(lootItem); // New method for buff loot
                Debug.Log("Will call ProcessBuffLoot");
            }
            else
            {
                Debug.LogError("LootItemPickup: AlmanacManager not found in the scene for buff loot.");
            }
        }
        else
        {
            // Handle regular loot via InventoryManager
            InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
            if (inventoryManager != null)
            {
                inventoryManager.AddItemToInventory(lootItem);
            }
            else
            {
                Debug.LogError("LootItemPickup: InventoryManager not found in the scene for regular loot.");
            }
        }

        Destroy(gameObject);
    }

    void ApplySizeAndOffset()
    {
        // Apply the size
        transform.localScale = new Vector3(spriteSize, spriteSize, spriteSize);

        // Apply the offset
        transform.position = new Vector3(transform.position.x, transform.position.y + offsetY, transform.position.z);
    }
}