using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAgent : MonoBehaviour {
    public bool IsCommander = false;

    [System.NonSerialized] public PlayerController Player;

    public void OnPlayerTriggerEnter(Collider other, Collider thisCollider) {
//        Debug.Log($"{gameObject.name} entered trigger: {other}. Original collider (this): {thisCollider}.");

        Player.AgentOnTriggerEnter(this, other);
    }
}
