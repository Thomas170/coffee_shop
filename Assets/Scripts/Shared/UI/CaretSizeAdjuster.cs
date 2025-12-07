using System.Collections;
using TMPro;
using UnityEngine;

public class CaretSizeAdjuster : MonoBehaviour
{
    public TMP_InputField inputField;
    public float caretHeight = 20f;

    private void Start()
    {
        StartCoroutine(FixCaret());
    }

    private IEnumerator FixCaret()
    {
        yield return null; // attendre création du caret

        Transform caret = inputField.transform.Find("Text Area/Caret");
        if (caret)
        {
            var rt = caret.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, caretHeight);
        }
    }
}