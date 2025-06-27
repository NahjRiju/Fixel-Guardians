using UnityEngine;

public class FallingRock : MonoBehaviour
{
    private AudioSource audioSource;
    private Rigidbody rb;
    private HealthComponent playerHealth;

    private bool hasExploded = false;
    private float lifeTimer = 0f;
    private bool hasStartedFalling = false;
    private FallingRockConfig config;

    private System.Action onDestroyedCallback;


    public void Initialize(FallingRockConfig config, HealthComponent playerHealth, System.Action onDestroyed = null)
    {
        this.config = config;
        this.playerHealth = playerHealth;
        this.onDestroyedCallback = onDestroyed;

        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.velocity = Vector3.down * config.fallSpeed;
        }

        if (audioSource != null && config.warningSound != null)
        {
            audioSource.PlayOneShot(config.warningSound);
        }

        hasStartedFalling = true;
    }


    private void Update()
    {
        if (hasExploded || !hasStartedFalling) return;

        lifeTimer += Time.deltaTime;

        if (rb != null && rb.velocity.magnitude < 0.1f && lifeTimer > 0.5f)
        {
            Explode();
        }

        if (lifeTimer >= config.maxLifetime)
        {
            Explode();
        }

        transform.Rotate(Vector3.forward * config.rotationSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            HealthComponent health = collision.gameObject.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.ApplyDamage(config.damage);
            }
        }

        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (audioSource != null && config.impactSound != null)
        {
            audioSource.PlayOneShot(config.impactSound);
        }

        if (config.explosionEffectPrefab != null)
        {
            Instantiate(config.explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, config.explosionRadius);
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
        onDestroyedCallback?.Invoke();

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (config != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, config.explosionRadius);
        }
    }
}
