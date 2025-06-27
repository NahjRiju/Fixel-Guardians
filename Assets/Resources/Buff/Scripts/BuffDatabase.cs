using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For .FirstOrDefault()

[CreateAssetMenu(fileName = "BuffDatabase", menuName = "Game Data/Buff Database")]
public class BuffDatabase : ScriptableObject
{
    public List<EffectConfig> allAvailableBuffs; // Drag all your EffectConfig Scriptable Objects here in the Inspector

    // Helper method to get an EffectConfig by its name
    public EffectConfig GetBuffByName(string buffName)
    {
        if (allAvailableBuffs == null)
        {
            Debug.LogError("BuffDatabase: allAvailableBuffs list is null!");
            return null;
        }
        return allAvailableBuffs.FirstOrDefault(buff => buff.effectName == buffName);
    }
}