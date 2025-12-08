using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class BubbleDialogue : NetworkBehaviour
{
    public event Action OnDialogueEnd;

    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePopup;
    [SerializeField] private TMP_Text dialogueText;

    private Queue<string> _sentences;
    private string _currentSentence;
    private float _typingSpeed = 0.06f;
    
    private void Awake()
    {
        _sentences = new Queue<string>();

        if (dialoguePopup != null)
            dialoguePopup.SetActive(false);
    }

    public void StartDialogue(string[] lines, float waitingTime = 3f, Action onAfter = null)
    {
        Action handler = null;
        handler = () =>
        {
            OnDialogueEnd -= handler;
            onAfter?.Invoke();
        };

        OnDialogueEnd += handler;
        
        if (lines == null || lines.Length == 0) return;

        _sentences.Clear();
        foreach (string sentence in lines)
        {
            _sentences.Enqueue(sentence);
        }

        dialoguePopup.SetActive(true);
        DisplayNextSentence(waitingTime);
    }

    public void DisplayNextSentence(float waitingTime = 3f)
    {
        if (_sentences.Count == 0)
        {
            OnDialogueEnd?.Invoke();
            HideDialogue();
            return;
        }

        _currentSentence = _sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(_currentSentence, waitingTime));
    }

    IEnumerator TypeSentence(string sentence, float waitingTime = 3f)
    {
        dialogueText.text = "";

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(_typingSpeed);
        }

        yield return new WaitForSeconds(3f);
        DisplayNextSentence();
    }
    
    public void HideDialogue()
    {
        dialoguePopup.SetActive(false);
    }
}
