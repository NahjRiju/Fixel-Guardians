using UnityEngine;
using System.Collections.Generic;

public class IllusionGenerator : MonoBehaviour
{
    public GameObject[] illusionObjects;
    public Vector3 areaSize = new Vector3(100f, 10f, 100f);
    public int numberOfObjects = 50;
    public float minScale = 1f;
    public float maxScale = 3f;
    public float minRotation = 0f;
    public float maxRotation = 360f;
    public float minRotationSpeed = 10f;
    public float maxRotationSpeed = 50f;
    public float minMorphSpeed = 0.5f;
    public float maxMorphSpeed = 2f;
    public float colliderChance = 0.8f;
    public float followChance = 0.2f;
    public float followSpeed = 5f;
    public float followDistance = 10f;
    public float destroyDelay = 3f;
    public float damageAmount = 10f; // Damage amount to apply

    private List<GameObject> generatedObjects = new List<GameObject>();
    private Transform playerTransform;
    private HealthComponent playerHealth; // Reference to player's health

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player not found! Make sure the player GameObject has the 'Player' tag.");
        }
        playerHealth = playerTransform?.GetComponent<HealthComponent>(); // Get player health
        if (playerHealth == null && playerTransform != null)
        {
            Debug.LogError("Player health component not found!");
        }
        GenerateIllusions();
    }

    void GenerateIllusions()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            GameObject selectedObject = illusionObjects[Random.Range(0, illusionObjects.Length)];

            Vector3 randomPosition;
            Quaternion randomRotation;
            Vector3 objectScale;

            if (TryGenerateValidPosition(selectedObject, out randomPosition, out randomRotation, out objectScale))
            {
                GameObject newObject = Instantiate(selectedObject, randomPosition, randomRotation);
                newObject.transform.localScale = objectScale;
                newObject.transform.parent = transform;

                Collider col = null;
                if (Random.value < colliderChance)
                {
                    col = AddCollider(newObject);
                }

                if (Random.value < followChance && playerTransform != null)
                {
                    FollowPlayerComponent followComponent = newObject.AddComponent<FollowPlayerComponent>();
                    followComponent.player = playerTransform;
                    followComponent.speed = followSpeed;
                    followComponent.followDistance = followDistance;
                    followComponent.destroyDelay = destroyDelay;
                    followComponent.damage = damageAmount; // Pass damage amount
                    if(col != null) col.isTrigger = true;
                }

                RotationComponent rotationComponent = newObject.AddComponent<RotationComponent>();
                rotationComponent.rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);

                MorphComponent morphComponent = newObject.AddComponent<MorphComponent>();
                morphComponent.morphSpeed = Random.Range(minMorphSpeed, maxMorphSpeed);
                morphComponent.initialScale = objectScale;
                morphComponent.targetScale = new Vector3(Random.Range(minScale, maxScale), Random.Range(minScale, maxScale), Random.Range(minScale, maxScale));

                generatedObjects.Add(newObject);
            }
            else
            {
                Debug.LogWarning("Could not find a valid position for object " + i);
            }
        }
    }

    Collider AddCollider(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();
        if (col == null)
        {
            if (obj.GetComponent<MeshFilter>() != null)
            {
                col = obj.AddComponent<MeshCollider>();
            }
            else
            {
                col = obj.AddComponent<BoxCollider>();
            }
        }
        return col;
    }

    bool TryGenerateValidPosition(GameObject obj, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        int maxAttempts = 100;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            position = transform.position + new Vector3(
                Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                Random.Range(0f, areaSize.y),
                Random.Range(-areaSize.z / 2f, areaSize.z / 2f)
            );

            rotation = Quaternion.Euler(
                Random.Range(minRotation, maxRotation),
                Random.Range(minRotation, maxRotation),
                Random.Range(minRotation, maxRotation)
            );

            scale = new Vector3(Random.Range(minScale, maxScale), Random.Range(minScale, maxScale), Random.Range(minScale, maxScale));

            obj.transform.localScale = scale;

            if (!CheckOverlap(obj, position))
            {
                return true;
            }
        }
        position = Vector3.zero;
        rotation = Quaternion.identity;
        scale = Vector3.one;
        return false;
    }

    bool CheckOverlap(GameObject obj, Vector3 position)
    {
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider == null) return false;

        foreach (GameObject generatedObj in generatedObjects)
        {
            Collider generatedCollider = generatedObj.GetComponent<Collider>();
            if (generatedCollider != null)
            {
                if (Physics.CheckBox(position, objCollider.bounds.extents, Quaternion.identity) && obj != generatedObj)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

public class RotationComponent : MonoBehaviour
{
    public float rotationSpeed = 30f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}

public class MorphComponent : MonoBehaviour
{
    public float morphSpeed = 1f;
    public Vector3 initialScale;
    public Vector3 targetScale;
    private float timer = 0f;

    void Update()
    {
        transform.localScale = Vector3.Lerp(initialScale, targetScale, Mathf.PingPong(timer, 1f));
    }
}

public class FollowPlayerComponent : MonoBehaviour
{
    public Transform player;
    public float speed = 5f;
    public float followDistance = 10f;
    public float destroyDelay = 3f;
    public float damage = 10f; // Damage to apply
    private bool isFollowing = false;
    private float followTimer = 0f;
    private Vector3 lastPlayerPosition;
    private HealthComponent playerHealth;

    void Start()
    {
        playerHealth = player.GetComponent<HealthComponent>(); // Get health on this object
    }

    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= followDistance && !isFollowing)
            {
                isFollowing = true;
                followTimer = 0f;
                lastPlayerPosition = player.position;
            }

            if (isFollowing)
            {
                transform.position = Vector3.MoveTowards(transform.position, lastPlayerPosition, speed * Time.deltaTime);
                followTimer += Time.deltaTime;

                if (followTimer >= destroyDelay)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HealthComponent playerHealth = other.GetComponent<HealthComponent>();
            if (playerHealth != null)
            {
                playerHealth.ApplyDamage(damage); // Apply damage to player
            }
            Destroy(gameObject);
        }
    }
}