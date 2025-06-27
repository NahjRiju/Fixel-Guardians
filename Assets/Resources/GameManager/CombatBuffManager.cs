using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CombatBuffManager : MonoBehaviour
{
    [SerializeField] public Transform EffectAnchor; // Drag this in Inspector (next step)

    private Dictionary<EffectConfig.EffectType, GameObject> activeEffectVFX = new();
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();
    private Health health;
    private CombatUI combatUI;
    private CombatLogUI _combatLogUI;
    private float damageMultiplier = 1f; // New variable to track damage multiplier
    private bool isStunned = false; // New flag to track stun
    private bool isFrozen = false;  // New flag to track freeze
    private bool isWeakened = false; // New flag for weakness
    private bool isHallucinating = false; // New flag for hallucination

    private AudioSource effectAudioSource;

    void Start()
    {
        health = GetComponent<Health>();
        if (health == null) Debug.LogError("CombatBuffManager requires a Health component.");
        combatUI = CombatUI.Instance;
        if (combatUI == null) Debug.LogError("CombatUI Singleton not found.");

        effectAudioSource = gameObject.AddComponent<AudioSource>();
        effectAudioSource.playOnAwake = false;

        _combatLogUI = FindObjectOfType<CombatLogUI>(); // Find CombatLogUI
        if (_combatLogUI == null)
        {
            Debug.LogError("CombatLogUI not found in the scene!");
        }
    }

    public void ApplyEffect(EffectConfig config, int duration)
    {
        if (IsEffectActive(config.effectType))
        {
            Debug.Log($"{gameObject.name} already has an active {config.effectType} effect. Cannot apply again.");
            return;
        }

        ActiveEffect newEffect = new(config, duration);
        activeEffects.Add(newEffect);

        _combatLogUI?.AddLogMessage($"{config.effectName} applied to {gameObject.name} for {duration} turns."); // Add log here

         // ✅ Ensure VFX is visible immediately if reused
            if (activeEffectVFX.TryGetValue(config.effectType, out GameObject vfx))
            {
                if (vfx != null)
                    vfx.SetActive(true);
            }

        if (gameObject.CompareTag("Enemy")) combatUI.AddEnemyActiveEffectDisplay(newEffect);
        else if (gameObject.CompareTag("Player")) combatUI.AddPlayerActiveEffectDisplay(newEffect);
    }

    public void ProcessTurnStart()
    {
        List<ActiveEffect> effectsToRemove = new List<ActiveEffect>();

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = activeEffects[i];
            if (effect.config.triggerTiming == EffectConfig.TriggerTiming.StartOfTurn)
            {
                _combatLogUI?.AddLogMessage($"{gameObject.name} - {effect.config.effectName} triggered at the start of the turn."); // Add log here
                effect.remainingTurns--;
                combatUI.UpdateActiveEffectDisplay(effect);
                ApplyEffectLogic(effect);

                if (effect.remainingTurns <= 0)
                {
                    effectsToRemove.Add(effect);
                }
            }
        }

        foreach (var effectToRemove in effectsToRemove)
        {
            RemoveEffect(effectToRemove);
        }
    }

    public void ProcessAttack()
    {
        StartCoroutine(DelayedProcessAttack());
    }

    private IEnumerator DelayedProcessAttack()
    {
        List<ActiveEffect> effectsToRemove = new List<ActiveEffect>();

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = activeEffects[i];

            if (effect.config.triggerTiming == EffectConfig.TriggerTiming.OnAttack)
            {
                _combatLogUI?.AddLogMessage($"{gameObject.name} - {effect.config.effectName} triggered on attack."); // Add log here
                ApplyEffectLogic(effect); // Apply the effect logic
                yield return new WaitForSeconds(1f); // Wait for 1 seconds
                effect.remainingTurns--;
                combatUI.UpdateActiveEffectDisplay(effect);

                if (effect.remainingTurns <= 0)
                {
                    effectsToRemove.Add(effect);
                }
            }
        }

        foreach (var effectToRemove in effectsToRemove)
        {
            RemoveEffect(effectToRemove);
        }
    }

    public void ProcessTurnEnd()
    {
        List<ActiveEffect> effectsToRemove = new List<ActiveEffect>();

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = activeEffects[i];

            if (effect.config.triggerTiming == EffectConfig.TriggerTiming.EndOfTurn)
            {
                _combatLogUI?.AddLogMessage($"{gameObject.name} - {effect.config.effectName} triggered at the end of the turn."); // Add log here
                ApplyEffectLogic(effect);
                effect.remainingTurns--;
                combatUI.UpdateActiveEffectDisplay(effect);

                if (effect.remainingTurns <= 0)
                {
                    effectsToRemove.Add(effect);
                }
            }
        }

        foreach (var effectToRemove in effectsToRemove)
        {
            RemoveEffect(effectToRemove);
        }
    }

    public void ProcessDamage()
    {
        StartCoroutine(DelayedProcessDamage());
    }

    private IEnumerator DelayedProcessDamage()
    {
        List<ActiveEffect> effectsToRemove = new List<ActiveEffect>();

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = activeEffects[i];

            if (effect.config.triggerTiming == EffectConfig.TriggerTiming.OnTakeDamage)
            {
                _combatLogUI?.AddLogMessage($"{gameObject.name} - {effect.config.effectName} triggered on taking damage."); // Add log here
                ApplyEffectLogic(effect); // Apply the effect logic
                yield return new WaitForSeconds(1f); // Wait for 1 seconds
                effect.remainingTurns--;
                combatUI.UpdateActiveEffectDisplay(effect);

                if (effect.remainingTurns <= 0)
                {
                    effectsToRemove.Add(effect);
                }
            }
        }

        foreach (var effectToRemove in effectsToRemove)
        {
            RemoveEffect(effectToRemove);
        }
    }

    private void ApplyEffectLogic(ActiveEffect activeEffect)
    {
        EffectConfig config = activeEffect.config;
        Debug.Log($"{gameObject.name} - Effect: {config.effectType} triggered at: {config.triggerTiming}");

        TriggerEffectVFXAndSound(config);


        switch (config.effectType)
        {
            //Buff
            case EffectConfig.EffectType.Heal:
                health.AddHealth(config.effectValue);
                _combatLogUI?.AddLogMessage($"{gameObject.name} healed for {config.effectValue} health."); // Add log here
                break;

            case EffectConfig.EffectType.DamageIncrease:
                damageMultiplier = config.effectValue;
                _combatLogUI?.AddLogMessage($"{gameObject.name}'s damage increased by {config.effectValue}."); // Add log here
                break;

            case EffectConfig.EffectType.Lifesteal:
                // Lifesteal logic will be applied during the attack phase
                break;

            case EffectConfig.EffectType.Regeneration:
                health.AddHealth(config.effectValue);
                _combatLogUI?.AddLogMessage($"{gameObject.name} regenerated {config.effectValue} health."); // Add log here
                break;

            case EffectConfig.EffectType.CriticalStrike:
                // Just note that this buff is active; the damage modification happens on attack
                break;

            case EffectConfig.EffectType.Shield:
                // Just note that the shield is active; the blocking happens on taking damage
                break;

            case EffectConfig.EffectType.Thorns:
                // Thorns logic is handled in the ApplyDamage method of the Health script
                break;

            case EffectConfig.EffectType.Impact:
                _combatLogUI?.AddLogMessage($"{gameObject.name} gained Impact for {activeEffect.remainingTurns} turns.");
                Debug.Log($"{gameObject.name} gained Impact for {activeEffect.remainingTurns} turns.");
                // The actual attack trigger will happen in CombatManager
                break;

            //Debuff
            case EffectConfig.EffectType.Poison:

            case EffectConfig.EffectType.Burn:
                health.ApplyDamage(config.effectValue, gameObject); // Apply damage from the effect
                _combatLogUI?.AddLogMessage($"{gameObject.name} took {config.effectValue} {config.effectName} damage."); // Add log here
                if (!health.IsAlive)
                {
                    // Inform the CombatManager that the enemy died from a debuff
                    CombatManager.Instance?.HandleEnemyDeathByDebuff(gameObject.GetComponent<EnemyAI>());
                }
                break;

            case EffectConfig.EffectType.Stun:
                isStunned = true;
                Debug.Log($"{gameObject.name} is now stunned for {activeEffect.remainingTurns} turns.");
                 _combatLogUI?.AddLogMessage($"{gameObject.name} is stunned for {activeEffect.remainingTurns} turns."); // Add log here
                // We might want to store the remaining turns of stun directly in this manager
                // or rely on the activeEffect list. For simplicity, let's rely on the list for now.
                break;

            case EffectConfig.EffectType.Weakness:
                isWeakened = true;
                Debug.Log($"{gameObject.name} is now weakened for {activeEffect.remainingTurns} turns.");
                _combatLogUI?.AddLogMessage($"{gameObject.name} is weakened for {activeEffect.remainingTurns} turns."); // Add log here
                break;

            case EffectConfig.EffectType.Hallucination:
                isHallucinating = true;
                Debug.Log($"{gameObject.name} is now hallucinating for {activeEffect.remainingTurns} turns.");
                _combatLogUI?.AddLogMessage($"{gameObject.name} is hallucinating for {activeEffect.remainingTurns} turns."); // Add log here
                break;

            default:
                Debug.LogWarning($"Unknown effect type: {config.effectType}");
                break;
        }
    }

    public void ClearAllEffects()
    {
        Debug.Log($"{gameObject.name}'s CombatBuffManager - Clearing all active effects.");
        // Revert any persistent effect logic before clearing
        foreach (var effect in activeEffects.ToList()) // Iterate over a copy to avoid modification during iteration
        {
            RevertEffectLogic(effect.config);
        }
        activeEffects.Clear();

        // Update the UI
        if (combatUI != null)
        {
            if (gameObject.CompareTag("Player"))
            {
                combatUI.ClearPlayerActiveEffectDisplays();
                Debug.Log("Cleared player's active effect displays.");
            }
            else if (gameObject.CompareTag("Enemy"))
            {
                combatUI.ClearEnemyActiveEffectDisplays();
                Debug.Log("Cleared enemy's active effect displays.");
            }
        }
        else
        {
            Debug.LogWarning("CombatUI.Instance is null in CombatBuffManager. Cannot clear UI.");
        }
    }

    public void RemoveEffect(ActiveEffect effectToRemove)
    {
        activeEffects.Remove(effectToRemove);
        combatUI.RemoveActiveEffectDisplay(effectToRemove);
        _combatLogUI?.AddLogMessage($"{effectToRemove.config.effectName} wore off from {gameObject.name}."); // Add log here
        RevertEffectLogic(effectToRemove.config);

        //Particle effect

      if (activeEffectVFX.TryGetValue(effectToRemove.config.effectType, out GameObject vfx))
        {
            if (vfx != null)
                vfx.SetActive(false); // ✅ Safe from MissingReferenceException
        }

    }

    private void RevertEffectLogic(EffectConfig config)
    {
        switch (config.effectType)
        {
            case EffectConfig.EffectType.DamageIncrease:
                Debug.Log($"{gameObject.name} - DamageIncrease effect ended. Resetting damage multiplier.");
                _combatLogUI?.AddLogMessage($"{gameObject.name}'s damage increase ended."); // Add log here
                damageMultiplier = 1f; // Reset the multiplier
                break;

            case EffectConfig.EffectType.Stun:
                isStunned = false; // Optional: Reset stun flag on removal
                break;

            case EffectConfig.EffectType.Freeze:
                isFrozen = false;  // Optional: Reset freeze flag on removal
                break;

            case EffectConfig.EffectType.Weakness:
                isWeakened = false; // Optional: Reset weakness flag on removal
                _combatLogUI?.AddLogMessage($"{gameObject.name}'s weakness ended."); // Add log here
                break;

            case EffectConfig.EffectType.Impact:
                _combatLogUI?.AddLogMessage($"{gameObject.name}'s impact ended."); // Add log here
                break;
            // Add cases for other effects if they need reverting logic
        }
    }

        // 🟢 This method handles both VFX and sound playback for effects
   private void TriggerEffectVFXAndSound(EffectConfig config)
        {
            // 🔁 Always instantiate a new VFX instance
            if (config.statusEffectPrefab != null)
            {
                Transform anchor = EffectAnchor != null ? EffectAnchor : transform;
                GameObject vfx = Instantiate(config.statusEffectPrefab, anchor.position, Quaternion.identity, anchor);

                // Optional: destroy after X seconds if not looping
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null && !ps.main.loop)
                {
                    Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(vfx, 3f); // fallback destroy in 3s
                }
            }

            // 🔊 Play sound as usual
            if (config.effectSound != null)
            {
                effectAudioSource.clip = config.effectSound;
                effectAudioSource.outputAudioMixerGroup = config.audioMixerGroup;
                effectAudioSource.volume = config.soundVolume;
                effectAudioSource.Play();
            }
        }

    public List<ActiveEffect> GetActiveEffects()
    {
        return activeEffects;
    }

    public bool IsEffectActive(EffectConfig.EffectType type)
    {
        return activeEffects.Any(effect => effect.config.effectType == type);
    }

    public float GetEffectValue(EffectConfig.EffectType type)
    {
        ActiveEffect effect = activeEffects.FirstOrDefault(e => e.config.effectType == type);
        return effect != null ? effect.config.effectValue : 0f;
    }

    // Public method to get the current damage multiplier
    public float GetDamageMultiplier()
    {
        return damageMultiplier;
    }

    public void ForceEffectVisual(EffectConfig config)
    {
        TriggerEffectVFXAndSound(config);
    }
}