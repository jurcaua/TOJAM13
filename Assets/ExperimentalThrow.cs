using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentalThrow : MonoBehaviour {


	public GameObject hookPrefab;
	public GameObject hook;
	public Transform fire;
	public int force;

	public GameObject dot;

	public ContactPoint2D[] contacts = new ContactPoint2D[10];

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			hook = Instantiate (hookPrefab, transform.position, Quaternion.identity);
			hook.GetComponent<Rigidbody2D> ().AddForce (fire.up * force);

			StartCoroutine (DrawLine ());
		}
	}

	IEnumerator DrawLine() {
		if (hook != null) {
			float i = 0;
			while (i <= 3) {
				Instantiate (dot, hook.transform.position, Quaternion.identity);
				yield return new WaitForSeconds (0.1f);
				i += 0.1f;
				//hook.GetComponent<BoxCollider2D> ().GetContacts (contacts);

			}
		}
	}
}
