using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement; 

public class PersistentGameManager : MonoBehaviour
{
    public static PersistentGameManager Instance { get; private set; }

    public bool hasKey = false;
    public int currentLevelIndex = 0; // Keep track of current level within an act

    public HashSet<string> unlockedLearningNames = new HashSet<string>();
    public HashSet<string> unlockedBuffEntryNames = new HashSet<string>();

    // NEW: List to store names of collected buffs for persistence
    private List<string> _collectedBuffNames = new List<string>();

    // Reference to ALL possible EffectConfigs (assign this in the Inspector)
    public List<EffectConfig> allAvailableBuffs; // Assign all your EffectConfig ScriptableObjects here

    // --- INVENTORY PERSISTENCE ADDITIONS ---
    private List<string> _collectedInventoryItemNames = new List<string>();
    public List<LootItem> allAvailableLootItems; // Assign ALL your LootItem ScriptableObjects here in the Inspector
    // ----------------------------------------

    public static event Action OnGameDataReset;

    // Store unlocked status of levels per act. Key: act index (0-based), Value: List of bools (true = unlocked)
    public Dictionary<int, List<bool>> actLevelUnlockStatus = new Dictionary<int, List<bool>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Awake(): Duplicate PersistentGameManager detected. Destroying.");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Awake(): Setting Instance and DontDestroyOnLoad.");
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAlmanacData();
            LoadLevelUnlockData(); // Load level unlock data on start
            LoadCollectedBuffsData(); // Load collected buffs data
            LoadInventoryData(); // NEW: Load inventory data on start
        }
    }

    // --- Buff Collection Persistence Methods ---

    public void CollectBuff(EffectConfig buffConfig)
    {
        if (buffConfig == null)
        {
            Debug.LogError("Attempted to collect a null buff config.");
            return;
        }

        if (!_collectedBuffNames.Contains(buffConfig.effectName))
        {
            _collectedBuffNames.Add(buffConfig.effectName);
            SaveCollectedBuffsData();
            Debug.Log($"Buff '{buffConfig.effectName}' collected and saved. Total collected: {_collectedBuffNames.Count}");
        }
        else
        {
            Debug.Log($"Buff '{buffConfig.effectName}' was already collected.");
        }
    }

    public List<EffectConfig> GetCollectedBuffs()
    {
        List<EffectConfig> collected = new List<EffectConfig>();
        foreach (string buffName in _collectedBuffNames)
        {
            EffectConfig foundBuff = allAvailableBuffs.Find(b => b.effectName == buffName);
            if (foundBuff != null)
            {
                collected.Add(foundBuff);
            }
            else
            {
                Debug.LogWarning($"Could not find EffectConfig for collected buff name: {buffName}. Was it removed or renamed?");
            }
        }
        return collected;
    }

    private void SaveCollectedBuffsData()
    {
        string collectedBuffsString = string.Join(",", _collectedBuffNames);
        PlayerPrefs.SetString("CollectedBuffs", collectedBuffsString);
        PlayerPrefs.Save();
        Debug.Log("Collected Buffs data saved: " + collectedBuffsString);
    }

    private void LoadCollectedBuffsData()
    {
        if (PlayerPrefs.HasKey("CollectedBuffs"))
        {
            string collectedBuffsString = PlayerPrefs.GetString("CollectedBuffs");
            if (!string.IsNullOrEmpty(collectedBuffsString))
            {
                _collectedBuffNames = new List<string>(collectedBuffsString.Split(',').ToList());
                Debug.Log("Collected Buffs data loaded: " + collectedBuffsString);
            }
            else
            {
                _collectedBuffNames.Clear();
                Debug.Log("CollectedBuffs string was empty, clearing list.");
            }
        }
        else
        {
            _collectedBuffNames.Clear();
            Debug.Log("No 'CollectedBuffs' key found in PlayerPrefs. Initializing empty list.");
        }
    }

    // --- INVENTORY PERSISTENCE METHODS ---

    public void AddInventoryItem(LootItem itemConfig)
    {
        if (itemConfig == null)
        {
            Debug.LogError("Attempted to add a null item config to inventory.");
            return;
        }

        // We allow duplicate items in inventory, so just add its name
        _collectedInventoryItemNames.Add(itemConfig.itemName); // Assuming itemName is unique enough or you need to allow duplicates
        SaveInventoryData();
        Debug.Log($"Inventory item '{itemConfig.itemName}' added and saved. Total items: {_collectedInventoryItemNames.Count}");
    }

    public List<LootItem> GetCollectedInventoryItems()
    {
        List<LootItem> collected = new List<LootItem>();
        foreach (string itemName in _collectedInventoryItemNames)
        {
            LootItem foundItem = allAvailableLootItems.Find(i => i.itemName == itemName);
            if (foundItem != null)
            {
                collected.Add(foundItem);
            }
            else
            {
                Debug.LogWarning($"Could not find LootItem for collected inventory item name: {itemName}. Was it removed or renamed?");
            }
        }
        return collected;
    }

    private void SaveInventoryData()
    {
        string inventoryItemsString = string.Join(",", _collectedInventoryItemNames);
        PlayerPrefs.SetString("CollectedInventoryItems", inventoryItemsString);
        PlayerPrefs.Save();
        Debug.Log("Inventory data saved: " + inventoryItemsString);
    }

    private void LoadInventoryData()
    {
        if (PlayerPrefs.HasKey("CollectedInventoryItems"))
        {
            string inventoryItemsString = PlayerPrefs.GetString("CollectedInventoryItems");
            if (!string.IsNullOrEmpty(inventoryItemsString))
            {
                _collectedInventoryItemNames = new List<string>(inventoryItemsString.Split(',').ToList());
                Debug.Log("Inventory data loaded: " + inventoryItemsString);
            }
            else
            {
                _collectedInventoryItemNames.Clear();
                Debug.Log("CollectedInventoryItems string was empty, clearing list.");
            }
        }
        else
        {
            _collectedInventoryItemNames.Clear();
            Debug.Log("No 'CollectedInventoryItems' key found in PlayerPrefs. Initializing empty list.");
        }
    }

    // --- Existing methods (kept for context) ---

    public void SaveLevelProgress(string levelName)
    {
        Debug.Log($"SaveLevelProgress() called for level: {levelName}");
        // ... (your level-specific save logic) ...
    }

    public void LoadLevelProgress(string levelName)
    {
        Debug.Log($"LoadLevelProgress() called for level: {levelName}");
        // ... (your level-specific load logic) ...
    }

    public bool GetHasKey()
    {
        Debug.Log("GetHasKey() called. Returning: " + hasKey);
        return hasKey;
    }

    public void SetHasKey(bool hasIt)
    {
        Debug.Log("SetHasKey() called with value: " + hasIt);
        hasKey = hasIt;
        Debug.Log($"Has Key: {hasKey}");
    }

    public void SaveAlmanacData()
    {
        Debug.Log("SaveAlmanacData() called.");
        Debug.Log("Unlocked Learning to save: " + string.Join(",", unlockedLearningNames));
        Debug.Log("Unlocked Buffs to save: " + string.Join(",", unlockedBuffEntryNames));
        PlayerPrefs.SetString("UnlockedLearning", string.Join(",", unlockedLearningNames));
        PlayerPrefs.SetString("UnlockedBuffs", string.Join(",", unlockedBuffEntryNames));
        PlayerPrefs.Save();
        Debug.Log("Almanac data saved.");
    }

    public void LoadAlmanacData()
    {
        Debug.Log("LoadAlmanacData() called.");
        if (PlayerPrefs.HasKey("UnlockedLearning"))
        {
            string learningString = PlayerPrefs.GetString("UnlockedLearning");
            Debug.Log("Loaded UnlockedLearning string: " + learningString);
            if (!string.IsNullOrEmpty(learningString))
            {
                unlockedLearningNames = new HashSet<string>(learningString.Split(','));
                Debug.Log("Parsed UnlockedLearning. Count: " + unlockedLearningNames.Count);
            }
            else
            {
                Debug.Log("UnlockedLearning string was empty.");
            }
        }
        else
        {
            Debug.Log("PlayerPrefs does not contain key 'UnlockedLearning'.");
        }

        if (PlayerPrefs.HasKey("UnlockedBuffs"))
        {
            string buffsString = PlayerPrefs.GetString("UnlockedBuffs");
            Debug.Log("Loaded UnlockedBuffs string: " + buffsString);
            if (!string.IsNullOrEmpty(buffsString))
            {
                unlockedBuffEntryNames = new HashSet<string>(buffsString.Split(','));
                Debug.Log("Parsed UnlockedBuffs. Count: " + unlockedBuffEntryNames.Count);
            }
            else
            {
                Debug.Log("UnlockedBuffs string was empty.");
            }
        }
        else
        {
            Debug.Log("PlayerPrefs does not contain key 'UnlockedBuffs'.");
        }
        Debug.Log("Almanac data loaded.");
    }

    public void ResetPersistentData()
    {
        Debug.Log("ResetPersistentData() called.");
        hasKey = false;
        currentLevelIndex = 0;
        unlockedLearningNames.Clear();
        unlockedBuffEntryNames.Clear();
        _collectedBuffNames.Clear(); // Clear collected buff names on reset
        _collectedInventoryItemNames.Clear(); // NEW: Clear collected inventory item names on reset
        actLevelUnlockStatus.Clear(); // Clear level unlock status as well
        PlayerPrefs.DeleteKey("UnlockedLearning");
        PlayerPrefs.DeleteKey("UnlockedBuffs");
        PlayerPrefs.DeleteKey("LevelUnlockStatus"); // Clear saved level unlocks
        PlayerPrefs.DeleteKey("CollectedBuffs"); // Delete collected buffs key
        PlayerPrefs.DeleteKey("CollectedInventoryItems"); // NEW: Delete collected inventory items key
        PlayerPrefs.Save();
        Debug.Log("Persistent game data reset.");

        if (OnGameDataReset != null)
        {
            Debug.Log("Broadcasting OnGameDataReset event.");
            OnGameDataReset.Invoke();
        }
    }

    // Save the unlocked level data to PlayerPrefs
    private void SaveLevelUnlockData()
    {
        string data = "";
        foreach (var actEntry in actLevelUnlockStatus)
        {
            data += actEntry.Key.ToString() + ",";
            for (int i = 0; i < actEntry.Value.Count; i++)
            {
                data += actEntry.Value[i].ToString() + (i < actEntry.Value.Count - 1 ? "," : "");
            }
            data += ";";
        }
        PlayerPrefs.SetString("LevelUnlockStatus", data);
        PlayerPrefs.Save();
        Debug.Log("Level unlock data saved: " + data);
    }
    // Load the unlocked level data from PlayerPrefs
    private void LoadLevelUnlockData()
    {
        actLevelUnlockStatus.Clear();
        string data = PlayerPrefs.GetString("LevelUnlockStatus", "");
        if (!string.IsNullOrEmpty(data))
        {
            string[] actEntries = data.Split(';');
            foreach (string actEntry in actEntries)
            {
                if (!string.IsNullOrEmpty(actEntry))
                {
                    string[] levelStatuses = actEntry.Split(',');
                    int actIndex = int.Parse(levelStatuses[0]);
                    List<bool> unlocks = new List<bool>();
                    for (int i = 1; i < levelStatuses.Length; i++)
                    {
                        unlocks.Add(bool.Parse(levelStatuses[i]));
                    }
                    actLevelUnlockStatus[actIndex] = unlocks;
                }
            }
            Debug.Log("Level unlock data loaded.");
        }
        else
        {
            Debug.Log("No level unlock data saved.");
        }
    }

    // Helper to get the unlock status of a specific level
    public bool IsLevelUnlocked(int actIndex, int levelIndex)
    {
        bool unlocked = levelIndex == 0; // First level is always unlocked by default
        if (actLevelUnlockStatus.ContainsKey(actIndex) && levelIndex < actLevelUnlockStatus[actIndex].Count)
        {
            unlocked = actLevelUnlockStatus[actIndex][levelIndex];
        }
        Debug.Log($"PersistentGameManager.IsLevelUnlocked() called. Act: {actIndex}, Level: {levelIndex}, Unlocked: {unlocked}");
        return unlocked;
    }

    // Helper to unlock the next level
    public void UnlockNextLevel(int actIndex, int currentLevelIndex)
    {
        Debug.Log($"PersistentGameManager.UnlockNextLevel() - Received actIndex: {actIndex}, currentLevelIndex: {currentLevelIndex}");

        if (ActSelectionController.Instance == null || ActSelectionController.Instance.allActs == null)
        {
            Debug.LogError("PersistentGameManager: ActSelectionController instance or allActs array is NULL!");
            return;
        }

        if (actIndex < 0 || actIndex >= ActSelectionController.Instance.allActs.Length)
        {
            Debug.LogError($"PersistentGameManager: Act Index out of bounds! Act Index: {actIndex}, allActs Length: {ActSelectionController.Instance.allActs.Length}");
            return;
        }

        // Mark the current level as unlocked (completed)
        if (!actLevelUnlockStatus.ContainsKey(actIndex))
        {
            actLevelUnlockStatus[actIndex] = new List<bool>();
        }
        while (actLevelUnlockStatus[actIndex].Count <= currentLevelIndex)
        {
            actLevelUnlockStatus[actIndex].Add(false);
        }
        if (currentLevelIndex < actLevelUnlockStatus[actIndex].Count)
        {
            actLevelUnlockStatus[actIndex][currentLevelIndex] = true;
            Debug.Log($"PersistentGameManager.UnlockNextLevel() - Marked level {currentLevelIndex + 1} in act {actIndex + 1} as unlocked.");
        }

        // Check if the completed level was the last level of the current act
        if (currentLevelIndex == ActSelectionController.Instance.allActs[actIndex].levels.Length - 1)
        {
            Debug.Log($"PersistentGameManager.UnlockNextLevel() - Last level of Act {actIndex + 1} completed. Attempting to unlock next act.");
            UnlockFirstLevelNextAct(actIndex);
        }
        else
        {
            // Unlock the next level within the current act
            int nextLevelIndex = currentLevelIndex + 1;
            Debug.Log($"PersistentGameManager.UnlockNextLevel() - Attempting to unlock next level at index: {nextLevelIndex} in act: {actIndex}");
            while (actLevelUnlockStatus[actIndex].Count <= nextLevelIndex)
            {
                actLevelUnlockStatus[actIndex].Add(false);
            }
            if (nextLevelIndex < ActSelectionController.Instance.allActs[actIndex].levels.Length)
            {
                actLevelUnlockStatus[actIndex][nextLevelIndex] = true;
                SaveLevelUnlockData(); // Save immediately after unlocking
                Debug.Log($"PersistentGameManager.UnlockNextLevel() - Unlocked level {nextLevelIndex + 1} in act {actIndex + 1}. Saved data.");
            }
            else
            {
                Debug.Log($"PersistentGameManager.UnlockNextLevel() - No next level to unlock in act {actIndex + 1}.");
            }
        }
    }

    public void UnlockFirstLevelNextAct(int currentActIndex)
    {
        int nextActIndex = currentActIndex + 1;
        if (nextActIndex < ActSelectionController.Instance.allActs.Length)
        {
            if (!actLevelUnlockStatus.ContainsKey(nextActIndex))
            {
                actLevelUnlockStatus[nextActIndex] = new List<bool>();
            }
            // Ensure the first level is marked as unlocked
            if (actLevelUnlockStatus[nextActIndex].Count == 0)
            {
                actLevelUnlockStatus[nextActIndex].Add(true);
            }
            else if (actLevelUnlockStatus[nextActIndex].Count > 0)
            {
                actLevelUnlockStatus[nextActIndex][0] = true;
            }
            SaveLevelUnlockData();
            Debug.Log($"Unlocked first level of Act {nextActIndex + 1}.");
        }
        else
        {
            Debug.Log("No next act to unlock.");
        }
    }

    public void UnlockAllContent()
    {
        // --- Unlock all Acts and Levels ---
        if (ActSelectionController.Instance != null && ActSelectionController.Instance.allActs != null)
        {
            actLevelUnlockStatus.Clear(); // Clear existing unlock data
            
            for (int i = 0; i < ActSelectionController.Instance.allActs.Length; i++)
            {
                if (!actLevelUnlockStatus.ContainsKey(i))
                {
                    actLevelUnlockStatus[i] = new List<bool>();
                }
                for (int j = 0; j < ActSelectionController.Instance.allActs[i].levels.Length; j++)
                {
                    actLevelUnlockStatus[i].Add(true);
                }
            }
            SaveLevelUnlockData(); // Save the unlocked level state
            Debug.Log("All acts and levels unlocked!");
        }
        else
        {
            Debug.LogWarning("ActSelectionController instance or allActs array is NULL! Cannot unlock levels.");
        }

        // --- Unlock all Learning Almanac Entries ---
        unlockedLearningNames.Clear();
        LevelLearningData[] allLearnings = Resources.LoadAll<LevelLearningData>("Almanac/Data/Learnings");
        foreach (LevelLearningData learning in allLearnings)
        {
            unlockedLearningNames.Add(learning.learningName.Trim().ToLower());
        }
        Debug.Log($"Unlocked {unlockedLearningNames.Count} learning entries in Almanac.");

        // --- Unlock all Buff Almanac Entries ---
        unlockedBuffEntryNames.Clear();
        BuffEffectData[] allBuffs = Resources.LoadAll<BuffEffectData>("Almanac/Data/Buffs"); 
        foreach (BuffEffectData buff in allBuffs)
        {
            unlockedBuffEntryNames.Add(buff.effectName.Trim().ToLower());
        }
        Debug.Log($"Unlocked {unlockedBuffEntryNames.Count} buff entries in Almanac.");

        // --- Also collect all buffs (for the BuffSelectionController) ---
        _collectedBuffNames.Clear(); 
        if (allAvailableBuffs != null)
        {
            foreach(var buff in allAvailableBuffs)
            {
                if (!_collectedBuffNames.Contains(buff.effectName))
                {
                    _collectedBuffNames.Add(buff.effectName);
                }
            }
            Debug.Log($"Collected {allAvailableBuffs.Count} actual buffs for player inventory/selection.");
        }
        else
        {
            Debug.LogWarning("allAvailableBuffs list is not assigned in PersistentGameManager. Cannot collect all buffs via UnlockAllContent.");
        }

        // --- NEW: Unlock all Inventory Items (if desired for "Unlock All Content") ---
        _collectedInventoryItemNames.Clear();
        LootItem[] allItems = Resources.LoadAll<LootItem>("LootItems"); // Adjust this path if your LootItems are elsewhere!
        if (allItems != null)
        {
            foreach (LootItem item in allItems)
            {
                _collectedInventoryItemNames.Add(item.itemName);
            }
            Debug.Log($"Collected {allItems.Length} inventory items via UnlockAllContent.");
        }
        else
        {
            Debug.LogWarning("No LootItems found in Resources/LootItems. Cannot unlock all inventory items.");
        }


        SaveAlmanacData();         // Save the updated Almanac unlock status
        SaveCollectedBuffsData();  // Save the newly collected buffs
        SaveInventoryData();       // NEW: Save the updated inventory data
        
        Debug.Log("All content unlocked and saved!");

        // Trigger the reset event to ensure UI updates if necessary
        if (OnGameDataReset != null)
        {
            Debug.Log("Broadcasting OnGameDataReset event after UnlockAllContent.");
            OnGameDataReset.Invoke();
        }
    }
}