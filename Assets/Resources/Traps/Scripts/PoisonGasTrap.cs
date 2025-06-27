using UnityEngine;
using System.Collections;

public class PoisonGasTrap : MonoBehaviour
{
    public PoisonGasTrapConfig config;

    private GameObject gasInstance;
    private SphereCollider gasCollider;
    private bool isActive = false;

    private void Start()
    {
        if (config.autoStartOnAwake)
        {
            ActivateGas();
        }
    }

    public void ActivateGas()
    {
        if (isActive || config == null) return;
        StartCoroutine(GasCycle());

        if (config.hissSound != null)
        {
            AudioSource.PlayClipAtPoint(config.hissSound, transform.position);
        }

    }

    private IEnumerator GasCycle()
    {
        isActive = true;

        // ✅ Spawn gas VFX exactly as it looks in the prefab
        gasInstance = Instantiate(config.gasVFXPrefab, transform.position, Quaternion.identity, transform);

        // ✅ Create detection collider based on config radius
        gasCollider = gameObject.AddComponent<SphereCollider>();
        gasCollider.isTrigger = true;
        gasCollider.radius = config.maxRadius;

        // Wait for gas duration
        yield return new WaitForSeconds(config.gasDuration);

        // Cleanup
        if (gasInstance != null)
        {
            StartCoroutine(FadeOutAndDestroy(gasInstance, config.fadeOutDuration));
        }
        if (gasCollider) Destroy(gasCollider);

        isActive = false;

        // 🔁 Loop if enabled
        if (config.loopGas)
        {
            yield return new WaitForSeconds(config.loopDelay);
            ActivateGas();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            HealthComponent health = other.GetComponent<HealthComponent>();
            if (health != null)
            {
                float damage = config.damagePerSecond * Time.deltaTime;
                health.ApplyDamage(damage);
            }
        }
    }

   private IEnumerator FadeOutAndDestroy(GameObject gasObject, float fadeDuration)
    {
        if (gasObject == null) yield break;

        ParticleSystem[] systems = gasObject.GetComponentsInChildren<ParticleSystem>();

        foreach (var ps in systems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // Wait for particles to fade naturally based on Start Lifetime and Color Over Lifetime
        yield return new WaitForSeconds(fadeDuration);

        Destroy(gasObject);
}





    private void OnDrawGizmosSelected()
    {
        if (config == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, config.maxRadius);
    }
}
