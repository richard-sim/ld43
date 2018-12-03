using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputForwarder : MonoBehaviour, IPointerDownHandler {
    public PlayerController Player;
    
    public void OnPointerDown(PointerEventData eventData) {
        Player.OnForwardedPointerDown(this, eventData);
    }
}
