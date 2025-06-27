using UnityEngine;

[CreateAssetMenu(fileName = "PoisonGasTrapConfig", menuName = "Traps/Poison Gas Trap Config")]
public class PoisonGasTrapConfig : ScriptableObject
{
    public float gasDuration = 5f;
    public float damagePerSecond = 10f;
    public float expansionSpeed = 1f;
    public float maxRadius = 5f;

    public bool autoStartOnAwake = false;

    public GameObject gasVFXPrefab;

    [Header("Looping")]
    public bool loopGas = false;
    public float loopDelay = 3f; 
    [Header("Fade Settings")]
    public float fadeOutDuration = 1f; 

    [Header("Audio")]
    public AudioClip hissSound;



}
