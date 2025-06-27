    using UnityEngine;

    public class LocationTrigger : MonoBehaviour
    {
        public string locationName; // Name of the location (should match conditionParameter)
        public ObjectiveConditionManager objectiveConditionManager; // Reference to ObjectiveConditionManager

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) // Assuming your player has the "Player" tag
            {
                if (objectiveConditionManager != null)
                {
                    objectiveConditionManager.CheckObjectiveConditions(LevelConfig.ObjectiveConditionType.ReachLocation, locationName);
                }
                else
                {
                    Debug.LogError("LocationTrigger: objectiveConditionManager is not assigned.");
                }
            }
        }
    }