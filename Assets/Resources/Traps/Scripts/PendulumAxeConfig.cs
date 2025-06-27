using UnityEngine;

public enum SwingStartPosition
{
    Center,
    Left,
    Right
}

[CreateAssetMenu(fileName = "PendulumAxeConfig", menuName = "Traps/Pendulum Axe Config")]
public class PendulumAxeConfig : ScriptableObject
{
    [Header("Swing Settings")]
    public float swingAngle = 45f;                     // Max left/right swing in degrees
    public float swingSpeed = 1f;                      // Speed of swing
    public SwingStartPosition startPosition = SwingStartPosition.Center;

    [Header("Damage Settings")]
    public float damage = 25f;

    [Header("Audio")]
    public AudioClip swingSound;
}
