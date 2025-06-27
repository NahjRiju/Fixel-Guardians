using UnityEngine;

public class PlayerTriggerTrap : MonoBehaviour
{
    [Tooltip("Drop any spawner that has a TriggerTrap() method here (e.g., FallingRockSpawner, SpikeTrapSpawner)")]
    public MonoBehaviour[] trapSpawners;
    public bool startSpawners = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (var spawner in trapSpawners)
        {
            if (spawner == null) continue;

            var method = startSpawners ? spawner.GetType().GetMethod("StartSpawning") : spawner.GetType().GetMethod("StopSpawning");
            if (method != null)
            {
                method.Invoke(spawner, null);
                continue;
            }

            var spikeTrigger = spawner.GetType().GetMethod("TriggerSpike");
            if (spikeTrigger != null)
            {
                spikeTrigger.Invoke(spawner, null);
            }
        }

        Destroy(gameObject);
    }
}
