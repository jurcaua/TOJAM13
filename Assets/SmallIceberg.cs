using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallIceberg : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (transform.position.y >= -8.5) {
			GetComponent<SpriteRenderer> ().sortingOrder = 0;
		}

	}
	
	// Update is called once per frame
	void Update () {
		transform.position += transform.right * 0.2f;
	}

	void OnTriggerEnter2D(Collider2D coll) {
		//Debug.Log ("hahahaahah");
		if (coll.tag == "Boat") {
			coll.GetComponent<Animator> ().SetTrigger ("Hit");
		}
	}
}
