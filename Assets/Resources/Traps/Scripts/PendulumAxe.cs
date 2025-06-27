using UnityEngine;

public class PendulumAxe : MonoBehaviour
{
    public PendulumAxeConfig config;

    private float timeCounter = 0f;
    private Quaternion initialRotation;
    private AudioSource audioSource;
    private bool soundPlayedThisSwing = false;

    private void Start()
    {
        initialRotation = transform.localRotation;

        // Apply swing start offset
        switch (config.startPosition)
        {
            case SwingStartPosition.Center:
                timeCounter = 0f;
                break;
            case SwingStartPosition.Left:
                timeCounter = -Mathf.PI / 2f;
                break;
            case SwingStartPosition.Right:
                timeCounter = Mathf.PI / 2f;
                break;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && config.swingSound != null)
        {
            audioSource.clip = config.swingSound;
            audioSource.loop = false;
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        timeCounter += Time.deltaTime * config.swingSpeed;
        float angle = config.swingAngle * Mathf.Sin(timeCounter);

        //transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, angle);

        transform.localRotation = initialRotation * Quaternion.Euler(angle, 0f, 0f);


        // Optional swing sound at peak
        if (Mathf.Abs(angle) >= config.swingAngle - 1f)
        {
            if (!soundPlayedThisSwing && audioSource != null)
            {
                audioSource.Play();
                soundPlayedThisSwing = true;
            }
        }
        else
        {
            soundPlayedThisSwing = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HealthComponent health = other.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.ApplyDamage(config.damage);
            }
        }
    }
}
