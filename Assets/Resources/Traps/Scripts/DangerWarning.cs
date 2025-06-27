using UnityEngine;
using UnityEngine.UI;

public class DangerWarning : MonoBehaviour
{
    [Header("UI + Sound Setup")]
    public GameObject warningPanel;           // 🔧 Drag your UI panel here
    public AudioClip warningSound;            // 🔧 Assign your warning sound here
    public AudioSource audioSource;           // 🔧 Drag the source (optional)

    private bool panelActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && warningPanel != null)
        {
            warningPanel.SetActive(true);
            panelActive = true;

            // 🎵 Play the warning sound
            if (warningSound != null)
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(warningSound);
                else
                    AudioSource.PlayClipAtPoint(warningSound, transform.position);
            }
        }
    }

    private void Update()
    {
        if (panelActive && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HideWarning();
        }

#if UNITY_EDITOR
        if (panelActive && Input.GetMouseButtonDown(0))
        {
            HideWarning();
        }
#endif
    }

    private void HideWarning()
    {
        warningPanel.SetActive(false);
        panelActive = false;
    }
}
