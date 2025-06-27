using UnityEngine;

[CreateAssetMenu(fileName = "LaserTrapConfig", menuName = "Traps/Laser Trap Config")]
public class LaserTrapConfig : ScriptableObject
{
    public float damage = 5f;
    public float speed = 2f;
    public float moveDistance = 3f;
    public bool isLooping = true;

    public enum MovementType
    {
        Horizontal,  // ←→
        Vertical,    // ↑↓
        OneWay,      // Just from A to B then stop
        None         // Static laser
    }

    public enum MovementDirection
    {
        Forward, // Right or Up
        Reverse  // Left or Down
    }


    [Header("Movement")]
    public MovementType movementType = MovementType.Horizontal;

    public MovementDirection movementDirection = MovementDirection.Forward; // 🔧 Add this

    public GameObject laserBeamPrefab;
    public AudioClip laserSound;
}
