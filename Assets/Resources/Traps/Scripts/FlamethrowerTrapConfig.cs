using UnityEngine;

[CreateAssetMenu(fileName = "FlamethrowerTrapConfig", menuName = "Traps/Flamethrower Trap Config")]
public class FlamethrowerTrapConfig : ScriptableObject
{
    [Header("Timing")]
    public float flameDuration = 3f;
    public float cooldown = 2f;

    [Header("Damage")]
    public float damagePerSecond = 10f;

    [Header("Sound")]
    public AudioClip flameSound;
}
