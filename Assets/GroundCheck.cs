using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour {

    bool jumped = false;
    PlayerMovement pm;

    void Start() {
        pm = GetComponentInParent<PlayerMovement>();
    }

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "Ground") {
			pm.grounded = true;
        }
	}

	void OnTriggerExit2D(Collider2D coll) {
		if (coll.tag == "Ground") {
			pm.grounded = false;
		}
	}
}
