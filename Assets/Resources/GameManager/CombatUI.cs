using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CombatUI : MonoBehaviour
{
    public static CombatUI Instance { get; private set; }

    public GameObject combatPanel;
    public Button attackButton;
    public TMP_Text turnIndicator;
    public GameObject[] uiToHide;
    public GameObject winPanel;
    public Transform lootDisplayContainer; // A RectTransform within your winPanel to parent the loot items
    public GameObject lootItemUIPrefab; // A small UI prefab for each individual loot item (Image + Text)

    public Button proceedButton;

    [Header("Enemy Display")]
    public Healthbar enemyHealthbar;
    public TMP_Text enemyNameText;
    public Transform enemyQueueContainer;
    public GameObject enemyIconPrefab;
    public Image currentEnemyIconDisplay; // New variable for the current enemy icon
    private Dictionary<EnemyAI, GameObject> displayedEnemyIcons = new Dictionary<EnemyAI, GameObject>();

    [Header("Buff/Debuff Display")]
    public Transform playerBuffsContainer;
    public GameObject playerBuffDisplayPrefab;
    public GameObject playerDebuffDisplayPrefab;

    public Transform enemyBuffsContainer;
    public GameObject enemyBuffDisplayPrefab;
    public GameObject enemyDebuffDisplayPrefab;

    private Dictionary<ActiveEffect, GameObject> playerActiveEffectDisplays = new Dictionary<ActiveEffect, GameObject>();
    private Dictionary<ActiveEffect, GameObject> enemyActiveEffectDisplays = new Dictionary<ActiveEffect, GameObject>();

    public CombatLogUI combatLogUI;
    public Button combatLogButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple CombatUIs detected! Destroying the duplicate.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        combatPanel.SetActive(false);
        attackButton.onClick.AddListener(CombatManager.Instance.PlayerAttack);
        if (winPanel != null) winPanel.SetActive(false);
        if (proceedButton != null) proceedButton.onClick.AddListener(HideCombatPanel);
        else Debug.LogError("Proceed Button not assigned in CombatUI!");

        if (enemyHealthbar == null) Debug.LogError("Enemy Healthbar not assigned in CombatUI!");
        if (enemyNameText == null) Debug.LogError("Enemy Name Text not assigned in CombatUI!");
        if (enemyQueueContainer == null) Debug.LogError("Enemy Queue Container not assigned in CombatUI!");
        if (enemyIconPrefab == null) Debug.LogError("Enemy Icon Prefab not assigned in CombatUI!");
        if (currentEnemyIconDisplay == null) Debug.LogError("Current Enemy Icon Display not assigned in CombatUI!"); // Error check for the new element

        if (playerBuffsContainer == null) Debug.LogError("Player Buffs Container not assigned in CombatUI!");
        if (playerBuffDisplayPrefab == null) Debug.LogError("Player Buff Display Prefab not assigned in CombatUI!");
        if (playerDebuffDisplayPrefab == null) Debug.LogError("Player Debuff Display Prefab not assigned in CombatUI!");
        if (enemyBuffsContainer == null) Debug.LogError("Enemy Buffs Container not assigned in CombatUI!");
        if (enemyBuffDisplayPrefab == null) Debug.LogError("Enemy Buff Display Prefab not assigned in CombatUI!");
        if (enemyDebuffDisplayPrefab == null) Debug.LogError("Enemy Debuff Display Prefab not assigned in CombatUI!");

        if (combatLogButton != null && combatLogUI != null)
        {
            combatLogButton.onClick.AddListener(combatLogUI.ShowLogPanel);
            Debug.Log("CombatUI: Combat Log Button listener added."); // Added debug log
        }
        else
        {
            Debug.LogError("CombatUI: CombatLogButton not assigned or CombatLogUI not found!");
            if (combatLogButton == null)
            {
                Debug.LogError("CombatUI: combatLogButton is null.");
            }
            if (combatLogUI == null)
            {
                Debug.LogError("CombatUI: combatLogUI is null.");
            }
        }
    }

    public void ShowCombatPanel(Queue<EnemyAI> initialEnemyQueue)
    {
        combatPanel.SetActive(true);
        HideOtherUI();
        if (winPanel != null) winPanel.SetActive(false);
        PopulateEnemyQueue(initialEnemyQueue);
    }

    public void EnableAttackButton()
    {
        attackButton.interactable = true;
    }

    public void DisableAttackButton()
    {
        attackButton.interactable = false;
    }

    public void SetTurnIndicator(CombatManager.Turn turn)
    {
        turnIndicator.text = turn.ToString() + " Turn";
    }

    private void HideOtherUI()
    {
        foreach (GameObject ui in uiToHide)
        {
            if (ui != null) ui.SetActive(false);
        }
    }

    public void ShowOtherUI()
    {
        foreach (GameObject ui in uiToHide)
        {
            if (ui != null) ui.SetActive(true);
        }
    }

    public void HideCombatPanel()
    {
        combatPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        ClearEnemyQueueDisplay();
        ShowOtherUI();
    }

    public void ShowWinPanel(List<LootItem> earnedLoot)
    {
        winPanel.SetActive(true);
        // Add any other win panel setup here (e.g., displaying "Victory!")

        // Clear any previously displayed loot items
        foreach (Transform child in lootDisplayContainer)
        {
            Destroy(child.gameObject);
        }

        // Display the new loot items
        if (earnedLoot != null && earnedLoot.Count > 0)
        {
            foreach (LootItem item in earnedLoot)
            {
                if (item != null)
                {
                    // Instantiate the UI prefab for the loot item
                    GameObject lootUIElement = Instantiate(lootItemUIPrefab, lootDisplayContainer);

                    // Assuming your lootItemUIPrefab has an Image and a TextMeshProUGUI
                    // Adjust these GetComponent calls based on your prefab's structure
                    Image itemImage = lootUIElement.GetComponentInChildren<Image>(); 
                    TextMeshProUGUI itemNameText = lootUIElement.GetComponentInChildren<TextMeshProUGUI>();

                    if (itemImage != null && item.itemSprite != null)
                    {
                        itemImage.sprite = item.itemSprite;
                    }
                    else if (itemImage == null)
                    {
                        Debug.LogWarning("LootItemUIPrefab does not have an Image component or it's not found in children.");
                    }
                    else if (item.itemSprite == null)
                    {
                        Debug.LogWarning($"LootItem '{item.itemName}' has no sprite assigned for UI display.");
                    }

                    if (itemNameText != null && item.itemName != null)
                    {
                        itemNameText.text = item.itemName;
                    }
                    else if (itemNameText == null)
                    {
                        Debug.LogWarning("LootItemUIPrefab does not have a TextMeshProUGUI component or it's not found in children.");
                    }

                    Debug.Log($"Displaying '{item.itemName}' in Win Panel.");
                }
            }
        }
        else
        {
            Debug.Log("No loot items to display in the win panel.");
            // Optionally, display a "No Loot Earned" message.
        }
    }

    public void UpdateEnemyDisplay(EnemyAI enemy)
    {
        if (enemyHealthbar != null && enemy != null && enemy.GetComponent<Health>() != null)
        {
            enemyHealthbar.Health = enemy.GetComponent<Health>();
        }
        if (enemyNameText != null && enemy != null && enemy.enemyConfig != null)
        {
            enemyNameText.text = enemy.enemyConfig.enemyName;
        }
        if (currentEnemyIconDisplay != null && enemy != null && enemy.enemyConfig != null && enemy.enemyConfig.enemyIcon != null)
        {
            currentEnemyIconDisplay.sprite = enemy.enemyConfig.enemyIcon;
            currentEnemyIconDisplay.enabled = true; // Make sure the icon is visible
        }
        else if (currentEnemyIconDisplay != null)
        {
            currentEnemyIconDisplay.enabled = false; // Hide if no current enemy or icon
        }
    }

    private void PopulateEnemyQueue(Queue<EnemyAI> enemyQueue)
    {
        ClearEnemyQueueDisplay();
        displayedEnemyIcons.Clear();

        foreach (var enemy in enemyQueue)
        {
            if (enemy != null && enemy.enemyConfig != null && enemyIconPrefab != null && enemyQueueContainer != null)
            {
                GameObject iconObject = Instantiate(enemyIconPrefab, enemyQueueContainer);
                Image iconImage = iconObject.transform.Find("Icon").GetComponent<Image>();
                EnemyIconUI iconUI = iconObject.transform.Find("Icon").GetComponent<EnemyIconUI>();

                if (iconImage != null && enemy.enemyConfig.enemyIcon != null)
                {
                    iconImage.sprite = enemy.enemyConfig.enemyIcon;
                }
                else if (iconImage == null)
                {
                    Debug.LogError("Child 'Icon' with Image component not found on EnemyIcon prefab instance!");
                }

                if (iconUI != null)
                {
                    iconUI.SetEnemy(enemy); // ASSIGN THE REPRESENTED ENEMY HERE!
                }
                else
                {
                    Debug.LogError("Child 'Icon' with EnemyIconUI component not found on EnemyIcon prefab instance!");
                }

                displayedEnemyIcons.Add(enemy, iconObject);
            }
        }
    }

    public void SetEnemyQueueInteraction(CombatManager.Turn currentTurn)
    {
        foreach (var pair in displayedEnemyIcons)
        {
            EnemyAI enemy = pair.Key;
            GameObject iconObject = pair.Value;
            EnemyIconUI iconUI = iconObject.transform.Find("Icon").GetComponent<EnemyIconUI>();
            if (iconUI != null)
            {
                iconUI.EnableInteraction(currentTurn == CombatManager.Turn.Player);
            }
        }
    }

    public void UpdateEnemyQueueDisplay(EnemyAI removedEnemy)
    {
        if (displayedEnemyIcons.ContainsKey(removedEnemy))
        {
            Destroy(displayedEnemyIcons[removedEnemy]);
            displayedEnemyIcons.Remove(removedEnemy);
        }
    }

    public void ClearEnemyQueueDisplay()
    {
        if (enemyQueueContainer != null)
        {
            foreach (Transform child in enemyQueueContainer)
            {
                Destroy(child.gameObject);
            }
        }
        displayedEnemyIcons.Clear();
    }

    public void AddPlayerActiveEffectDisplay(ActiveEffect effect)
    {
        GameObject displayPrefab = null;
        Transform container = playerBuffsContainer; // Use the single player container

        if (effect.config.effectCategory == EffectConfig.EffectCategory.Buff)
        {
            displayPrefab = playerBuffDisplayPrefab;
        }
        else if (effect.config.effectCategory == EffectConfig.EffectCategory.Debuff)
        {
            displayPrefab = playerDebuffDisplayPrefab;
        }

        if (displayPrefab != null && container != null)
        {
            GameObject displayInstance = Instantiate(displayPrefab, container);
            Image icon = displayInstance.transform.Find("Background/BuffIcon").GetComponent<Image>();
            TMP_Text remainingTurnsText = displayInstance.transform.Find("RemainingTurns").GetComponent<TMP_Text>();

            if (icon != null && effect.config.effectIcon != null)
            {
                icon.sprite = effect.config.effectIcon;
            }
            if (remainingTurnsText != null)
            {
                remainingTurnsText.text = effect.remainingTurns.ToString();
            }

            playerActiveEffectDisplays.Add(effect, displayInstance);
        }
    }

    public void AddEnemyActiveEffectDisplay(ActiveEffect effect)
    {
        GameObject displayPrefab = null;
        Transform container = enemyBuffsContainer; // Use the single enemy container

        if (effect.config.effectCategory == EffectConfig.EffectCategory.Buff)
        {
            displayPrefab = enemyBuffDisplayPrefab;
        }
        else if (effect.config.effectCategory == EffectConfig.EffectCategory.Debuff)
        {
            displayPrefab = enemyDebuffDisplayPrefab;
        }

        if (displayPrefab != null && container != null)
        {
            GameObject displayInstance = Instantiate(displayPrefab, container);
            Image icon = displayInstance.transform.Find("Background/BuffIcon").GetComponent<Image>();
            TMP_Text remainingTurnsText = displayInstance.transform.Find("RemainingTurns").GetComponent<TMP_Text>();

            if (icon != null && effect.config.effectIcon != null)
            {
                icon.sprite = effect.config.effectIcon;
            }
            if (remainingTurnsText != null)
            {
                remainingTurnsText.text = effect.remainingTurns.ToString();
            }

            enemyActiveEffectDisplays.Add(effect, displayInstance);
        }
    }

    public void UpdateActiveEffectDisplay(ActiveEffect effect)
    {
        Dictionary<ActiveEffect, GameObject> relevantDisplay = null;

        if (playerActiveEffectDisplays.ContainsKey(effect))
        {
            relevantDisplay = playerActiveEffectDisplays;
        }
        else if (enemyActiveEffectDisplays.ContainsKey(effect))
        {
            relevantDisplay = enemyActiveEffectDisplays;
        }

        if (relevantDisplay != null && relevantDisplay.ContainsKey(effect))
        {
            TMP_Text remainingTurnsText = relevantDisplay[effect].transform.Find("RemainingTurns").GetComponent<TMP_Text>();
            if (remainingTurnsText != null)
            {
                remainingTurnsText.text = effect.remainingTurns.ToString();
            }
        }
    }

    public void RemoveActiveEffectDisplay(ActiveEffect effect)
    {
        if (playerActiveEffectDisplays.ContainsKey(effect))
        {
            Destroy(playerActiveEffectDisplays[effect]);
            playerActiveEffectDisplays.Remove(effect);
        }
        else if (enemyActiveEffectDisplays.ContainsKey(effect))
        {
            Destroy(enemyActiveEffectDisplays[effect]);
            enemyActiveEffectDisplays.Remove(effect);
        }
    }

    // New method to clear enemy active effect displays
    public void ClearEnemyActiveEffectDisplays()
    {
        foreach (var pair in enemyActiveEffectDisplays)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }
        enemyActiveEffectDisplays.Clear();
    }

    // New method to clear player active effect displays
    public void ClearPlayerActiveEffectDisplays()
    {
        foreach (var pair in playerActiveEffectDisplays)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }
        playerActiveEffectDisplays.Clear();
    }
}