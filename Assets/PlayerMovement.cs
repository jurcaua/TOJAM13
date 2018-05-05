﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public int playerID = -1;

	public float speed;
    [HideInInspector] public Vector2 previousVel = Vector2.zero;
	public float jumpForce;

	public float xAcceleration;
	public float maxAcceleration;

	public bool grounded;
	public bool falling;

	public float fallMultipler;
	public float lowJumpMultipler;
	public KeyCode jumpButton;

	public FishingLine line;
    public Grapple grapple;
	[HideInInspector] public bool frozen = false;
    public bool canMove = true;

    private Rigidbody2D r;
    public float maxVel;

	// Use this for initialization
	void Start () {
        r = GetComponent<Rigidbody2D>();

		speed = speed / 100;
	}

	void Update () {

        // Clamping Vel
        if (r.velocity.x > maxVel) {
            r.velocity = new Vector2(maxVel, r.velocity.y);
        }
        if (r.velocity.x < -maxVel) {
            r.velocity = new Vector2(-maxVel, r.velocity.y);
        }
        if (r.velocity.y > maxVel) {
            r.velocity = new Vector2(r.velocity.x, maxVel);
        }
        if (r.velocity.y < -maxVel) {
            r.velocity = new Vector2(r.velocity.x, -maxVel);
        }


        bool moving = false;

		if (!frozen) {
			if (Input.GetKey (SettingManager.MoveRight(playerID))) {
				if (xAcceleration >= 0) {
					xAcceleration = Mathf.Min (maxAcceleration, xAcceleration + speed);
				} else {
					xAcceleration = Mathf.Min (maxAcceleration, xAcceleration + speed * 2);
				}
				moving = true;
			}

			if (Input.GetKey (SettingManager.MoveLeft(playerID))) {
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

			if (grounded & !falling) {
				if (Input.GetKeyDown (SettingManager.Jump(playerID))) {
					grounded = false;
					GetComponent<Rigidbody2D> ().AddForce (Vector2.up * jumpForce);
				}
			}

			if (Input.GetKeyDown(SettingManager.Grapple(playerID))) {
				StartCoroutine (Shoot (GetComponent<Rigidbody2D> ()));
			}
		}
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (!frozen && canMove) {
			transform.position += new Vector3 (xAcceleration, 0, 0);

			Rigidbody2D r = GetComponent<Rigidbody2D> ();
			if (r.velocity.y < 0) {
				r.velocity += Vector2.up * Physics2D.gravity.y * (fallMultipler - 1) * Time.fixedDeltaTime;
			} 

			// if we are going upwards and are no longer holding "Jump", then lessen the amount we jump for
			else if (r.velocity.y > 0 && !Input.GetKey (jumpButton)) {
				r.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultipler - 1) * Time.deltaTime;
			}
		}
	}

	IEnumerator Freeze(bool condition) {
		while (!condition) {
			GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
			yield return new WaitForSeconds (0.1f);
		}
		GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;

	}
	//IEnumerator Jump() {
	//	float x = 0;
	//	float originY = transform.position.y;
	//
	//	while (!grounded) {
	//		float y = (2.5f * x) - 0.5f * x * x;
	//		x += 0.1f;
	//
	//		Debug.Log (y);
	//		Vector3 t = transform.position;
	//		transform.position = new Vector3(t.x, originY + y, t.z);
	//		yield return new WaitForFixedUpdate ();
	//		//yield return new WaitForSeconds (1);
	//	}
	//}

	public IEnumerator Shoot(Rigidbody2D player) {

        canMove = false;
        frozen = true;
		LineRenderer lr = line.GetComponent<LineRenderer> ();

        previousVel = player.velocity;

        player.constraints = RigidbodyConstraints2D.FreezeAll;

		lr.positionCount = 0;
		lr.enabled = true;
		int throwing = 0;

		grapple.grappledObject = null;

		Vector3[] segments = new Vector3[line.segmentCount];
		int k = line.SimulatePath (segments);

		while (throwing < k + 1) {
			for (int i = 0; i < throwing; i++) {
				lr.positionCount = throwing;
				lr.SetPosition (i, segments [i]);
			}
			throwing++;
			yield return new WaitForFixedUpdate();

		}

		if (grapple.grappledObject != null) {
			//sightLine.enabled = false;
			line.straightLine.SetPosition (0, line.fire.position);
			line.straightLine.SetPosition (1, segments [k]);

			throwing = 0;

			for (int i = lr.positionCount - 1; i > 0; i--) {
				lr.SetPosition (i, line.straightLine.GetPosition (1));
				yield return new WaitForFixedUpdate ();
			}
			lr.positionCount = 2;

			frozen = false;
			player.constraints = RigidbodyConstraints2D.FreezeRotation;

			grapple.SecureHook (lr.GetPosition (1));

		}

		frozen = false;
		player.constraints = RigidbodyConstraints2D.FreezeRotation;

		lr.enabled = false;
		line.straightLine.enabled = false;

		
        player.velocity = previousVel;
        previousVel = Vector2.zero;

    }
}
