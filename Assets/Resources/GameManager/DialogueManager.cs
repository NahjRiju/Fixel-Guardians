using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image characterImage;
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public GameObject dialogPanel;

    [Header("Settings")]
    public float typingSpeed = 0.05f;

    private AudioSource audioSource;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isDialogueActive = false;

    private LevelConfig.DialogueData[] currentDialogues;
    private int currentDialogueIndex = 0;

    public delegate void DialogueFinishedHandler();
    public event DialogueFinishedHandler OnDialogueFinished;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("AudioSource was dynamically added to DialogueManager.");
        }

        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        Debug.Log("DialogueManager initialized.");
    }

    public void SetupDialogue(LevelConfig.DialogueData[] dialogues)
    {
        currentDialogues = dialogues;
        currentDialogueIndex = 0;
        currentLineIndex = 0;

        ShowCurrentDialogue();
    }

    public void StartDialogue()
    {
        if (currentDialogues.Length > 0 && currentDialogues[0].dialogueEntries.Length > 0)
        {
            Debug.Log("Dialogue session starting...");
            isDialogueActive = true;

            if (dialogPanel != null)
            {
                dialogPanel.SetActive(true);
            }

            ShowCurrentDialogue();
            PlayAudio(currentLineIndex);
            StartCoroutine(TypeLine(currentDialogues[currentDialogueIndex].dialogueEntries[currentLineIndex].dialogueLine));
        }
        else
        {
            Debug.LogWarning("No dialogue entries found!");
        }
    }

    void Update()
    {
        if (isDialogueActive)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                HandleInput();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                HandleInput();
            }
        }
    }

    private void HandleInput()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentDialogues[currentDialogueIndex].dialogueEntries[currentLineIndex].dialogueLine;
            isTyping = false;
            Debug.Log($"Typing effect completed for line: {currentLineIndex}");
        }
        else
        {
            currentLineIndex++;

            if (currentLineIndex < currentDialogues[currentDialogueIndex].dialogueEntries.Length)
            {
                Debug.Log($"Advancing to line: {currentLineIndex}, Text: {currentDialogues[currentDialogueIndex].dialogueEntries[currentLineIndex].dialogueLine}");
                PlayAudio(currentLineIndex);
                StartCoroutine(TypeLine(currentDialogues[currentDialogueIndex].dialogueEntries[currentLineIndex].dialogueLine));
            }
            else
            {
                currentDialogueIndex++;
                currentLineIndex = 0;

                if (currentDialogueIndex < currentDialogues.Length)
                {
                    ShowCurrentDialogue();
                    PlayAudio(currentLineIndex);
                    StartCoroutine(TypeLine(currentDialogues[currentDialogueIndex].dialogueEntries[currentLineIndex].dialogueLine));
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        Debug.Log($"Typing line: {line}");
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        Debug.Log($"Typing finished for line: {line}");
    }

    private void PlayAudio(int index)
    {
        if (index < currentDialogues[currentDialogueIndex].dialogueEntries.Length && currentDialogues[currentDialogueIndex].dialogueEntries[index].voiceClip != null)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = currentDialogues[currentDialogueIndex].dialogueEntries[index].voiceClip;
                audioSource.Play();
                Debug.Log($"Playing audio for line {index}");
            }
            else
            {
                Debug.LogError("AudioSource is missing! Unable to play audio.");
            }
        }
        else
        {
            Debug.LogWarning($"No audio clip assigned for line {index}");
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    private void EndDialogue()
    {
        Debug.Log("Dialogue session finished.");
        isDialogueActive = false;

        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        dialogueText.text = "";
        OnDialogueFinished?.Invoke();
    }

    private void ShowCurrentDialogue()
    {
        if (currentDialogues.Length > 0)
        {
            characterNameText.text = currentDialogues[currentDialogueIndex].characterName;

            if (characterImage != null && currentDialogues[currentDialogueIndex].characterImage != null)
            {
                characterImage.sprite = currentDialogues[currentDialogueIndex].characterImage;

                // Apply RectTransform properties
                RectTransform imageRect = characterImage.rectTransform;
                imageRect.anchoredPosition = currentDialogues[currentDialogueIndex].imagePosition;
                imageRect.localEulerAngles = currentDialogues[currentDialogueIndex].imageRotation;
                imageRect.localScale = currentDialogues[currentDialogueIndex].imageScale;
            }
            else if (characterImage != null)
            {
                characterImage.sprite = null;
            }
        }
    }
}