using UnityEngine;

[CreateAssetMenu(fileName = "LootItem", menuName = "Game/Loot Item")]
public class LootItem : ScriptableObject
{
    [Header("General Information")]
    public string itemName;
    public Sprite itemSprite;
    public string itemDescription;

    [Header("Buff Item?")]
    public bool isBuff = false; // New flag to indicate if this is a buff
    public EffectConfig buffEffect; // Optional: Reference to the EffectConfig if it's a buff
    // Add other item properties here
}