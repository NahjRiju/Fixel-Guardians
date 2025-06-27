using UnityEngine;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance; // Singleton pattern for easy access

    public GameObject popupPanel;  // The panel that holds the popup UI
   

    private void Awake()
    {
        // Singleton Setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // Ensure only one instance exists
        }
    }

    public void ShowPopup()
    {
        popupPanel.SetActive(true); // Show the popup panel

        // Optionally, update the image and text dynamically here
        // fragmentImage.GetComponent<Image>().sprite = yourFragmentImage; 
        // Add other dynamic updates for your popup content

        // Enable tap to close
        StartCoroutine(WaitForTapToClose());
    }

    private IEnumerator WaitForTapToClose()
    {
        // Wait for any touch/click on the screen
        bool touched = false;

        while (!touched)
        {
            if (Input.GetMouseButtonDown(0)) // Detect left mouse click or screen tap
            {
                touched = true;
            }

            yield return null;
        }

        // Close the panel once tapped
        popupPanel.SetActive(false);
    }
}
