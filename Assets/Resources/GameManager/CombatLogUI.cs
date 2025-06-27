using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatLogUI : MonoBehaviour
{
    public GameObject combatLogPanel; // Make sure this is assigned in the Inspector
    public ScrollRect scrollView;
    public Transform logContent;
    public GameObject logEntryPrefab;
    public Button closeButton;

    // Optional: Reference to the CombatManager if needed
    private CombatManager combatManager;

    void Start()
    {
        // Initially hide the log panel
        if (combatLogPanel != null)
        {
            combatLogPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("CombatLogUI: combatLogPanel GameObject is not assigned in the Inspector!");
        }

        // Attach listener to the close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideLogPanel);
        }
        else
        {
            Debug.LogError("CombatLogUI: CloseButton not assigned in CombatLogUI!");
        }

        // Find the CombatManager (you might have a better way to get this reference)
        combatManager = CombatManager.Instance;
        if (combatManager == null)
        {
            Debug.LogError("CombatManager not found!");
        }
    }

    public void ShowLogPanel()
    {
        if (combatLogPanel != null)
        {
            combatLogPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("CombatLogUI: combatLogPanel GameObject is not assigned in the Inspector!");
        }
        // Optionally, you might want to scroll to the bottom when showing the log
        if (scrollView != null)
        {
            Canvas.ForceUpdateCanvases(); // Ensure layout is updated before scrolling
            scrollView.verticalNormalizedPosition = 0f; // Scroll to bottom (assuming vertical layout)
        }
    }

    public void HideLogPanel()
    {
        if (combatLogPanel != null)
        {
            combatLogPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("CombatLogUI: combatLogPanel GameObject is not assigned in the Inspector!");
        }
    }

    public void AddLogMessage(string message)
    {
        if (logEntryPrefab != null && logContent != null)
        {
            GameObject newEntryGO = Instantiate(logEntryPrefab, logContent);
            TextMeshProUGUI logText = newEntryGO.GetComponentInChildren<TextMeshProUGUI>();
            if (logText != null)
            {
                logText.text = message;
            }
            else
            {
                Debug.LogError("CombatLogList prefab does not contain a TextMeshProUGUI component in its children!");
            }

            // Optionally, scroll to the bottom after adding a new message
            if (scrollView != null)
            {
                // Wait for the end of the frame so layout updates before scrolling
                StartCoroutine(ScrollToBottomEndOfFrame());
            }
        }
        else
        {
            Debug.LogError("Log Entry Prefab or Log Content not assigned in CombatLogUI!");
        }
    }

    private System.Collections.IEnumerator ScrollToBottomEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        scrollView.verticalNormalizedPosition = 0f;
    }
}