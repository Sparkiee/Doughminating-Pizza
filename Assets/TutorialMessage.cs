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
            yield return new WaitForSecondsRealtime(typingSpeed);
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
            yield return new WaitForSecondsRealtime(clearSpeed);
        }
        if(this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
        }
        onComplete?.Invoke();
    }
}