using UnityEngine;

[System.Serializable] // Make it serializable for potential saving/debugging
public class ActiveEffect
{
    public EffectConfig config;
    public int remainingTurns;

    public ActiveEffect(EffectConfig config, int remainingTurns)
    {
        this.config = config;
        this.remainingTurns = remainingTurns;
    }
}