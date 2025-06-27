// LevelLearningData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelLearning", menuName = "Almanac/LevelLearning")]
public class LevelLearningData : ScriptableObject
{
    public string learningName;
    public Sprite learningImage;
    [TextArea] public string learningDescription;
    // Add other relevant data (e.g., level name, related concepts)
}