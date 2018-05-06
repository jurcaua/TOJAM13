using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour {
    
    private PlayerMovement pm;

    void Start() {
        pm = GetComponentInParent<PlayerMovement>();
    }

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "Ground") {
			pm.grounded = true;
			pm.ac.SetBool ("Grounded", true);
        }
	}

	void OnTriggerExit2D(Collider2D coll) {
		if (coll.tag == "Ground") {
			pm.grounded = false;
			pm.ac.SetBool ("Grounded", false);

		}
	}
}
