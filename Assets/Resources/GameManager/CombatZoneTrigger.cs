using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CombatZoneTrigger : MonoBehaviour
{
    public string zoneIdentifier; // Unique identifier for this combat zone
    public List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();
    public bool spawnOnStart = false;
    public bool allEnemiesChasing = false;
    public bool combatPanelActive = false;

    [Header("Loot Settings")]
    public LevelLootConfig levelLootConfig;
    public GameObject lootItemPrefab;
    public float lootDropRadius = 2f; // Radius around the drop position

    private bool enemiesSpawned = false;
    private Queue<EnemyAI> enemyQueue = new Queue<EnemyAI>();
    private Vector3 lastEnemyDefeatedPosition = Vector3.zero; // Store the position of the last defeated enemy

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemies();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !enemiesSpawned)
        {
            Debug.Log("Player entered combat zone!");
            SpawnEnemies();
        }
    }

    public void SpawnEnemies()
    {
        foreach (var enemyConfig in enemyConfigs)
        {
            GameObject enemyObject = Instantiate(enemyConfig.enemyPrefab, transform.position, Quaternion.identity, transform);
            enemyObject.name = enemyConfig.enemyPrefab.name;
            EnemyAI enemyAI = enemyObject.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyQueue.Enqueue(enemyAI);
            }
        }
        enemiesSpawned = true;
    }

    public void AlertAllEnemiesChase()
    {
        foreach (var enemy in enemyQueue)
        {
            enemy.AlertToChase();
        }
        allEnemiesChasing = true;
    }

    public EnemyAI GetNextEnemy()
    {
        if (enemyQueue.Count > 0)
        {
            return enemyQueue.Peek();
        }
        return null;
    }

    public void RemoveEnemy(EnemyAI enemy)
    {
        lastEnemyDefeatedPosition = enemy.transform.position; // Store the position
        enemyQueue = new Queue<EnemyAI>(enemyQueue.Where(e => e != enemy));
    }

    public bool IsQueueEmpty()
    {
        return enemyQueue.Count == 0;
    }

    // New method to get all enemies in the queue
    public Queue<EnemyAI> GetAllEnemies()
    {
        return new Queue<EnemyAI>(enemyQueue); // Return a copy to avoid direct modification
    }

    /// <summary>
    /// Identifies and returns the loot items for this zone without physically dropping them.
    /// </summary>
    /// <returns>A list of LootItem ScriptableObjects.</returns>
    public List<LootItem> GetLootItemsForDisplay()
    {
        List<LootItem> lootItemsForDisplay = new List<LootItem>();
        Debug.Log($"Retrieving loot items for display from zone '{zoneIdentifier}'.");

        // Find the loot configuration for this zone
        CombatZoneLoot zoneLootConfig = levelLootConfig.zoneLoot.FirstOrDefault(zl => zl.zoneIdentifier == zoneIdentifier);

        if (zoneLootConfig != null && zoneLootConfig.lootTable != null && zoneLootConfig.lootTable.lootItems.Length > 0)
        {
            LootTable lootTable = zoneLootConfig.lootTable;

            foreach (LootItem itemToDisplay in lootTable.lootItems)
            {
                if (itemToDisplay != null)
                {
                    lootItemsForDisplay.Add(itemToDisplay);
                    Debug.Log($"Added '{itemToDisplay.itemName}' to display list for zone '{zoneIdentifier}'.");
                }
                else
                {
                    Debug.LogWarning($"An item in the loot table for zone '{zoneIdentifier}' is null when getting for display.");
                }
            }
        }
        else
        {
            Debug.Log($"No loot configured for combat zone '{zoneIdentifier}' to display.");
        }
        return lootItemsForDisplay;
    }

    // New public method to drop loot at a specific position
    public void DropZoneLoot(Vector3 dropPosition)
    {
        Debug.Log($"Combat zone '{zoneIdentifier}' cleared. Attempting to drop loot at {dropPosition}.");

        // Find the loot configuration for this zone
        CombatZoneLoot zoneLootConfig = levelLootConfig.zoneLoot.FirstOrDefault(zl => zl.zoneIdentifier == zoneIdentifier);

        if (zoneLootConfig != null && zoneLootConfig.lootTable != null && zoneLootConfig.lootTable.lootItems.Length > 0)
        {
            LootTable lootTable = zoneLootConfig.lootTable;

            foreach (LootItem itemToDrop in lootTable.lootItems)
            {
                if (itemToDrop != null)
                {
                    Vector3 randomOffset = Random.insideUnitSphere * lootDropRadius;
                    randomOffset.y = Mathf.Abs(randomOffset.y); // Ensure loot drops above the ground slightly
                    Vector3 dropLocation = dropPosition + randomOffset;

                    GameObject droppedLoot = Instantiate(lootItemPrefab, dropLocation, Quaternion.identity);
                    LootItemPickup pickupScript = droppedLoot.GetComponent<LootItemPickup>();
                    SpriteRenderer spriteRenderer = droppedLoot.GetComponent<SpriteRenderer>(); // Get the SpriteRenderer

                    if (pickupScript != null)
                    {
                        pickupScript.lootItem = itemToDrop;
                    }
                    else
                    {
                        Debug.LogError("LootItemPrefab does not have a LootItemPickup script attached!");
                        Destroy(droppedLoot); // Clean up if the script is missing
                        continue; // Move to the next item
                    }

                    // Assign the sprite if the SpriteRenderer exists
                    if (spriteRenderer != null && itemToDrop.itemSprite != null)
                    {
                        spriteRenderer.sprite = itemToDrop.itemSprite;
                    }
                    else if (spriteRenderer == null)
                    {
                        Debug.LogWarning("LootItemPrefab does not have a SpriteRenderer component!");
                    }
                    else if (itemToDrop.itemSprite == null)
                    {
                        Debug.LogWarning($"LootItem '{itemToDrop.itemName}' has no sprite assigned!");
                    }

                    Debug.Log($"Dropped '{itemToDrop.itemName}' in zone '{zoneIdentifier}'.");
                }
                else
                {
                    Debug.LogWarning($"An item in the loot table for zone '{zoneIdentifier}' is null.");
                }
            }
        }
        else
        {
            Debug.Log($"No loot configured for combat zone '{zoneIdentifier}'.");
        }
    }

    public Vector3 GetLastEnemyDefeatedPosition()
    {
        return lastEnemyDefeatedPosition;
    }
}