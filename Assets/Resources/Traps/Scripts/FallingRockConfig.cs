using UnityEngine;

[CreateAssetMenu(fileName = "FallingRockConfig", menuName = "Traps/Falling Rock Config")]
public class FallingRockConfig : ScriptableObject
{
    public AudioClip warningSound;
    public AudioClip impactSound;
    public float fallSpeed = 10f;
    public float rotationSpeed = 360f;
    public float spawnInterval = 0.5f;
    public float damage = 10f;
    public float explosionRadius = 4f;
    public float maxLifetime = 5f;
    public GameObject rockPrefab;
    public GameObject explosionEffectPrefab;

    [Header("Optional Scatter Settings")]
    public float scatterAmount = 0f; // 0 = exact position, >0 = random offset
}
