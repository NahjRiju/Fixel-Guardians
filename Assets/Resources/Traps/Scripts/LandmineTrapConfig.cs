using UnityEngine;

[CreateAssetMenu(fileName = "LandmineTrapConfig", menuName = "Traps/Landmine Trap Config")]
public class LandmineTrapConfig : ScriptableObject
{
    public float detectionRadius = 2f;
    public float damage = 20f;
    [Header("Timing")]
    public float explosionDelay = 2f; // seconds between detection and explosion
    


    public GameObject explosionEffectPrefab;
    public GameObject landminePrefab; 
    public AudioClip detectionSound;  
    public AudioClip explosionSound;
}
