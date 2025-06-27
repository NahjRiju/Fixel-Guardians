using UnityEngine;

public class Landmine : MonoBehaviour
{
    private LandmineTrapConfig config;
    private AudioSource audioSource;
    private bool isTriggered = false;

    public void Initialize(LandmineTrapConfig config)
    {
        this.config = config;
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered || config == null) return;
        if (!other.CompareTag("Player")) return;

        isTriggered = true;

        // ✅ Step 1: play detection sound
        if (audioSource != null && config.detectionSound != null)
        {
            audioSource.PlayOneShot(config.detectionSound);
        }

        // ✅ Step 2: wait before explosion
        Invoke(nameof(Explode), config.explosionDelay);
    }

    private void Explode()
    {
        // ✅ Play explosion sound immediately
        if (audioSource != null && config.explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(config.explosionSound, transform.position);
        }

        // 💥 Spawn explosion VFX
        if (config.explosionEffectPrefab != null)
        {
            Instantiate(config.explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 💥 Deal AoE damage
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, config.detectionRadius);
        foreach (Collider c in hitColliders)
        {
            if (c.CompareTag("Player"))
            {
                HealthComponent health = c.GetComponent<HealthComponent>();
                if (health != null)
                {
                    health.ApplyDamage(config.damage);
                }
            }
        }

        // ✅ Destroy immediately after explosion
        Destroy(gameObject);
    }
}
