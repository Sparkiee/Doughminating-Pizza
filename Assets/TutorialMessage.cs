using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float typingSpeed;
    [SerializeField] private float clearSpeed;

    private Coroutine typingCoroutine;
    private bool isPaused = false;

    public void ShowMessage(string message, Action onComplete = null)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeMessage(message, onComplete));
    }

    private IEnumerator TypeMessage(string message, Action onComplete)
    {
        messageText.text = "";
        foreach (char letter in message.ToCharArray())
        {
            messageText.text += letter;
            
            // Wait for pause to end, then wait for typing speed
            yield return StartCoroutine(WaitForUnpausedTime(typingSpeed));
        }

        onComplete?.Invoke();
    }

    public void ClearMessageBackwards(Action onComplete = null)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(ClearTextBackwards(onComplete));
    }

    private IEnumerator ClearTextBackwards(Action onComplete)
    {
        string currentText = messageText.text;
        for (int i = currentText.Length - 1; i >= 0; i--)
        {
            messageText.text = currentText.Substring(0, i);
            
            // Wait for pause to end, then wait for clear speed
            yield return StartCoroutine(WaitForUnpausedTime(clearSpeed));
        }
        
        if(this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
        }
        onComplete?.Invoke();
    }

    // Custom wait function that respects pause state
    private IEnumerator WaitForUnpausedTime(float waitTime)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < waitTime)
        {
            // Wait until the game is unpaused
            yield return new WaitUntil(() => Time.timeScale > 0f);
            
            // Add time based on scaled time
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    // Public method to pause/unpause tutorial messages
    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }
}