using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class AlmanacManager : MonoBehaviour
{
    public GameObject almanacPanel;
    public GameObject listViewContent;
    public GameObject listItemPrefab;
    public Image detailImage;
    public TMP_Text detailName;
    public TMP_Text detailText;
    public GameObject detailView;
    public PlayerConfig playerConfig; // Public reference to the PlayerConfig (still needed for selectedEffects, etc.)

    private List<AlmanacEntry> learningEntries = new List<AlmanacEntry>();
    private List<AlmanacEntry> buffEntries = new List<AlmanacEntry>();
    private List<GameObject> currentListItems = new List<GameObject>();
    private bool isShowingLearning = false;
    private bool isShowingBuffs = false;

    public LevelConfig levelConfig; // Add LevelConfig reference
    public HashSet<string> unlockedLearningNames { get; private set; } = new HashSet<string>(); // Public getter, private setter
    public HashSet<string> unlockedBuffEntryNames { get; private set; } = new HashSet<string>(); // Public getter, private setter

    public void LoadUnlockedEntries(HashSet<string> learningNames, HashSet<string> buffNames)
    {
        unlockedLearningNames = learningNames;
        unlockedBuffEntryNames = buffNames;
        Debug.Log($"AlmanacManager: Loaded {unlockedLearningNames.Count} learning entries and {unlockedBuffEntryNames.Count} buff entries.");
        // Refresh UI if Almanac is open
        if (almanacPanel.activeSelf)
        {
            if (isShowingLearning) ShowLearningTab();
            else if (isShowingBuffs) ShowBuffsTab();
        }
    }

    public void ShowAlmanac()
    {
        Debug.Log("AlmanacManager: ShowAlmanac() called");
        almanacPanel.SetActive(true);
        ShowLearningTab(); // Default to Learning tab

         // ✅ Unlock Almanac achievement once
    if (!AchievementsManager.Instance.IsUnlocked("view_almanac"))
    {
        AchievementsManager.Instance.UnlockAchievement("view_almanac");
    }
    }

    public void HideAlmanac()
    {
        Debug.Log("AlmanacManager: HideAlmanac() called");
        almanacPanel.SetActive(false);
        ClearList();
        isShowingLearning = false;
        isShowingBuffs = false;
    }

    public void ShowLearningTab()
    {
        if (isShowingLearning) return;
        isShowingLearning = true;
        isShowingBuffs = false;
        Debug.Log("AlmanacManager: ShowLearningTab() called");
        ClearList();
        learningEntries.Clear();

        if (detailView != null)
        {
            detailView.SetActive(false);
        }

        LevelLearningData[] learnings = Resources.LoadAll<LevelLearningData>("Almanac/Data/Learnings");
        Debug.Log("AlmanacManager: Found " + learnings.Length + " Learning entries");

        foreach (LevelLearningData learning in learnings)
        {
            bool unlocked = unlockedLearningNames.Contains(learning.learningName.Trim().ToLower());
            AlmanacEntry newEntry = new AlmanacEntry(learning, learning.learningImage, learning.learningName, unlocked);
            learningEntries.Add(newEntry);
            Debug.Log($"Learning Entry: {learning.learningName}, Unlocked Status: {newEntry.isUnlocked}");
        }

        PopulateList(learningEntries);
    }

    public void ShowBuffsTab()
    {
        if (isShowingBuffs) return;
        isShowingBuffs = true;
        isShowingLearning = false;
        Debug.Log("AlmanacManager: ShowBuffsTab() called");
        ClearList();
        buffEntries.Clear();

        if (detailView != null)
        {
            detailView.SetActive(false);
        }

        BuffEffectData[] buffData = Resources.LoadAll<BuffEffectData>("Almanac/Data/Buffs"); // Assuming BuffEffectData are in a "Buffs" subfolder
        Debug.Log("AlmanacManager: Found " + buffData.Length + " Buff entries");

        foreach (BuffEffectData buff in buffData)
        {
            bool unlocked = unlockedBuffEntryNames.Contains(buff.effectName.Trim().ToLower());
            AlmanacEntry newEntry = new AlmanacEntry(buff, buff.effectIcon, buff.effectName, unlocked);
            buffEntries.Add(newEntry);
            Debug.Log($"Buff Entry: {buff.effectName}, Unlocked Status: {newEntry.isUnlocked}");
        }

        PopulateList(buffEntries);
    }

    // NEW METHOD: DYNAMICALLY UNLOCK LEARNING ENTRY BASED ON FRAGMENT TYPE
    public void UnlockLearningEntry(string fragmentType)
    {
        Debug.Log("AlmanacManager: UnlockLearningEntry called with fragmentType: " + fragmentType);

        if (levelConfig != null && levelConfig.objectives != null)
        {
            foreach (var objective in levelConfig.objectives)
            {
                if (objective.conditionParameter == fragmentType)
                {
                    int collected = objective.currentCount;
                    var entries = objective.learningEntriesToUnlock;

                    if (entries != null && collected - 1 < entries.Length)
                    {
                        string entryName = entries[collected - 1]; // unlock based on how many fragments collected
                        Debug.Log($"Unlocking learning entry '{entryName}' for fragment #{collected}");
                        UnlockAlmanacLearningEntry(entryName);
                    }
                    break;
                }
            }
        }
    }

    public void UnlockAlmanacLearningEntry(string name)
    {
        Debug.Log("UnlockAlmanacLearningEntry called with name: " + name);
        unlockedLearningNames.Add(name.Trim().ToLower());
        UpdateAlmanacList();
        // You might want to save persistent data here as well, if these unlocks
        // are separate from the collected buffs. PersistentGameManager handles this.
    }

    public void UnlockAlmanacBuffEntry(string name)
    {
        Debug.Log("UnlockAlmanacBuffEntry called with name: " + name);
        unlockedBuffEntryNames.Add(name.Trim().ToLower());
        UpdateAlmanacList();
        // You might want to save persistent data here as well, if these unlocks
        // are separate from the collected buffs. PersistentGameManager handles this.
    }

    private void UpdateAlmanacList()
    {
        ClearList();
        if (isShowingLearning)
        {
            PopulateList(learningEntries);
        }
        else if (isShowingBuffs)
        {
            PopulateList(buffEntries);
        }
    }

    private void PopulateList(List<AlmanacEntry> entryList)
    {
        Debug.Log($"AlmanacManager: PopulateList() called with {entryList.Count} entries.");
        foreach (AlmanacEntry entry in entryList)
        {
            Debug.Log($"Populating entry: {entry.name}, Unlocked: {entry.isUnlocked}");
            AddListItem(entry);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(listViewContent.GetComponent<RectTransform>());
    }

    private void AddListItem(AlmanacEntry entry)
    {
        Debug.Log($"AlmanacManager: AddListItem() called. Entry name: {entry.name}");
        GameObject listItem = Instantiate(listItemPrefab, listViewContent.transform);
        Image itemImage = listItem.GetComponentInChildren<Image>();
        TMP_Text itemText = listItem.GetComponentInChildren<TMP_Text>(); // Assuming you have a Text component

        if (itemImage != null)
        {
            itemImage.sprite = entry.image;
            itemImage.color = entry.isUnlocked ? Color.white : Color.black;
        }

        if (itemText != null)
        {
            itemText.text = entry.name;
            itemText.color = entry.isUnlocked ? Color.white : Color.gray;
        }

        listItem.GetComponent<Button>().onClick.AddListener(() => ShowDetail(entry.data, entry.isUnlocked));
        currentListItems.Add(listItem);
    }

    public void ShowDetail(ScriptableObject data, bool isUnlocked)
    {
        Debug.Log($"AlmanacManager: ShowDetail() called. Data type: {data?.GetType().Name}, Unlocked: {isUnlocked}");

        if (detailView != null)
        {
            detailView.SetActive(true);
        }

        if (data is LevelLearningData learning)
        {
            detailImage.sprite = isUnlocked ? learning.learningImage : null;
            detailName.text = isUnlocked ? learning.learningName : "???";
            detailText.text = isUnlocked ? learning.learningDescription : "Information Locked.";
        }
        else if (data is BuffEffectData buff)
        {
            detailImage.sprite = isUnlocked ? buff.effectIcon : null;
            detailName.text = isUnlocked ? buff.effectName : "???";
            detailText.text = isUnlocked ? buff.effectDescription : "Information Locked.";
        }
    }

    private void ClearList()
    {
        Debug.Log("AlmanacManager: ClearList() called");
        foreach (GameObject item in currentListItems)
        {
            Destroy(item);
        }
        currentListItems.Clear();
    }

    public void ProcessBuffLoot(LootItem collectedLoot)
    {
        if (collectedLoot.isBuff && collectedLoot.buffEffect != null) // Removed playerConfig check here
        {
            Debug.Log($"AlmanacManager: Processing buff loot - {collectedLoot.itemName} ({collectedLoot.buffEffect.effectName})");

            // --- THE CRUCIAL CHANGE IS HERE ---
            if (PersistentGameManager.Instance != null)
            {
                // Call PersistentGameManager to collect and save the buff
                PersistentGameManager.Instance.CollectBuff(collectedLoot.buffEffect);
                Debug.Log($"AlmanacManager: Buff '{collectedLoot.buffEffect.effectName}' sent to PersistentGameManager for collection.");

                // Unlock the corresponding Almanac entry (still relevant for Almanac UI)
                UnlockAlmanacBuffEntry(collectedLoot.buffEffect.effectName);
            }
            else
            {
                Debug.LogError("AlmanacManager: PersistentGameManager instance is null! Cannot collect buff.");
            }
        }
        else if (collectedLoot.isBuff && collectedLoot.buffEffect == null)
        {
            Debug.LogError($"AlmanacManager: Buff loot '{collectedLoot.itemName}' has isBuff set to true but no buffEffect assigned!");
        }
        else if (!collectedLoot.isBuff)
        {
            Debug.Log($"AlmanacManager: Loot '{collectedLoot.itemName}' is not a buff, ignoring.");
            // The InventoryManager should handle this.
        }
        // Removed the playerConfig == null check as it's now handled by PersistentGameManager check
    }
}