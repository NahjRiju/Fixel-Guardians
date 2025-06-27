using UnityEngine;

[CreateAssetMenu(fileName = "EffectConfig", menuName = "Game/Effect Config")]
public class EffectConfig : ScriptableObject
{
    public enum EffectType
    {
        //Buff
        DamageIncrease,     // CGI Object
        Heal,               // VC Object 
        Lifesteal,          // MP Object
        Regeneration,       // SMA Object
        CriticalStrike,     // CK Object
        Shield,             // Fix Object 
        Thorns,             // Panel Object
        //Debuff
        Stun,               // BT Object
        Weakness,           // DC Object
        Poison,             // MCP Object
        Burn,               // PM Object 
        Hallucination,      // Node Object
        Freeze,             // Choose Object
        Impact        // ChromaObject
    }
    public enum EffectCategory { Buff, Debuff }

    public enum TriggerTiming
    {
        StartOfTurn,
        OnAttack,
        EndOfTurn,
        OnTakeDamage
    }

    [Header("Effect Settings")]
    public string effectName;
    public Sprite effectIcon;
    [TextArea] public string learningDescription;
    public EffectType effectType;
    public EffectCategory effectCategory; // Field to distinguish buff/debuff
    public TriggerTiming triggerTiming;   // When the effect should be applied/triggered
    public float effectValue;
    public int effectTurns;


    // Particle effect prefab
    [Header("Visual Effect")]
    public GameObject statusEffectPrefab;  // Drag your VFX prefab here in the Inspector

    [Header("Audio")]
    public AudioClip effectSound;
    public UnityEngine.Audio.AudioMixerGroup audioMixerGroup;
    public float soundVolume = 1f;

}