using UnityEngine;

public class AlmanacEntry
{
    public ScriptableObject data;
    public Sprite image;
    public string name;
    public bool isUnlocked; // Added this line

    public AlmanacEntry(ScriptableObject data, Sprite image, string name, bool isUnlocked = false) // Added this line
    {
        this.data = data;
        this.image = image;
        this.name = name;
        this.isUnlocked = isUnlocked; // Added this line
    }
}