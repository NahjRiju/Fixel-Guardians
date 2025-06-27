using UnityEngine;

[CreateAssetMenu(fileName = "LootTable", menuName = "Game/Loot Table")]
public class LootTable : ScriptableObject
{
    public LootItem[] lootItems;
}