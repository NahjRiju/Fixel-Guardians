using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectID; // Unique ID for this interactable object
    public ObjectiveConditionManager objectiveConditionManager; // Reference to ObjectiveConditionManager
    public bool isObjectiveInteractable = false; // Flag for objective-related interactables

    private bool playerInRange = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public void Interact()
    {
        Debug.Log("Interact() called on " + gameObject.name + ", playerInRange: " + playerInRange + ", objectID: " + objectID + ", isObjectiveInteractable: " + isObjectiveInteractable);
        if (playerInRange && objectiveConditionManager != null)
        {
            if (isObjectiveInteractable)
            {
                objectiveConditionManager.CheckObjectiveConditions(LevelConfig.ObjectiveConditionType.InteractObject, objectID);
            }
            else
            {
                Debug.Log("InteractableObject: General interaction with " + gameObject.name);
                // Add code for general interaction here (e.g., open door, activate lever)
            }
        }
        else if (!playerInRange)
        {
            Debug.LogWarning("InteractableObject: Player not in range.");
        }
        else if (objectiveConditionManager == null)
        {
            Debug.LogError("InteractableObject: objectiveConditionManager is not assigned.");
        }
    }
}