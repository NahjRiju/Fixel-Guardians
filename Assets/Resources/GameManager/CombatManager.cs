using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using Random = UnityEngine.Random;
using System.Collections.Generic;using StarterAssets;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    public CombatUI combatUI;
    private CombatLogUI _combatLogUI; // Add this line
    public enum Turn { Player, Enemy }
    public Turn currentTurn = Turn.Player;

    public UIVirtualJoystick virtualJoystick;
    private EnemyAI _currentEnemy;
    public EnemyAI CurrentEnemy
    {
        get { return _currentEnemy; }
        private set { _currentEnemy = value; }
    }
    private Transform playerTransform;
    private Animator playerAnimator;
    private Health playerHealth;
    private CombatBuffManager playerBuffManager;
    public PlayerConfig playerConfig;

    private CombatZoneTrigger currentCombatZone;
    private GameStateManager gameStateManager;
    private bool objectiveCompleted = false;
    private string clearedZoneIdentifier;
    private Queue<EnemyAI> initialEnemyQueue;

    [Header("Turn Durations")]
    public float startOfTurnDelay = 1f;
    public float attackDelay = 2f;
    public float endOfTurnDelay = 1f;
    public float turnTransitionDelay = 1f;
    public float gameOverDelay = 3f;
    public float combatEndDelay = 1f;

    [Header("Combat VFX & SFX")]
    public GameObject hitEffectPrefab; // Assign in Inspector
    public AudioClip hitSoundClip;     // Assign in Inspector
    public AudioSource sfxAudioSource; // Assign in Inspector
    public AudioSource bgmAudioSource; // Assign in Inspector (looped)
    public float screenShakeIntensity = 0.3f;
    public float screenShakeDuration = 0.2f;

    IEnumerator ScreenShake(float intensity, float duration)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            Camera.main.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple CombatManagers detected! Destroying the duplicate.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        combatUI = FindObjectOfType<CombatUI>();
        if (combatUI == null)
        {
            Debug.LogError("CombatUI not found!");
        }
        combatUI.SetTurnIndicator(currentTurn);
        combatUI.proceedButton.onClick.AddListener(CompleteObjective);

        _combatLogUI = FindObjectOfType<CombatLogUI>(); // Add this line
        if (_combatLogUI == null)
        {
            Debug.LogError("CombatLogUI not found in the scene!");
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerAnimator = player.GetComponent<Animator>();
            playerHealth = player.GetComponent<Health>();
            playerBuffManager = player.GetComponent<CombatBuffManager>();
            if (playerAnimator == null) Debug.LogError("Player Animator component not found!");
            if (playerHealth == null) Debug.LogError("Player Health component not found!");
            if (playerBuffManager == null) Debug.LogError("Player CombatBuffManager component not found!");
            playerHealth.OnHealthEmpty += HandlePlayerDeath;
        }
        else
        {
            Debug.LogError("Player GameObject not found!");
        }

        gameStateManager = FindObjectOfType<GameStateManager>();
        if (gameStateManager == null)
        {
            Debug.LogError("GameStateManager not found!");
        }
    }

    public void StartCombat(EnemyAI enemy)
    {
        currentCombatZone = enemy.GetComponentInParent<CombatZoneTrigger>();
        if (currentCombatZone != null)
        {
            initialEnemyQueue = new Queue<EnemyAI>(currentCombatZone.GetAllEnemies());
            CurrentEnemy = enemy;
            combatUI.ShowCombatPanel(initialEnemyQueue);
            combatUI.UpdateEnemyDisplay(CurrentEnemy);
            UpdateEnemyOutlineHighlight();
            combatUI.EnableAttackButton();
            combatUI.SetEnemyQueueInteraction(currentTurn);
            FaceEachOther(CurrentEnemy.transform);
            objectiveCompleted = false;
            _combatLogUI?.AddLogMessage($"Combat started against {CurrentEnemy.enemyConfig.enemyName}!"); // Use _combatLogUI

            // Reset the joystick
            if (virtualJoystick != null) virtualJoystick.ResetJoystick();
            else Debug.LogError("UIVirtualJoystick not assigned in the CombatManager!");

            // Stop Player Movement
            if (playerTransform != null)
            {
                StarterAssetsInputs playerInput = playerTransform.GetComponent<StarterAssetsInputs>();
                if (playerInput != null)
                {
                    playerInput.canMove = false;
                    playerInput.MoveInput(Vector2.zero);
                    playerInput.sprint = false;
                }
                else Debug.LogError("StarterAssetsInputs component not found on the Player!");
            }

            if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
                {
                    bgmAudioSource.Play(); // Assuming loop = true
                }

        }
    }

    public void SwitchCurrentEnemy(EnemyAI newTarget)
    {
        if (currentTurn == Turn.Player && newTarget != CurrentEnemy && newTarget.GetComponent<Health>().IsAlive)
        {
            Debug.Log($"Player switched target to {newTarget.enemyConfig.enemyName}");
            _combatLogUI?.AddLogMessage($"Player switched target to {newTarget.enemyConfig.enemyName}"); // Use _combatLogUI
            CurrentEnemy = newTarget;
            FaceEachOther(CurrentEnemy.transform);
            combatUI.UpdateEnemyDisplay(CurrentEnemy);
            UpdateEnemyOutlineHighlight();
        }
    }

    public void PlayerAttack()
    {
        combatUI.DisableAttackButton();
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("PlayerAttack");
        }
        StartCoroutine(ExecutePlayerAttack());
    }

    IEnumerator PlayerStartOfTurn()
    {
        yield return new WaitForSeconds(startOfTurnDelay);
        CombatBuffManager buffManager = playerBuffManager;
       if (buffManager != null && buffManager.IsEffectActive(EffectConfig.EffectType.Freeze))
        {
            Debug.Log("Player is frozen! Skipping turn.");
            _combatLogUI?.AddLogMessage("Player is frozen and cannot act!");

            var effect = buffManager.GetActiveEffects().FirstOrDefault(e => e.config.effectType == EffectConfig.EffectType.Freeze);
            if (effect != null)
            {
                // 🟢 Manually trigger VFX and sound
                buffManager.ForceEffectVisual(effect.config);
            }

            DecrementFreeze(buffManager);
            StartCoroutine(BeginEnemyTurn());
        }
        else if (buffManager != null && buffManager.IsEffectActive(EffectConfig.EffectType.Stun))
        {
            Debug.Log("Player is stunned! Skipping turn.");
            _combatLogUI?.AddLogMessage("Player is stunned and cannot act!");

            var effect = buffManager.GetActiveEffects().FirstOrDefault(e => e.config.effectType == EffectConfig.EffectType.Stun);
            if (effect != null)
            {
                buffManager.ForceEffectVisual(effect.config);
            }

            DecrementStun(buffManager);
            StartCoroutine(BeginEnemyTurn());
        }

        else
        {
            _combatLogUI?.AddLogMessage("Player's turn begins.");
            buffManager?.ProcessTurnStart();
            combatUI.SetTurnIndicator(currentTurn);
            combatUI.EnableAttackButton();
            combatUI.SetEnemyQueueInteraction(currentTurn);
        }
    }

    IEnumerator ExecutePlayerAttack()
    {
        yield return new WaitForSeconds(attackDelay);

        if (CurrentEnemy != null && playerTransform != null)
        {
            playerBuffManager?.ProcessAttack();
            float finalDamage = playerConfig.playerDamage * playerBuffManager.GetDamageMultiplier();

            // Apply Weakness
            if (playerBuffManager.IsEffectActive(EffectConfig.EffectType.Weakness))
            {
                finalDamage *= 0.5f;
                Debug.Log("Player's attack damage weakened to 50%.");
                _combatLogUI?.AddLogMessage("Player's attack is weakened!");
            }

            // Check for Hallucination
            if (playerBuffManager.IsEffectActive(EffectConfig.EffectType.Hallucination))
            {
                float hallucinationChance = 0.5f;
                if (Random.Range(0f, 1f) < hallucinationChance)
                {
                    Debug.Log("Player's attack missed due to hallucination!");
                    _combatLogUI?.AddLogMessage("Player's attack missed due to hallucination!");
                    StartCoroutine(PlayerEndOfTurn()); // End the turn immediately
                    yield break; // Exit the coroutine
                }
            }

            // Check for Critical Strike
            if (playerBuffManager.IsEffectActive(EffectConfig.EffectType.CriticalStrike))
            {
                float criticalHitChance = 0.5f;
                if (Random.Range(0f, 1f) < criticalHitChance)
                {
                    float criticalMultiplier = playerBuffManager.GetEffectValue(EffectConfig.EffectType.CriticalStrike);
                    finalDamage *= criticalMultiplier;
                    Debug.Log("Player landed a critical hit!");
                    _combatLogUI?.AddLogMessage("Player landed a critical hit!");
                    // Optionally trigger a visual effect for critical hit
                }
            }

            Debug.Log($"Player dealt {finalDamage} damage to {CurrentEnemy.name}."); // Added debug log
            _combatLogUI?.AddLogMessage($"Player dealt {finalDamage} damage to {CurrentEnemy.name}.");

            CurrentEnemy.GetComponent<Health>().ApplyDamage(finalDamage, playerTransform.gameObject);

            // 💥 Spawn hit VFX
                Transform enemyAnchor = CurrentEnemy.GetComponent<CombatBuffManager>()?.EffectAnchor ?? CurrentEnemy.transform;
                Instantiate(hitEffectPrefab, enemyAnchor.position, Quaternion.identity);

                // 🔊 Play hit sound
                if (hitSoundClip != null && sfxAudioSource != null)
                {
                    sfxAudioSource.PlayOneShot(hitSoundClip);
                }

                // 🎮 Screen shake
                StartCoroutine(ScreenShake(screenShakeIntensity, screenShakeDuration));


            CurrentEnemy.TriggerEnemyTakeDamageAnimation();
            combatUI.UpdateEnemyDisplay(CurrentEnemy);

            // Apply Lifesteal (using the potentially critical damage)
            if (playerBuffManager.IsEffectActive(EffectConfig.EffectType.Lifesteal))
            {
                float lifestealValue = playerBuffManager.GetEffectValue(EffectConfig.EffectType.Lifesteal);
                float healthToRestore = finalDamage * lifestealValue;
                playerHealth.AddHealth(healthToRestore);
                Debug.Log($"Player lifestealed for {healthToRestore} health (final damage: {finalDamage}).");
                _combatLogUI?.AddLogMessage($"Player lifestealed for {healthToRestore} health.");
            }

            // Check for Impact
            if (playerBuffManager.IsEffectActive(EffectConfig.EffectType.Impact))
            {
                Debug.Log("Player has Impact! Initiating second attack.");
                _combatLogUI?.AddLogMessage("Player has Impact! Attacking again.");
                // Immediately execute another player attack
                StartCoroutine(ExecutePlayerAttack_Second());
                yield break; // Prevent the regular end of turn
            }
            
        }
        else
        {
            Debug.LogError("PlayerTransform is null in CombatManager during PlayerAttack!");
        }
        StartCoroutine(PlayerEndOfTurn());
    }

    IEnumerator ExecutePlayerAttack_Second()
    {
        yield return new WaitForSeconds(attackDelay * 2f); // Shorter delay for the second attack

        if (CurrentEnemy != null && playerTransform != null)
        {
            float finalDamage = playerConfig.playerDamage * playerBuffManager.GetDamageMultiplier();

            // Apply Weakness (still active if it was)
            if (playerBuffManager.IsEffectActive(EffectConfig.EffectType.Weakness))
            {
                finalDamage *= 0.5f;
                Debug.Log("Second Player attack damage weakened to 50%.");
                _combatLogUI?.AddLogMessage("Second Player attack is weakened!");
            }

            // Hallucination check for the second attack
            if (playerBuffManager.IsEffectActive(EffectConfig.EffectType.Hallucination))
            {
                float hallucinationChance = 0.5f;
                if (Random.Range(0f, 1f) < hallucinationChance)
                {
                    Debug.Log("Second Player attack missed due to hallucination!");
                    _combatLogUI?.AddLogMessage("Second Player attack missed due to hallucination!");
                    StartCoroutine(PlayerEndOfTurn());
                    yield break;
                }
            }

            // Critical Strike check for the second attack
            if (playerBuffManager.IsEffectActive(EffectConfig.EffectType.CriticalStrike))
            {
                float criticalHitChance = 0.5f;
                if (Random.Range(0f, 1f) < criticalHitChance)
                {
                    float criticalMultiplier = playerBuffManager.GetEffectValue(EffectConfig.EffectType.CriticalStrike);
                    finalDamage *= criticalMultiplier;
                    Debug.Log("Second Player attack landed a critical hit!");
                    _combatLogUI?.AddLogMessage("Second Player attack landed a critical hit!");
                }
            }

            Debug.Log($"Second Player attack dealt {finalDamage} damage to {CurrentEnemy.name}.");
            _combatLogUI?.AddLogMessage($"Second Player attack dealt {finalDamage} damage to {CurrentEnemy.name}.");

            CurrentEnemy.GetComponent<Health>().ApplyDamage(finalDamage, playerTransform.gameObject);
            CurrentEnemy.TriggerEnemyTakeDamageAnimation();
            combatUI.UpdateEnemyDisplay(CurrentEnemy);

            // Lifesteal for the second attack
            if (playerBuffManager.IsEffectActive(EffectConfig.EffectType.Lifesteal))
            {
                float lifestealValue = playerBuffManager.GetEffectValue(EffectConfig.EffectType.Lifesteal);
                float healthToRestore = finalDamage * lifestealValue;
                playerHealth.AddHealth(healthToRestore);
                Debug.Log($"Second Player attack lifestealed for {healthToRestore} health (final damage: {finalDamage}).");
                _combatLogUI?.AddLogMessage($"Second Player attack lifestealed for {healthToRestore} health.");
            }
        }
        else
        {
            Debug.LogError("PlayerTransform is null in CombatManager during ExecutePlayerAttack_Second!");
        }
        StartCoroutine(PlayerEndOfTurn());
    }

    IEnumerator PlayerEndOfTurn()
    {
        yield return new WaitForSeconds(endOfTurnDelay);
        playerBuffManager?.ProcessTurnEnd();
        _combatLogUI?.AddLogMessage("Player's turn ends.");
        StartCoroutine(BeginEnemyTurn());
    }

    IEnumerator BeginEnemyTurn()
    {
        yield return new WaitForSeconds(turnTransitionDelay);
        currentTurn = Turn.Enemy;
        StartCoroutine(EnemyStartOfTurn());
    }

    IEnumerator EnemyStartOfTurn()
    {
        yield return new WaitForSeconds(startOfTurnDelay);
        if (CurrentEnemy != null)
        {
            CombatBuffManager enemyBuffManager = CurrentEnemy.GetComponent<CombatBuffManager>();
            if (enemyBuffManager != null && enemyBuffManager.IsEffectActive(EffectConfig.EffectType.Freeze))
            {
                Debug.Log($"{CurrentEnemy.name} is frozen! Skipping turn.");
                _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName} is frozen and cannot act!");
                DecrementFreeze(enemyBuffManager);
                StartCoroutine(BeginPlayerTurn());
            }
            else if (enemyBuffManager != null && enemyBuffManager.IsEffectActive(EffectConfig.EffectType.Stun))
            {
                Debug.Log($"{CurrentEnemy.name} is stunned! Skipping turn.");
                _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName} is stunned and cannot act!");
                DecrementStun(enemyBuffManager);
                StartCoroutine(BeginPlayerTurn());
            }
            else
            {
                _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName}'s turn begins.");
                enemyBuffManager?.ProcessTurnStart();
                combatUI.SetTurnIndicator(currentTurn);
                combatUI.SetEnemyQueueInteraction(currentTurn);
                EnemyAttack();
            }
        }
        else
        {
            Debug.LogWarning("EnemyStartOfTurn called but CurrentEnemy is null.");
            StartCoroutine(BeginPlayerTurnAfterEnemyDeath()); // Handle case where enemy might have died
        }
    }

    void EnemyAttack()
    {
        if (CurrentEnemy != null && CurrentEnemy.GetComponent<Health>().IsAlive)
        {
            StartCoroutine(ExecuteEnemyAttack());
        }
        else
        {
            StartCoroutine(BeginPlayerTurnAfterEnemyDeath()); // Handle case where enemy might have died before attacking
        }
    }

    IEnumerator ExecuteEnemyAttack()
    {
        yield return new WaitForSeconds(attackDelay);
        if (CurrentEnemy != null && CurrentEnemy.GetComponent<Health>().IsAlive)
        {
            CurrentEnemy.TriggerEnemyAttackAnimation();
            if (playerHealth != null)
            {
                CurrentEnemy.GetComponent<CombatBuffManager>()?.ProcessAttack();
                CombatBuffManager enemyBuffManager = CurrentEnemy.GetComponent<CombatBuffManager>();
                float finalDamage = CurrentEnemy.enemyConfig.attackDamage * enemyBuffManager.GetDamageMultiplier();
                
                // Apply Weakness
                if (enemyBuffManager.IsEffectActive(EffectConfig.EffectType.Weakness))
                {
                    finalDamage *= 0.5f;
                    Debug.Log($"{CurrentEnemy.name}'s attack damage weakened to 50%.");
                    _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName}'s attack is weakened!");
                }

                // Check for Hallucination
                if (enemyBuffManager.IsEffectActive(EffectConfig.EffectType.Hallucination))
                {
                    float hallucinationChance = 0.5f;
                    if (Random.Range(0f, 1f) < hallucinationChance)
                    {
                        Debug.Log($"{CurrentEnemy.name}'s attack missed due to hallucination!");
                        _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName}'s attack missed due to hallucination!");
                        StartCoroutine(EnemyEndOfTurn()); // End the turn immediately
                        yield break; // Exit the coroutine
                    }
                }

                // Check for Critical Strike
                if (enemyBuffManager.IsEffectActive(EffectConfig.EffectType.CriticalStrike))
                {
                    float criticalHitChance = 0.3f; // Enemies might have a different crit chance
                    if (Random.Range(0f, 1f) < criticalHitChance)
                    {
                        float criticalMultiplier = enemyBuffManager.GetEffectValue(EffectConfig.EffectType.CriticalStrike);
                        finalDamage *= criticalMultiplier;
                        Debug.Log($"{CurrentEnemy.name} landed a critical hit!");
                        _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName} landed a critical hit!");
                        // Optionally trigger a visual effect for enemy critical hit
                    }
                }

                Debug.Log($"{CurrentEnemy.name} dealt {finalDamage} damage to Player."); // Added debug log
                _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName} dealt {finalDamage} damage to Player.");

                playerHealth.ApplyDamage(finalDamage, CurrentEnemy.gameObject);

                Transform playerAnchor = playerBuffManager.EffectAnchor ?? playerTransform;
                Instantiate(hitEffectPrefab, playerAnchor.position, Quaternion.identity);

                if (hitSoundClip != null && sfxAudioSource != null)
                {
                    sfxAudioSource.PlayOneShot(hitSoundClip);
                }

                StartCoroutine(ScreenShake(screenShakeIntensity, screenShakeDuration));

                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("PlayerTakeDamage");
                }

                // Apply Lifesteal (using the potentially critical damage)
                if (enemyBuffManager.IsEffectActive(EffectConfig.EffectType.Lifesteal))
                {
                    float lifestealValue = enemyBuffManager.GetEffectValue(EffectConfig.EffectType.Lifesteal);
                    float healthToRestore = finalDamage * lifestealValue;
                    CurrentEnemy.GetComponent<Health>().AddHealth(healthToRestore);
                    Debug.Log($"{CurrentEnemy.name} lifestealed for {healthToRestore} health (final damage: {finalDamage}).");
                    _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName} lifestealed for {healthToRestore} health.");
                }

                // Check for Impact (Enemy)
                if (enemyBuffManager.IsEffectActive(EffectConfig.EffectType.Impact))
                {
                    Debug.Log($"{CurrentEnemy.name} has Impact! Initiating second attack.");
                    _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName} has Impact! Attacking again.");
                    // Immediately execute another enemy attack
                    StartCoroutine(ExecuteEnemyAttack_Second());
                    yield break; // Prevent the regular end of turn
                }
            }
            StartCoroutine(EnemyEndOfTurn());
        }
        else
        {
            StartCoroutine(BeginPlayerTurnAfterEnemyDeath()); // Handle case where enemy might have died during attack
        }
    }

    IEnumerator ExecuteEnemyAttack_Second()
    {
        yield return new WaitForSeconds(attackDelay * 2f); // Shorter delay for the second attack

        if (CurrentEnemy != null && CurrentEnemy.GetComponent<Health>().IsAlive)
        {
            if (playerHealth != null)
            {
                CombatBuffManager enemyBuffManager = CurrentEnemy.GetComponent<CombatBuffManager>();
                float finalDamage = CurrentEnemy.enemyConfig.attackDamage * enemyBuffManager.GetDamageMultiplier();

                // Apply Weakness (still active)
                if (enemyBuffManager.IsEffectActive(EffectConfig.EffectType.Weakness))
                {
                    finalDamage *= 0.5f;
                    Debug.Log($"{CurrentEnemy.name}'s second attack damage weakened to 50%.");
                    _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName}'s second attack is weakened!");
                }

                // Hallucination check for the second attack
                if (enemyBuffManager.IsEffectActive(EffectConfig.EffectType.Hallucination))
                {
                    float hallucinationChance = 0.5f;
                    if (Random.Range(0f, 1f) < hallucinationChance)
                    {
                        Debug.Log($"{CurrentEnemy.name}'s second attack missed due to hallucination!");
                        _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName}'s second attack missed due to hallucination!");
                        StartCoroutine(EnemyEndOfTurn());
                        yield break;
                    }
                }

                // Critical Strike check for the second attack
                if (enemyBuffManager.IsEffectActive(EffectConfig.EffectType.CriticalStrike))
                {
                    float criticalHitChance = 0.3f;
                    if (Random.Range(0f, 1f) < criticalHitChance)
                    {
                        float criticalMultiplier = enemyBuffManager.GetEffectValue(EffectConfig.EffectType.CriticalStrike);
                        finalDamage *= criticalMultiplier;
                        Debug.Log($"{CurrentEnemy.name}'s second attack landed a critical hit!");
                        _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName}'s second attack landed a critical hit!");
                    }
                }

                Debug.Log($"{CurrentEnemy.name}'s second attack dealt {finalDamage} damage to Player.");
                _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName}'s second attack dealt {finalDamage} damage to Player.");

                playerHealth.ApplyDamage(finalDamage, CurrentEnemy.gameObject);
                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("PlayerTakeDamage");
                }

                // Lifesteal for the second attack
                if (enemyBuffManager.IsEffectActive(EffectConfig.EffectType.Lifesteal))
                {
                    float lifestealValue = enemyBuffManager.GetEffectValue(EffectConfig.EffectType.Lifesteal);
                    float healthToRestore = finalDamage * lifestealValue;
                    CurrentEnemy.GetComponent<Health>().AddHealth(healthToRestore);
                    Debug.Log($"{CurrentEnemy.name}'s second attack lifestealed for {healthToRestore} health (final damage: {finalDamage}).");
                    _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName}'s second attack lifestealed for {healthToRestore} health.");
                }
            }
            StartCoroutine(EnemyEndOfTurn());
        }
        else
        {
            StartCoroutine(BeginPlayerTurnAfterEnemyDeath());
        }
    }

    IEnumerator EnemyEndOfTurn()
    {
        yield return new WaitForSeconds(endOfTurnDelay);
        if (CurrentEnemy != null)
        {
            CurrentEnemy.GetComponent<CombatBuffManager>()?.ProcessTurnEnd();
            _combatLogUI?.AddLogMessage($"{CurrentEnemy.enemyConfig.enemyName}'s turn ends.");
        }
        StartCoroutine(BeginPlayerTurn());
    }

    IEnumerator BeginPlayerTurn()
    {
        yield return new WaitForSeconds(turnTransitionDelay);
        currentTurn = Turn.Player;
        StartCoroutine(PlayerStartOfTurn());
    }

    private void FaceEachOther(Transform target)
    {
        if (playerTransform != null && target != null)
        {
            Vector3 playerToTarget = target.position - playerTransform.position;
            playerToTarget.y = 0;
            Vector3 targetToPlayer = playerTransform.position - target.position;
            target.rotation = Quaternion.LookRotation(targetToPlayer);
            playerTransform.rotation = Quaternion.LookRotation(playerToTarget);
        }
    }

    public void HandleEnemyDeath(EnemyAI deadEnemy)
    {
        if (currentCombatZone != null)
        {
            _combatLogUI?.AddLogMessage($"{deadEnemy.enemyConfig.enemyName} has been defeated!");
            currentCombatZone.RemoveEnemy(deadEnemy);
            combatUI.UpdateEnemyQueueDisplay(deadEnemy);

            CombatBuffManager enemyBuffManager = deadEnemy.GetComponent<CombatBuffManager>();
            if (enemyBuffManager != null)
            {
                List<ActiveEffect> activeEffectsToRemove = new List<ActiveEffect>(enemyBuffManager.GetActiveEffects());
                foreach (var effect in activeEffectsToRemove)
                {
                    combatUI.RemoveActiveEffectDisplay(effect);
                }
            }
            else
            {
                Debug.LogWarning($"CombatBuffManager not found on dead enemy: {deadEnemy.name}");
            }

            if (currentCombatZone.IsQueueEmpty())
            {
                Vector3 dropPosition = currentCombatZone.GetLastEnemyDefeatedPosition();
                currentCombatZone.DropZoneLoot(dropPosition); // Drop loot here
                StartCoroutine(TransitionToCombatEnd(currentCombatZone.zoneIdentifier));
            }
            else
            {
                // Immediately start the player's turn
                StartCoroutine(BeginPlayerTurn());
            }
        }
    }

    IEnumerator BeginPlayerTurnAfterEnemyDeath()
    {
        yield return new WaitForSeconds(turnTransitionDelay);
        currentTurn = Turn.Player;
        combatUI.SetTurnIndicator(currentTurn);
        combatUI.EnableAttackButton();
        combatUI.SetEnemyQueueInteraction(currentTurn);

        var remainingEnemies = currentCombatZone.GetComponentsInChildren<EnemyAI>().Where(e => e != null && e.GetComponent<Health>().IsAlive).ToList();
        if (remainingEnemies.Count > 0)
        {
            int randomIndex = Random.Range(0, remainingEnemies.Count);
            CurrentEnemy = remainingEnemies[randomIndex];
            combatUI.UpdateEnemyDisplay(CurrentEnemy);
            FaceEachOther(CurrentEnemy.transform);
            UpdateEnemyOutlineHighlight();
            Debug.Log($"Enemy defeated. Starting new player turn. Facing: {CurrentEnemy.name}");
            _combatLogUI?.AddLogMessage($"Enemy defeated. Player's turn begins, facing {CurrentEnemy.enemyConfig.enemyName}.");
        }
        else
        {
            Debug.Log("Enemy defeated. No enemies remaining.");
        }

        StartCoroutine(PlayerStartOfTurn()); // Start the player's start of turn coroutine
    }

    IEnumerator TransitionToCombatEnd(string zoneId)
    {
        yield return new WaitForSeconds(combatEndDelay);

        if (currentCombatZone != null && currentCombatZone.IsQueueEmpty())
        {
            // First, trigger the physical loot drop
            // Pass the player's current position or the last defeated enemy's position for the drop.

            // Then, get the list of loot items specifically for UI display
            List<LootItem> earnedLootForDisplay = currentCombatZone.GetLootItemsForDisplay();
            
            combatUI.ShowWinPanel(earnedLootForDisplay); // Pass the earned loot for display to the UI
            combatUI.ClearEnemyQueueDisplay();
            Debug.Log($"Combat in zone '{zoneId}' cleared!");
            _combatLogUI?.AddLogMessage($"Combat in zone '{zoneId}' cleared!");
            clearedZoneIdentifier = zoneId;
        }
        else
        {
            Debug.LogWarning("TransitionToCombatEnd called but the zone is not empty?");
        }
    }

    private void CompleteObjective()
    {
        if (!objectiveCompleted && !string.IsNullOrEmpty(clearedZoneIdentifier))
        {
            ObjectiveConditionManager objectiveConditionManager = FindObjectOfType<ObjectiveConditionManager>();
            if (objectiveConditionManager != null)
            {
                objectiveConditionManager.CheckObjectiveConditions(LevelConfig.ObjectiveConditionType.ClearCombatZone, clearedZoneIdentifier);
                objectiveCompleted = true;
                Debug.Log($"Objective for clearing zone '{clearedZoneIdentifier}' completed.");
                _combatLogUI?.AddLogMessage($"Objective for clearing zone '{clearedZoneIdentifier}' completed.");

                // Re-enable Player Movement
                if (playerTransform != null)
                {
                    StarterAssetsInputs playerInput = playerTransform.GetComponent<StarterAssetsInputs>();
                    if (playerInput != null)
                    {
                        playerInput.canMove = true;
                    }
                }
            }
            else
            {
                Debug.LogError("ObjectiveConditionManager not found!");
            }
        }
        else if (objectiveCompleted)
        {
            Debug.Log("Combat zone objective already completed.");
        }
        else
        {
            Debug.LogWarning("Cleared zone identifier is null or empty.");
        }
        combatUI.HideCombatPanel();
        _combatLogUI?.AddLogMessage("Combat panel closed.");

        if (bgmAudioSource != null) bgmAudioSource.Stop();

    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(DelayedGameOver());
    }

    public void HandleEnemyDeathByDebuff(EnemyAI deadEnemy)
    {
        if (currentCombatZone != null)
        {
            _combatLogUI?.AddLogMessage($"{deadEnemy.enemyConfig.enemyName} was defeated by a debuff!");
            currentCombatZone.RemoveEnemy(deadEnemy);
            combatUI.UpdateEnemyQueueDisplay(deadEnemy);

            CombatBuffManager enemyBuffManager = deadEnemy.GetComponent<CombatBuffManager>();
            if (enemyBuffManager != null)
            {
                List<ActiveEffect> activeEffectsToRemove = new List<ActiveEffect>(enemyBuffManager.GetActiveEffects());
                foreach (var effect in activeEffectsToRemove)
                {
                    combatUI.RemoveActiveEffectDisplay(effect);
                }
            }
            else
            {
                Debug.LogWarning($"CombatBuffManager not found on dead enemy: {deadEnemy.name}");
            }

            if (currentCombatZone.IsQueueEmpty())
            {
                Vector3 dropPosition = currentCombatZone.GetLastEnemyDefeatedPosition();
                currentCombatZone.DropZoneLoot(dropPosition); // Drop loot here
                StartCoroutine(TransitionToCombatEnd(currentCombatZone.zoneIdentifier));
            }
            else
            {
                // Immediately start the player's turn
                StartCoroutine(BeginPlayerTurn());
            }
        }
    }

    IEnumerator DelayedGameOver()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("PlayerDie");
        }

        yield return new WaitForSeconds(gameOverDelay);
        Debug.Log("Player has died! Triggering Game Over.");
        if (gameStateManager != null)
        {
            gameStateManager.GameOver();
        }
    }

    private void UpdateEnemyOutlineHighlight()
    {
        if (currentCombatZone == null) return;

        foreach (var enemy in currentCombatZone.GetAllEnemies())
        {
            if (enemy == null) continue;

            Outline outline = enemy.GetComponentInChildren<Outline>();
            if (outline != null)
            {
                outline.enabled = (enemy == CurrentEnemy);
            }
        }
    }

    private void DecrementStun(CombatBuffManager buffManager)
    {
        ActiveEffect stunEffect = buffManager.GetActiveEffects().FirstOrDefault(e => e.config.effectType == EffectConfig.EffectType.Stun);
        if (stunEffect != null)
        {
            stunEffect.remainingTurns--;
            combatUI.UpdateActiveEffectDisplay(stunEffect);
            _combatLogUI?.AddLogMessage($"{stunEffect.config.effectName} on {(buffManager == playerBuffManager ? "Player" : CurrentEnemy.enemyConfig.enemyName)} remaining turns: {stunEffect.remainingTurns}.");
            if (stunEffect.remainingTurns <= 0)
            {
                buffManager.RemoveEffect(stunEffect);
                _combatLogUI?.AddLogMessage($"{stunEffect.config.effectName} has worn off from {(buffManager == playerBuffManager ? "Player" : CurrentEnemy.enemyConfig.enemyName)}.");
            }
        }
    }

    private void DecrementFreeze(CombatBuffManager buffManager)
    {
        ActiveEffect freezeEffect = buffManager.GetActiveEffects().FirstOrDefault(e => e.config.effectType == EffectConfig.EffectType.Freeze);
        if (freezeEffect != null)
        {
            freezeEffect.remainingTurns--;
            combatUI.UpdateActiveEffectDisplay(freezeEffect);
            _combatLogUI?.AddLogMessage($"{freezeEffect.config.effectName} on {(buffManager == playerBuffManager ? "Player" : CurrentEnemy.enemyConfig.enemyName)} remaining turns: {freezeEffect.remainingTurns}.");
            if (freezeEffect.remainingTurns <= 0)
            {
                buffManager.RemoveEffect(freezeEffect);
                _combatLogUI?.AddLogMessage($"{freezeEffect.config.effectName} has worn off from {(buffManager == playerBuffManager ? "Player" : CurrentEnemy.enemyConfig.enemyName)}.");
            }
        }
    }
}