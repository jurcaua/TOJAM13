using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookBehaviour : MonoBehaviour {


	//public PlayerMovement parent;
	//public bool contact;
	// Use this for initialization
	void Start () {
		//parent = transform.parent.GetComponent<PlayerMovement> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.rotation = Quaternion.Euler (transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
	}

	//void OnTriggerEnter2D(Collider2D coll) {
	//
	//	if (coll.gameObject.tag == "Player" && coll.GetComponent<PlayerMovement>().playerID != parent.playerID) {
	//		contact = true;
	//		//Destroy (coll.gameObject);
	//	}
	//
	//	if (coll.gameObject.tag == "Ground") {
	//		contact = true;
	//
	//	}
	//}
}
