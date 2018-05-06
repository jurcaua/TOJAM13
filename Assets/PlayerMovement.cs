using System.Collections;
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
	public FishingLineImproved newLine;
    public Grapple grapple;
	[HideInInspector] public bool frozen = false;
    public bool canMove = true;

    private Rigidbody2D r;
    public float maxVel;
	public GameObject hook;
	public GameObject hookPoint;

	public bool hit;
	public int hitBy;
	public bool paralized;

	public bool disabled = false;
	public float disabledTime;
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
			if (SettingManager.Right(playerID)) {
				if (xAcceleration >= 0) {
					xAcceleration = Mathf.Min (maxAcceleration, xAcceleration + speed);
				} else {
					xAcceleration = Mathf.Min (maxAcceleration, xAcceleration + speed * 2);
				}
				moving = true;
			}

			if (SettingManager.Left(playerID)) {
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
				if (SettingManager.Jump(playerID)) {
					grounded = false;
					GetComponent<Rigidbody2D> ().AddForce (Vector2.up * jumpForce);
				}
			}

			if (SettingManager.GrappleDown(playerID) && !disabled) {
				//StartCoroutine (Shoot (GetComponent<Rigidbody2D> ()));
				StartCoroutine (ShootImproved (GetComponent<Rigidbody2D> ()));
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

		if (hookPoint == null && !frozen) {
			//hook.SetActive (false);
		} else if (hookPoint != null){
			//hook.transform.position = hookPoint.transform.position;

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
	public IEnumerator ShootImproved(Rigidbody2D player) {
		
		canMove = false;
		frozen = true;
		player.constraints = RigidbodyConstraints2D.FreezeAll;


		previousVel = player.velocity;

		newLine.sightLine.enabled = true;
		GameObject _hook = Instantiate (hook);
		newLine.hook = _hook.transform;

		StartCoroutine (newLine.SimulatePath());

		while (!newLine.done) {
			yield return new WaitForFixedUpdate ();
		}

		//Debug.Log ("done");

		if (newLine.hit) {
			//Destroy (newLine.hit.collider.gameObject);
			GameObject contactObject = newLine.hit.collider.gameObject;


			int max = newLine.sightLine.positionCount - 1;
			_hook.transform.position = newLine.sightLine.GetPosition(max);
			_hook.transform.parent = contactObject.transform;
			//hook.transform.parent = _hook.transform;
			
			for (int i = max; i > 1; i--) {
				//newLine.sightLine.SetPosition (i - 1, newLine.sightLine.GetPosition (i));
				newLine.sightLine.SetPosition (i - 1, _hook.transform.position);

				newLine.sightLine.positionCount--;
				//yield return new WaitForSeconds(1);
				yield return new WaitForFixedUpdate ();
			}

			//while (Input.GetKey (KeyCode.Mouse0)) {
			//	newLine.sightLine.SetPosition (1, _hook.transform.position);
			//	yield return new WaitForFixedUpdate ();

			//}
			grapple.grappledObject = contactObject;
			grapple.SecureHookImproved (_hook);
		}
			
		newLine.sightLine.enabled = false;

		if (!grapple.playerGrapple) {
			frozen = false;
			player.constraints = RigidbodyConstraints2D.FreezeRotation;

			player.velocity = previousVel;
			previousVel = Vector2.zero;
		}

	}

	public IEnumerator Shoot(Rigidbody2D player) {

        canMove = false;
        frozen = true;
		LineRenderer lr = line.GetComponent<LineRenderer> ();
		//hook.SetActive (true);


        previousVel = player.velocity;

        player.constraints = RigidbodyConstraints2D.FreezeAll;

		lr.positionCount = 0;
		lr.enabled = true;

		grapple.grappledObject = null;

		Vector3[] segments = new Vector3[line.segmentCount];
		int k = line.SimulatePath (segments);

		Debug.Log (segments);
		//lr.positionCount = k + 2;

		for (int i = 0; i < k + 1; i++) {
			lr.positionCount = i + 1;
			lr.SetPosition (i, segments [i]);
			//hook.transform.position = segments [i];
			yield return new WaitForFixedUpdate();
		}


		//yield return new WaitForSeconds(10);

		//while (throwing < k + 1) {
		//	throwing++;



		//	lr.SetPosition (throwing, segments [throwing-1]);

			//for (int i = 0; i < throwing; i++) {
			//	lr.positionCount = throwing;
			//	lr.SetPosition (i, segments [i]);
			//}
		//	hook.transform.position = segments [throwing-1];

			//yield return new WaitForFixedUpdate();

		//}

		if (grapple.grappledObject != null) {
			//sightLine.enabled = false;
			line.straightLine.SetPosition (0, line.fire.position);
			line.straightLine.SetPosition (1, segments [k]);

			//throwing = 0;

			for (int i = lr.positionCount - 1; i > 0; i--) {
				lr.SetPosition (i, line.straightLine.GetPosition (1));
				yield return new WaitForFixedUpdate ();
			}
			lr.positionCount = 2;

			frozen = false;
			player.constraints = RigidbodyConstraints2D.FreezeRotation;

			hookPoint = grapple.SecureHook (lr.GetPosition (1));

		}

		frozen = false;
		player.constraints = RigidbodyConstraints2D.FreezeRotation;

		lr.enabled = false;
		line.straightLine.enabled = false;

		
        player.velocity = previousVel;
        previousVel = Vector2.zero;

    }

	public IEnumerator Disable() {
		disabled = true;
		//change color
		GetComponent<SpriteRenderer>().color = Color.red;

		yield return new WaitForSeconds (disabledTime);

		disabled = false;
		//change color back
		GetComponent<SpriteRenderer>().color = Color.white;

	}
}
