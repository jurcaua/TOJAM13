using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "Ground") {
			GetComponentInParent<PlayerMovement> ().grounded = true;
		}
	}

	void OnTriggerExit2D(Collider2D coll) {
		if (coll.tag == "Ground") {
			GetComponentInParent<PlayerMovement> ().grounded = false;
		}
	}
}
