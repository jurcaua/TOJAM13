using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour {

    public GameObject waitingFor;
    public Transform teleportTo;

	void OnTriggerEnter2D(Collider2D c) {
        if (c.gameObject == waitingFor) {
            c.transform.position = teleportTo.position;
        }
    }
}
