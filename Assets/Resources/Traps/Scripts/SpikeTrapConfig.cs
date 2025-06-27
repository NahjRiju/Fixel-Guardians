using UnityEngine;

[CreateAssetMenu(fileName = "SpikeTrapConfig", menuName = "Traps/Spike Trap Config")]
public class SpikeTrapConfig : ScriptableObject
{
    public float damage = 10f;
    public float delayBeforeSpike = 0.5f;
    public float spikeDuration = 1f;
    public float cooldown = 2f;
    
    public float riseHeight = 1f;           // How high it pops up
    public float riseSpeed = 5f;            // How fast it pops up

    public AudioClip spikeSound;
    public GameObject spikePrefab;

    [Header("Looping Behavior")]
    public bool isLooping = false;
    public float loopDelay = 2f;
}
