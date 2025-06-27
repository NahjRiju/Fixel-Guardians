//BuffEffectData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuffEffectData", menuName = "Almanac/BuffEffectData")]
public class BuffEffectData : ScriptableObject
{
    public string  effectName;
    public Sprite effectIcon;
    [TextArea] public string effectDescription;
    // Add other relevant data (e.g., level name, related concepts)
}
