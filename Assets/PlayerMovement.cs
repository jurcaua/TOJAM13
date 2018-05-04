using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public float speed;
	public float jumpForce;

	public float xAcceleration;
	public float maxAcceleration;

	public bool grounded;
	// Use this for initialization
	void Start () {
		speed = speed / 100;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		//moving

		bool moving = false;

		if (Input.GetKey (KeyCode.D)) {
			if (xAcceleration >= 0) {
				xAcceleration = Mathf.Min (maxAcceleration, xAcceleration + speed);
			} else {
				xAcceleration = Mathf.Min (maxAcceleration, xAcceleration + speed * 2);
			}
			moving = true;
		}

		if (Input.GetKey (KeyCode.A)) {
			if (xAcceleration <= 0) {
				xAcceleration = Mathf.Max (-maxAcceleration, xAcceleration - speed);
			} else {
				xAcceleration = Mathf.Max (-maxAcceleration, xAcceleration - speed * 2);
			}
			moving = true;
		}

		if (!moving) {
			if (xAcceleration > 0) {
				xAcceleration -= speed * 2;
				if (xAcceleration < 0) {
					xAcceleration = 0;
				}
			} else if (xAcceleration < 0) {
				xAcceleration += speed * 2;
				if (xAcceleration > 0) {
					xAcceleration = 0;
				}
			}
		}

		transform.position += new Vector3 (xAcceleration, 0, 0);

		//jumping

		if (grounded) {
			if (Input.GetKeyDown (KeyCode.W)) {
				//GetComponent<Rigidbody2D> ().AddForce (Vector2.up * jumpForce);
				grounded = false;
				StartCoroutine (Jump());
			}
		} else if (GetComponent<Rigidbody2D> ().velocity.y < 0) {
			//GetComponent<Rigidbody2D> ().mass = 10;
			//GetComponent<Rigidbody2D> ().AddForce (Vector2.down * 100);

		}
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Ground") {
			//grounded = true;
			GetComponent<Rigidbody2D> ().mass = 1;

		}
	}

	IEnumerator Jump() {
		float x = 0;
		float max = 10;
		float originY = transform.position.y;

		GetComponent<Rigidbody2D> ().simulated = false;

		while (!grounded) {
			float y = 2 * x + (-1 / 2) * x * x;
			x += 1f;

			Debug.Log (y);
			transform.position += new Vector3(0, y, 0);
			//yield return new WaitForFixedUpdate ();
			yield return new WaitForSeconds (1);
		}
	}
}
