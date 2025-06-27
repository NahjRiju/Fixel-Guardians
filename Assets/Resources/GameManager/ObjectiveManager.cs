using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    public LevelConfig levelConfig;
    public Transform objectiveListContent;
    public GameObject objectivePanel;

    public Slider progressBar;
    public TMP_Text progressText;
    public GameObject objectiveMarkerPrefab;
    public Sprite raceFlagSprite;

    private Dictionary<string, bool> objectiveCompletionStatus = new Dictionary<string, bool>();
    private GameObject currentWorldMarker;
    private List<GameObject> restrictedZones = new List<GameObject>();
    private int currentObjectiveIndex = 0;
    private GameStateManager gameStateManager;

    [Header("Managers")] // Added a header for organization
    public DialogueManager dialogueManager; // Public variable to link in the Inspector

    void Start()
    {
        gameStateManager = FindObjectOfType<GameStateManager>();
        UpdateProgressBar();
        DisplayCurrentWorldMarker();
        InstantiateAllRestrictedZones(); // Instantiate all zones at the start
        //UpdateRestrictedZoneVisibility(); // Removed this line
    }

    public void DisplayObjectives()
    {
        Debug.Log("Display Objectives was called.");

        foreach (Transform child in objectiveListContent)
        {
            Destroy(child.gameObject);
        }

        objectiveCompletionStatus.Clear();

        if (levelConfig != null && levelConfig.objectives != null && levelConfig.objectiveListItemPrefab != null)
        {
            for (int i = 0; i < levelConfig.objectives.Length; i++)
            {
                LevelConfig.ObjectiveData objective = levelConfig.objectives[i];
                GameObject listItem = Instantiate(levelConfig.objectiveListItemPrefab, objectiveListContent);
                TMP_Text objectiveText = listItem.GetComponentInChildren<TMP_Text>();
                Toggle objectiveToggle = listItem.GetComponentInChildren<Toggle>();

                if (objectiveText != null)
                {
                    objectiveText.text = objective.objectiveName;
                }

                if (objectiveToggle != null)
                {
                    objectiveToggle.isOn = objective.isCompleted;
                }

                objectiveCompletionStatus[objective.objectiveName] = objective.isCompleted;

                Debug.Log("Objective Index: " + i + ", Objective Name: " + objective.objectiveName + ", Completed: " + objective.isCompleted);
            }
        }
        else
        {
            Debug.LogError("ObjectiveManager: LevelConfig or objectives or objectiveListItemPrefab is not assigned.");
        }

        if (objectivePanel != null)
        {
            objectivePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("ObjectiveManager: objectivePanel is not assigned.");
        }

        UpdateProgressBar();
        DisplayCurrentWorldMarker();
        //UpdateRestrictedZoneVisibility(); // Removed this line
    }

    public void ShowObjectivePanel()
    {
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("ObjectiveManager: objectivePanel is not assigned.");
        }
    }

    public void HideObjectivePanel()
    {
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("ObjectiveManager: objectivePanel is not assigned.");
        }
    }

    public void UpdateObjectiveUI(string objectiveName, bool isCompleted)
    {
        if (objectiveCompletionStatus.ContainsKey(objectiveName))
        {
            objectiveCompletionStatus[objectiveName] = isCompleted;

            foreach (Transform child in objectiveListContent)
            {
                TMP_Text objectiveText = child.GetComponentInChildren<TMP_Text>();
                Toggle objectiveToggle = child.GetComponentInChildren<Toggle>();

                if (objectiveText != null && objectiveText.text == objectiveName && objectiveToggle != null)
                {
                    objectiveToggle.isOn = isCompleted;
                    break;
                }
            }

            UpdateProgressBar();
            UpdateCurrentObjectiveIndex();
            DisplayCurrentWorldMarker();
            DisableCompletedRestrictedZone(); // Disable the completed zone
        }
    }

    private bool CheckAllObjectivesCompleted()
    {
        if (levelConfig != null && levelConfig.objectives != null)
        {
            foreach (LevelConfig.ObjectiveData objective in levelConfig.objectives)
            {
                if (!objective.isCompleted)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    private void UpdateProgressBar()
    {
        if (progressBar != null && levelConfig != null && levelConfig.objectives != null)
        {
            int completedObjectives = 0;
            foreach (LevelConfig.ObjectiveData objective in levelConfig.objectives)
            {
                if (objective.isCompleted)
                {
                    completedObjectives++;
                }
            }

            float progress = (float)completedObjectives / levelConfig.objectives.Length;
            progressBar.value = progress;

            if (progressText != null)
            {
                progressText.text = $"{completedObjectives}/{levelConfig.objectives.Length}";
            }

            UpdateObjectiveMarkers();
        }
    }

    private void UpdateObjectiveMarkers()
    {
        ClearObjectiveMarkers();

        if (levelConfig == null || levelConfig.objectives == null || objectiveMarkerPrefab == null || progressBar == null) return;

        float progressBarWidth = progressBar.GetComponent<RectTransform>().rect.width;
        float markerSpacing = progressBarWidth / levelConfig.objectives.Length;

        GameObject firstMarker = Instantiate(objectiveMarkerPrefab, progressBar.transform);
        firstMarker.name = $"ObjectiveMarker_Start";

        if (firstMarker.TryGetComponent(out RectTransform firstMarkerRect))
        {
            firstMarkerRect.anchoredPosition = new Vector2(0, 0);
            firstMarkerRect.anchorMin = new Vector2(0, 0.5f);
            firstMarkerRect.anchorMax = new Vector2(0, 0.5f);
            firstMarkerRect.pivot = new Vector2(0, 0.5f);
        }

        if (firstMarker.TryGetComponent(out Image firstMarkerImage))
        {
            firstMarkerImage.enabled = false;
        }

        for (int i = 0; i < levelConfig.objectives.Length; i++)
        {
            float markerPosition = (i + 1) * markerSpacing;
            markerPosition -= 20f;

            GameObject marker = Instantiate(objectiveMarkerPrefab, progressBar.transform);
            marker.name = $"ObjectiveMarker_{i}";

            if (marker.TryGetComponent(out RectTransform markerRect))
            {
                markerRect.anchoredPosition = new Vector2(markerPosition, 0);
                markerRect.anchorMin = new Vector2(0, 0.5f);
                markerRect.anchorMax = new Vector2(0, 0.5f);
                markerRect.pivot = new Vector2(0, 0.5f);
            }

            if (marker.TryGetComponent(out Image markerImage))
            {
                if (i == levelConfig.objectives.Length - 1 && raceFlagSprite != null)
                {
                    markerImage.sprite = raceFlagSprite;
                }
                else
                {
                    markerImage.color = levelConfig.objectives[i].isCompleted ? Color.green : Color.gray;
                }
            }
        }
    }

    private void ClearObjectiveMarkers()
    {
        foreach (Transform child in progressBar.transform)
        {
            if (child.name.StartsWith("ObjectiveMarker"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void DisplayCurrentWorldMarker()
    {
        if (currentWorldMarker != null)
        {
            Destroy(currentWorldMarker);
        }

        if (levelConfig != null && levelConfig.objectives != null && currentObjectiveIndex < levelConfig.objectives.Length)
        {
            LevelConfig.ObjectiveData currentObjective = levelConfig.objectives[currentObjectiveIndex];
            if (currentObjective.objectiveMarkerPrefab != null)
            {
                currentWorldMarker = Instantiate(currentObjective.objectiveMarkerPrefab, currentObjective.objectiveWorldPosition, Quaternion.identity);
            }
        }
    }

    private void InstantiateAllRestrictedZones()
    {
        if (levelConfig != null && levelConfig.objectives != null)
        {
            for (int i = 0; i < levelConfig.objectives.Length; i++)
            {
                LevelConfig.ObjectiveData objective = levelConfig.objectives[i];
                if (objective.restrictedZonePrefab != null)
                {
                    GameObject restrictedZone = Instantiate(objective.restrictedZonePrefab, objective.restrictedZonePosition, Quaternion.Euler(objective.restrictedZoneRotation)); // Apply rotation here
                    if (restrictedZone.TryGetComponent(out BoxCollider boxCollider))
                    {
                        boxCollider.size = objective.restrictedZoneSize;
                    }
                    restrictedZones.Add(restrictedZone);
                }
                else
                {
                    restrictedZones.Add(null);
                }
            }
        }
    }

    private void DisableCompletedRestrictedZone()
    {
        if (currentObjectiveIndex > 0 && restrictedZones.Count > 0)
        {
            if (restrictedZones[currentObjectiveIndex - 1] != null)
            {
                restrictedZones[currentObjectiveIndex - 1].SetActive(false);
            }
        }
    }

    private void UpdateCurrentObjectiveIndex()
    {
        if (levelConfig != null && levelConfig.objectives != null)
        {
            for (int i = 0; i < levelConfig.objectives.Length; i++)
            {
                if (!levelConfig.objectives[i].isCompleted)
                {
                    currentObjectiveIndex = i;
                    return;
                }
            }

            currentObjectiveIndex = levelConfig.objectives.Length;
        }
    }

    // Public method to set the DialogueManager reference
    public void SetDialogueManager(DialogueManager dm)
    {
        dialogueManager = dm;
    }
}