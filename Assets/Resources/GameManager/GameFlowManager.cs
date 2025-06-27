using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public LevelConfig levelConfig;

    [Header("Manager Dependencies")]
    public DialogueManager dialogueManager;
    public ObjectiveManager objectiveManager;
    public AlmanacManager almanacManager; // Add this line

    public void StartGamePlay()
    {
        Debug.Log("GamePlay Started");

        if (dialogueManager != null && levelConfig != null && levelConfig.introCharacterDialogue != null && levelConfig.introCharacterDialogue.Length > 0)
        {
            dialogueManager.SetupDialogue(levelConfig.introCharacterDialogue); // Pass the array
            dialogueManager.StartDialogue();
            dialogueManager.OnDialogueFinished += OnIntroDialogueFinished;
        }
        else
        {
            Debug.LogError("GameFlowManager: Missing references or intro character dialogue data.");
        }
    }

    private void OnIntroDialogueFinished()
    {
        dialogueManager.OnDialogueFinished -= OnIntroDialogueFinished;

        if (objectiveManager != null)
        {
            objectiveManager.DisplayObjectives();
        }
        else
        {
            Debug.LogError("GameFlowManager: ObjectiveManager is not assigned.");
        }
    }

    public void StopGamePlay()
    {
        Debug.Log("GamePlay Stopped");
    }
}