using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverController : MonoBehaviour, IPointerEnterHandler
{
    public UnityEvent onHover;
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hoverzinho");
        onHover.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
