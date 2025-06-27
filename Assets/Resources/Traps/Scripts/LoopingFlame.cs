using UnityEngine;
using System.Collections;

public class LoopingFlame : MonoBehaviour
{
    [Header("Trap Config")]
    public FlamethrowerTrapConfig config;

    private AudioSource audioSource;
    private ParticleSystem flameVFX;
    private Collider flameCollider;
    private bool isFlameOn = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        flameVFX = GetComponentInChildren<ParticleSystem>();
        flameCollider = GetComponent<Collider>();

        if (audioSource != null && config.flameSound != null)
        {
            audioSource.clip = config.flameSound;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }

        if (flameCollider != null)
            flameCollider.enabled = false;

        if (flameVFX != null)
            flameVFX.Stop();
    }

    private void Start()
    {
        StartCoroutine(FlameLoop());
    }

    private IEnumerator FlameLoop()
    {
        while (true)
        {
            ActivateFlame();
            yield return new WaitForSeconds(config.flameDuration);

            DeactivateFlame();
            yield return new WaitForSeconds(config.cooldown);
        }
    }

    private void ActivateFlame()
    {
        isFlameOn = true;

        if (flameVFX != null) flameVFX.Play();
        if (audioSource != null) audioSource.Play();
        if (flameCollider != null) flameCollider.enabled = true;
    }

    private void DeactivateFlame()
    {
        isFlameOn = false;

        if (flameVFX != null) flameVFX.Stop();
        if (audioSource != null) audioSource.Stop();
        if (flameCollider != null) flameCollider.enabled = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isFlameOn) return;

        if (other.CompareTag("Player"))
        {
            HealthComponent health = other.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.ApplyDamage(config.damagePerSecond * Time.deltaTime);
            }
        }
    }
}
