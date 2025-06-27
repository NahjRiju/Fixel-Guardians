using UnityEngine;
using System.Collections;

public class Spike : MonoBehaviour
{
    public SpikeTrapConfig config;

    private AudioSource audioSource;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isRising = false;
    private bool isActive = false;
    private float cooldownTime;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        startPos = transform.position;
        targetPos = startPos + Vector3.up * config.riseHeight;
        transform.position = startPos;
    }

    private void Start()
    {
        if (config.isLooping)
        {
            InvokeRepeating(nameof(Activate), config.delayBeforeSpike, config.loopDelay);
        }
        else
        {
            Invoke(nameof(Activate), config.delayBeforeSpike);
        }
    }

    public void Activate()
    {
        if (Time.time < cooldownTime || isRising || isActive) return;

        cooldownTime = Time.time + config.cooldown;
        StartCoroutine(RiseAndRetract());
    }

    private IEnumerator RiseAndRetract()
    {
        if (audioSource != null && config.spikeSound != null)
        {
            audioSource.PlayOneShot(config.spikeSound);
        }

        isRising = true;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * config.riseSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        isRising = false;
        isActive = true;

        yield return new WaitForSeconds(config.spikeDuration);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * config.riseSpeed;
            transform.position = Vector3.Lerp(targetPos, startPos, t);
            yield return null;
        }

        isActive = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            HealthComponent health = other.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.ApplyDamage(config.damage);
                isActive = false; // Optional: prevent rapid repeated damage
            }
        }
    }
}
