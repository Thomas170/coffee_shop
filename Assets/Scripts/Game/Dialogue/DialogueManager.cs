using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public event Action OnDialogueEnd;

    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject passText;
    [SerializeField] private float typingSpeed = 0.03f;

    private Queue<string> _sentences;
    private bool _isTyping;
    private string _currentSentence;
    
    private Tween _passTween;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _sentences = new Queue<string>();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    private void Start()
    {
        InputReader.Instance.ActionAction.performed += OnNext;
    }

    private void OnDestroy()
    {
        if (InputReader.Instance != null)
            InputReader.Instance.ActionAction.performed -= OnNext;
    }

    public void StartDialogue(string[] lines)
    {
        PlayerIsDialogue(true);
        if (lines == null || lines.Length == 0) return;

        _sentences.Clear();
        foreach (string sentence in lines)
        {
            _sentences.Enqueue(sentence);
        }

        dialoguePanel.SetActive(true);
        StartPassAnimation();
        DisplayNextSentence();
    }

    private void OnNext(InputAction.CallbackContext ctx)
    {
        if (!dialoguePanel.activeSelf) return;

        if (_isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = _currentSentence;
            _isTyping = false;
        }
        else
        {
            DisplayNextSentence();
        }
    }

    public void DisplayNextSentence()
    {
        if (_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        _currentSentence = _sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(_currentSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        StopPassAnimation();
        
        OnDialogueEnd?.Invoke();
        PlayerIsDialogue(false);
    }
    
    public void PlayerIsDialogue(bool value)
    {
        PlayerController player = PlayerListManager.Instance?.GetPlayer(NetworkManager.Singleton.LocalClientId);
        if (player)
        {
            player.isInDialogue = value;
        }
    }
    
    private void StartPassAnimation()
    {
        if (!passText) return;

        _passTween?.Kill();

        _passTween = passText.transform.DOScale(1.1f, 0.8f)
            .SetLink(gameObject)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopPassAnimation()
    {
        _passTween?.Kill();
        passText.transform.localScale = Vector3.one;
    }
}
