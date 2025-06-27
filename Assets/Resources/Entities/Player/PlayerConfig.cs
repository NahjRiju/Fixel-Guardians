using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/Player Config")]
public class PlayerConfig : ScriptableObject
{
    [Header("General Stats")]
    public float playerDamage = 20f;
    public float maxHealth = 100f;
    [Range(0, 1)] public float initialRatio = 1.0f;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float sprintSpeed = 14f;
    public float jumpHeight = 1.5f;
    public float gravity = -15.0f;
    public float rotationSmoothTime = 0.12f;
    public float speedChangeRate = 10.0f;
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.28f;

    [Header("Jump and Fall Settings")]
    public float jumpTimeout = 0.50f;
    public float fallTimeout = 0.15f;

    [Header("Buff Settings")]
    public List<EffectConfig> selectedEffects = new List<EffectConfig>(); // For pre-level selection

    // REMOVE THIS:
    // [Header("Collected Buffs")]
    // public List<EffectConfig> collectedBuffs = new List<EffectConfig>(); // New list for permanently collected buffs

    // No longer needs to subscribe to OnGameDataReset to clear collectedBuffs here
    // PersistentGameManager handles clearing _collectedBuffNames directly.
    private void OnEnable()
    {
        // PersistentGameManager.OnGameDataReset += ClearCollectedBuffs; // REMOVE THIS LINE
        // Debug.Log("PlayerConfig subscribed to OnGameDataReset."); // REMOVE THIS LINE
    }

    private void OnDisable()
    {
        // PersistentGameManager.OnGameDataReset -= ClearCollectedBuffs; // REMOVE THIS LINE
        // Debug.Log("PlayerConfig unsubscribed from OnGameDataReset."); // REMOVE THIS LINE
    }

    // REMOVE THIS METHOD:
    // private void ClearCollectedBuffs()
    // {
    //     Debug.Log("ClearCollectedBuffs() called in PlayerConfig. Clearing collectedBuffs.");
    //     collectedBuffs.Clear();
    //     Debug.Log("collectedBuffs count after clear: " + collectedBuffs.Count);
    // }

    // Method to clear selected effects
    public void ClearSelectedEffects()
    {
        selectedEffects.Clear();
        Debug.Log("Selected effects cleared on PlayerConfig.");
    }
}