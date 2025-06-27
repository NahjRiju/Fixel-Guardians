using UnityEngine;

[CreateAssetMenu(fileName = "LevelLootConfig", menuName = "Game/Level Loot Config")]
public class LevelLootConfig : ScriptableObject
{
    public CombatZoneLoot[] zoneLoot;
}