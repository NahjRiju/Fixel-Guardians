using UnityEngine;

public class LandmineTrapSpawner : MonoBehaviour
{
    public LandmineTrapConfig config;

    private void Start()
    {
        GameObject mine = Instantiate(config.landminePrefab, transform.position, transform.rotation);
        Landmine landmineScript = mine.GetComponent<Landmine>();
        landmineScript?.Initialize(config);

    }

     private void OnDrawGizmosSelected()
    {
        if (config == null) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.4f); // Semi-transparent red
        Gizmos.DrawWireSphere(transform.position, config.detectionRadius);
    }
}
