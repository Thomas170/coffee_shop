using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractElement : MonoBehaviour
{
    [SerializeField] protected GameObject hightlightRender;
    
    public virtual void Interact() { }

    protected void Start()
    {
        SetHightlight(false);
    }

    public void SetHightlight(bool value)
    {
        if (hightlightRender)
        {
            hightlightRender.SetActive(value);
        }
    }
}
