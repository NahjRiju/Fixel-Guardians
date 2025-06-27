using UnityEngine;
using System.Collections;

public class LaserBeam : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private LaserTrapConfig config;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private Transform playerEffectAnchor; // 🔥 Drag in Inspector

    private AudioSource humAudio;
    private AudioSource hitAudio;

    private Vector3 startPos;
    private Vector3 endPos;
    private bool movingForward = true;
    private bool isPlayerInside = false; // Prevents double-triggering

    private Coroutine stopHitAudioCoroutine = null;


    private void Awake()
    {
        // Get both AudioSources on this object
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            humAudio = sources[0];  // Looping hum sound
            hitAudio = sources[1];  // One-shot hit sound
        }
        else
        {
            Debug.LogWarning("LaserBeam needs 2 AudioSources: [0] hum, [1] hit.");
        }

        // Configure hum audio (just in case not set in inspector)
        if (humAudio != null)
        {
            humAudio.loop = true;
            humAudio.playOnAwake = false;
            humAudio.spatialBlend = 1f;
            humAudio.dopplerLevel = 0f;
            humAudio.minDistance = 2f;
            humAudio.maxDistance = 15f;
        }

        if (hitAudio != null)
        {
            hitAudio.playOnAwake = false;
            hitAudio.spatialBlend = 1f;
        }
    }

    private void Start()
    {
        if (config == null)
        {
            Debug.LogWarning("LaserBeam missing LaserTrapConfig.");
            return;
        }

        startPos = transform.position;
        endPos = startPos + Vector3.right * config.moveDistance;

        if (config.isLooping)
            StartCoroutine(MoveLaserPingPong());

      startPos = transform.position;

       Vector3 baseDirection = Vector3.zero;

        switch (config.movementType)
        {
            case LaserTrapConfig.MovementType.Horizontal:
                baseDirection = transform.right;  // move perpendicular to beam
                break;
            case LaserTrapConfig.MovementType.Vertical:
                baseDirection = transform.up;     // move along the beam
                break;
            case LaserTrapConfig.MovementType.OneWay:
                baseDirection = transform.right;
                break;
            case LaserTrapConfig.MovementType.None:
                baseDirection = Vector3.zero;
                break;
        }

        // 🔁 Apply reverse if needed
        if (config.movementDirection == LaserTrapConfig.MovementDirection.Reverse)
        {
            baseDirection *= -1f;
        }

        Vector3 offset = baseDirection * config.moveDistance;
        endPos = startPos + offset;


        if (config.movementType != LaserTrapConfig.MovementType.None)
        {
            if (config.isLooping || config.movementType == LaserTrapConfig.MovementType.OneWay)
                StartCoroutine(MoveLaserPingPong());
        }


    }

    /*  private IEnumerator MoveLaser()
      {
          float t = 0;
          Vector3 from = startPos;
          Vector3 to = endPos;

          while (t < 1)
          {
              t += Time.deltaTime * config.speed;
              transform.position = Vector3.Lerp(from, to, t);
              yield return null;
          }

          // For looped lasers, ping-pong
          if (config.isLooping && config.movementType != LaserTrapConfig.MovementType.OneWay)
          {
              // Reverse and continue
              Vector3 temp = startPos;
              startPos = endPos;
              endPos = temp;
              StartCoroutine(MoveLaser());
          }
      } */
    
    private IEnumerator MoveLaserPingPong()
    {
        while (true)
        {
            yield return MoveToTarget(endPos);
            if (config.movementType == LaserTrapConfig.MovementType.OneWay) break;

            yield return MoveToTarget(startPos);
        }
    }

    private IEnumerator MoveToTarget(Vector3 target)
    {
        Vector3 origin = transform.position;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * config.speed;
            transform.position = Vector3.Lerp(origin, target, t);
            yield return null;
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isPlayerInside)
            return;

        isPlayerInside = true;

        // Cancel delayed stop if player re-enters quickly
        if (stopHitAudioCoroutine != null)
        {
            StopCoroutine(stopHitAudioCoroutine);
            stopHitAudioCoroutine = null;
        }

        if (humAudio != null && !humAudio.isPlaying)
            humAudio.Play();

        if (hitAudio != null)
        {
            hitAudio.Stop();
            hitAudio.Play();
        }

        if (hitEffectPrefab != null && playerEffectAnchor != null)
        {
            GameObject vfx = Instantiate(hitEffectPrefab, playerEffectAnchor.position, Quaternion.identity, playerEffectAnchor);
            Destroy(vfx, 2f);
        }

        HealthComponent health = other.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.ApplyDamage(config.damage);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInside = false;

        if (humAudio != null && humAudio.isPlaying)
            humAudio.Stop();

        if (hitAudio != null && hitAudio.isPlaying)
        {
            stopHitAudioCoroutine = StartCoroutine(DelayedStopHitAudio(0.5f));
        }
    }
    
    private IEnumerator DelayedStopHitAudio(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hitAudio != null && hitAudio.isPlaying)
            hitAudio.Stop();

        stopHitAudioCoroutine = null;
    }


}
