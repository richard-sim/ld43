using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
//        Debug.Log($"Triggered: {other}");
        
        PlayerAgent agent = other.gameObject.GetComponentInParent<PlayerAgent>();
        if (agent != null) {
            agent.OnPlayerTriggerEnter(this.GetComponent<Collider>(), other);
        }
    }
}
