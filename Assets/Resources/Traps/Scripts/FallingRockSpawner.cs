using UnityEngine;
using System.Collections;

public class FallingRockSpawner : MonoBehaviour
{
    public FallingRockConfig config;
    private HealthComponent playerHealth;
    private bool isSpawning = false;
    private GameObject currentRock = null;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<HealthComponent>();
        }
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRockLoop());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    private IEnumerator SpawnRockLoop()
    {
        while (isSpawning)
        {
            if (currentRock == null)
            {
                Vector3 basePosition = transform.position;

                // Scatter offset
                Vector3 randomOffset = new Vector3(
                    Random.Range(-config.scatterAmount, config.scatterAmount),
                    0f,
                    Random.Range(-config.scatterAmount, config.scatterAmount)
                );

                Vector3 spawnPosition = basePosition + randomOffset;

                currentRock = Instantiate(config.rockPrefab, spawnPosition, Quaternion.identity);
                var rockScript = currentRock.GetComponent<FallingRock>();
                rockScript.Initialize(config, playerHealth, OnRockDestroyed);
            }

            yield return new WaitForSeconds(4f);
        }
    }

    // Callback to clear reference when the rock is destroyed
    private void OnRockDestroyed()
    {
        currentRock = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (config != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, config.scatterAmount);
        }
    }
}
