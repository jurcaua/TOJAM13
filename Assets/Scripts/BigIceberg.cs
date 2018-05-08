using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigIceberg : MonoBehaviour {

	bool condition = true;
	// Use this for initialization
	void Start () {
		StartCoroutine (BounceTimer ());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter2D (Collision2D coll) {
		if (condition) {
			if (coll.gameObject.tag == "Player") {
				coll.gameObject.GetComponent<Rigidbody2D> ().AddForce (Vector2.up * 3000);
			}
		}
	}

	IEnumerator BounceTimer() {
		yield return new WaitForSeconds (4);
		condition = false;
	}
}
