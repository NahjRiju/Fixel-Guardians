using UnityEngine;
using TMPro;

public class FragmentUI : MonoBehaviour
{
    public TMP_Text currentCountText;
    public TMP_Text requiredCountText;
    public ObjectiveManager objectiveManager; // Reference to ObjectiveManager
    public string objectiveName = "Collect CGI Knowledge Fragments"; // The Objective name

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (objectiveManager != null && currentCountText != null && requiredCountText != null)
        {
            // Find the objective data
            LevelConfig.ObjectiveData objective = FindObjectiveData();

            if (objective.objectiveName != null)
            {
                currentCountText.text = "" + objective.currentCount;
                requiredCountText.text = "/ " + objective.requiredCount;
            }
            else
            {
                Debug.LogError("FragmentUI: Objective not found!");
            }
        }
    }

    private LevelConfig.ObjectiveData FindObjectiveData()
    {
        if (objectiveManager.levelConfig != null && objectiveManager.levelConfig.objectives != null)
        {
            foreach (LevelConfig.ObjectiveData objective in objectiveManager.levelConfig.objectives)
            {
                if (objective.objectiveName == objectiveName)
                {
                    return objective;
                }
            }
        }
        return new LevelConfig.ObjectiveData(); // Return empty if not found
    }
}