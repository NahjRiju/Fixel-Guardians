using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Required for Coroutines

public class ObjectiveConditionManager : MonoBehaviour
{
    public LevelConfig levelConfig;
    public ObjectiveManager objectiveManager;
    public DialogueManager dialogueManager;
    public AlmanacManager almanacManager;
    public GameStateManager gameStateManager; // Add reference to GameStateManager

    private Dictionary<string, bool> objectiveConditions = new Dictionary<string, bool>();
    private bool lastObjectiveCompleted = false;

    void Start()
    {
        InitializeObjectiveConditions();
    }

    void InitializeObjectiveConditions()
    {
        if (levelConfig != null && levelConfig.objectives != null)
        {
            foreach (LevelConfig.ObjectiveData objective in levelConfig.objectives)
            {
                objectiveConditions[objective.objectiveName] = false;
            }
        }
    }

    public void CheckObjectiveConditions(LevelConfig.ObjectiveConditionType conditionType, string conditionParameter)
    {
        Debug.Log($"ObjectiveConditionManager: Checking Objective Conditions. Type={conditionType}, Parameter={conditionParameter}");
        if (levelConfig != null && levelConfig.objectives != null)
        {
            for (int i = 0; i < levelConfig.objectives.Length; i++)
            {
                LevelConfig.ObjectiveData objective = levelConfig.objectives[i];
                if (objective.conditionType == conditionType && objective.conditionParameter == conditionParameter)
                {
                    if (conditionType == LevelConfig.ObjectiveConditionType.CollectItem && conditionParameter == conditionParameter)
                    {
                        objective.currentCount++;
                        levelConfig.objectives[i].currentCount = objective.currentCount;

                        if (objective.currentCount >= objective.requiredCount)
                        {
                            CompleteObjective(i);
                            return;
                        }
                    }
                    else
                    {
                        CompleteObjective(i);
                        return;
                    }
                }
            }
        }
    }

    private void CompleteObjective(int objectiveIndex)
    {
        LevelConfig.ObjectiveData objective = levelConfig.objectives[objectiveIndex];
        if (!objective.isCompleted)
        {
            objective.isCompleted = true;
            levelConfig.objectives[objectiveIndex].isCompleted = true;
            objectiveConditions[objective.objectiveName] = true;

            objectiveManager.UpdateObjectiveUI(objective.objectiveName, objective.isCompleted);

            if (objective.completionDialogue != null && objective.completionDialogue.Length > 0 && dialogueManager != null)
            {
                dialogueManager.OnDialogueFinished += HandleLastObjectiveDialogueFinished; // Subscribe to the event
                dialogueManager.SetupDialogue(objective.completionDialogue);
                dialogueManager.StartDialogue();
                if (objectiveIndex == levelConfig.objectives.Length - 1)
                {
                    lastObjectiveCompleted = true; // Set this AFTER starting the dialogue
                }
            }
            else
            {
                // If no dialogue, or DialogueManager is missing, check for win condition immediately
                if (objectiveIndex == levelConfig.objectives.Length - 1)
                {
                    lastObjectiveCompleted = true;
                    StartCoroutine(ShowWinPanelWithDelay()); // Start the coroutine for the delay
                }
            }

            if (objective.learningEntriesToUnlock != null && almanacManager != null)
            {
                foreach (string entryName in objective.learningEntriesToUnlock)
                {
                    almanacManager.UnlockAlmanacLearningEntry(entryName);
                    Debug.Log("AlmanacManager: Learning entry '" + entryName + "' unlocked.");
                }
            }

            // Unsubscribe in case this isn't the last objective with dialogue
            if (dialogueManager != null && objectiveIndex < levelConfig.objectives.Length - 1 && objective.completionDialogue != null && objective.completionDialogue.Length > 0)
            {
                dialogueManager.OnDialogueFinished -= HandleLastObjectiveDialogueFinished;
            }
        }
        // Check win condition if this was the last objective and no dialogue was played
        else if (objectiveIndex == levelConfig.objectives.Length - 1 && (objective.completionDialogue == null || objective.completionDialogue.Length == 0 || dialogueManager == null))
        {
            lastObjectiveCompleted = true;
            StartCoroutine(ShowWinPanelWithDelay()); // Start the coroutine for the delay
        }
    }

    private void HandleLastObjectiveDialogueFinished()
    {
        if (lastObjectiveCompleted && dialogueManager != null)
        {
            dialogueManager.OnDialogueFinished -= HandleLastObjectiveDialogueFinished; // Unsubscribe to avoid multiple calls
            StartCoroutine(ShowWinPanelWithDelay()); // Start the coroutine for the delay
        }
    }

    private IEnumerator ShowWinPanelWithDelay()
    {
        yield return new WaitForSeconds(6f);
        if (gameStateManager != null)
        {
            gameStateManager.SetGameState(GameStateManager.GameState.Won);
        }
        else
        {
            Debug.LogError("ObjectiveConditionManager: GameStateManager not found.");
        }
    }
}