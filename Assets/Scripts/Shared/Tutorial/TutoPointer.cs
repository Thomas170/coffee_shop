using UnityEngine;

public class TutoPointer : MonoBehaviour
{
    public float floatSpeed = 4.5f;
    public float floatHeight = 1f;

    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = _startPos + Vector3.up * offset;
    }
}