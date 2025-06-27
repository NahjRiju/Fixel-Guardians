using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelConfig", menuName = "Game/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Info")]
    public string levelName;
    public string nextLevelSceneName;
    public Sprite[] cutsceneImages;
    public string[] cutsceneNarratives;
    public AudioClip cutsceneBGM;
    public AudioClip[] cutsceneSFX;

    [Header("Game State Panels")]
    public GameObject startPanelPrefab;
    public GameObject winPanelPrefab;
    public GameObject gameOverPanelPrefab;
    public GameObject cutscenePanelPrefab;
    public GameObject pausePanelPrefab;

    [Header("Game Flow Settings")]

    [Header("Dialogue Data")]
    public DialogueData[] introCharacterDialogue;

    [System.Serializable]
    public class DialogueData
    {
        public string characterName;
        public Sprite characterImage;
        public Vector2 imagePosition = Vector2.zero;
        public Vector3 imageRotation = Vector3.zero;
        public Vector3 imageScale = Vector3.one;
        public DialogueEntry[] dialogueEntries;
    }

    [System.Serializable]
    public class DialogueEntry
    {
        public string dialogueLine;
        public AudioClip voiceClip;
    }

    [Header("Objective Data")]
    public ObjectiveData[] objectives;
    public GameObject objectiveListItemPrefab;

    [System.Serializable]
    public struct ObjectiveData
    {
        public string objectiveName;
        public ObjectiveConditionType conditionType;
        public string conditionParameter;
        public Vector3 objectiveWorldPosition; // Added this line
        public GameObject objectiveMarkerPrefab; // Added this line
        public GameObject restrictedZonePrefab; // Added restricted zone prefab
        public Vector3 restrictedZonePosition; // Added restricted zone position
        public Vector3 restrictedZoneRotation;
        public Vector3 restrictedZoneSize;    // Added restricted zone size

        // For Item Collection
        public int currentCount;
        public int requiredCount;
        public bool isCompleted;

        public DialogueData[] completionDialogue;

        public string[] learningEntriesToUnlock;
    }

    public enum ObjectiveConditionType
    {
        ReachLocation,
        AvoidObstacle,
        CollectItem,
        ClearIllusions,
        ClearCombatZone,
        InteractObject
    }
}