using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExperimentalHook : MonoBehaviour {

	[Header("Transforms")]

	//public Transform hook;
	public Transform player;
	public bool dummy = false;

	[Header("Line Renderer")]

	public LineRenderer lr;
	public RaycastHit2D hit;
	public int index = 0;
	public Transform currentHook;
	public bool hooked = false;
	public DistanceJoint2D dj;

	[Header("Movement")]

	public float speed;
	public float swingForce;
	public float jumpForce;
	public float pullForce;
	public Vector2 previousVelocity;

	public bool grounded = false;
	public int nJump = 0;
	public float airSpeed = 0;
	public bool invulnerable = false;

	[Header("Arrow")]

	public GameObject arrow;
	public Camera main;
	//public LayerMask throwMask;

	public GameObject target;

	[Header("Platforms")]

	public List<GameObject> hookPoints = new List<GameObject> ();
	public GameObject hookPoint;
	public BoxCollider2D lastPlatform;
	public List<BoxCollider2D> platformsToPoints = new List<BoxCollider2D>();


	//line renderer throw
	[Header("Throw Curve")]

	public int segmentCount;
	public Transform fire;
	public float fireStrength;
	public float segmentScale = 1;
	public LineRenderer sightLine;
	public LineRenderer throwLine;
	public bool done = true;
	public LayerMask lineLayer;
	public LayerMask contactLayer;
	public bool interrupted = false;

	[Header("Animation")]
	public Animator ac;


	[Header("Enemy Stuff")]
	bool enemyHit = false;
	bool immobile = false;
	GameObject enemyObject;

	public List<Vector3> cornertest;
	public BoxCollider2D test;

	// Use this for initialization
	void Start () {
		//lr.positionCount = 2;

		//currentHook.position = hook.position;
		dj = GetComponent<DistanceJoint2D> ();
		dj.connectedBody = currentHook.GetComponent<Rigidbody2D> ();
		dj.enabled = false;
		sightLine = GetComponent<LineRenderer> ();
	}

	void FixedUpdate() {
		if (Mathf.Abs (GetComponent<Rigidbody2D> ().velocity.x) >= 0.1) {
			
			ac.SetBool ("Moving", true);
		} else {
			ac.SetBool ("Moving", false);

		}

	}
	// Update is called once per frame
	void Update () {

		if (!dummy) {
			//cornertest = GetCorners (test);


			Vector2 dirArrow = (Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 10)) - transform.position).normalized;
			arrow.transform.rotation = Quaternion.Euler (0f, 0f, Mathf.Atan2 (dirArrow.y, dirArrow.x) * Mathf.Rad2Deg + 90f);

			if (!hooked && Input.GetKeyDown (KeyCode.Mouse0)) {
				//	Hook ();
				Throw ();
			}
			if (hooked && Input.GetKeyUp (KeyCode.Mouse0) && !enemyHit) {
				UnHook ();
			}

			if (hooked && Input.GetKey (KeyCode.Mouse1)) {
				dj.distance -= pullForce;
			}

			if (Input.GetKeyUp (KeyCode.Mouse0) && !done) {
				interrupted = true;
			}



			if (hooked) {
				if (!enemyHit) {
					Vector2 dir = currentHook.position - player.position; 
					if (hit = Physics2D.Raycast (player.position, dir, dir.magnitude, lineLayer)) {
						//conflict with current hook

						//Debug.Log ("folding");

						Vector2 hitDir = hit.point - new Vector2 (currentHook.position.x, currentHook.position.y);
					
						lastPlatform = hit.transform.GetComponent<BoxCollider2D> ();
						currentHook.position = GetClosestCorner (lastPlatform, hit.point);

						GameObject newPoint = new GameObject ();
						newPoint.transform.position = GetClosestCorner (lastPlatform, hit.point);
						newPoint.transform.parent = lastPlatform.transform;
						hookPoints.Add (newPoint);


						//dj.distance = (newPoint.transform.position - player.position).magnitude;
						StartCoroutine (DistanceJointCallibrating ());


						platformsToPoints.Add (lastPlatform);

						index++;

						//mid.position = hit.point - dir.normalized * 0.1f;

						lr.positionCount = index + 2;
						lr.SetPosition (index, currentHook.position);
						lr.SetPosition (index + 1, player.position);


						//transfers the current hook to mid
						currentHook.position = currentHook.position;
						//dj.distance = (currentHook.position - player.position).magnitude;

		
					} else {

						//TODO FIX THE GO BACK
						if (index > 0) {
							dir = lr.GetPosition (index - 1) - player.position; 
							hit = Physics2D.Raycast (player.position, dir, dir.magnitude, lineLayer);
							if (!hit) {

								//TODO Find A Better Logic For Knowing If Detach is needed or not
								if (lastPlatform == platformsToPoints [index - 1]) {
									DetachSingleHook ();

								} else {
									//Debug.Log ("ah");
									if (platformsToPoints [index].transform.position.y < platformsToPoints [index - 1].transform.position.y) {
										//platform is lower
										//get the slope
										Vector2 v1 = new Vector2 (lr.GetPosition (index).x, lr.GetPosition (index).y);
										Vector2 v2 = new Vector2 (lr.GetPosition (index - 1).x, lr.GetPosition (index - 1).y);
										float m = (v2.y - v1.y) / (v2.x - v1.x);

										float b = v1.y - m * v1.x;

										if (v1.y > platformsToPoints [index].transform.position.y) { 
											//top corner
											if (transform.position.y > m * transform.position.x + b) {
												//player is higher than point
												DetachSingleHook ();
											}
										} else {
											//bottom corner
											if (transform.position.y < m * transform.position.x + b) {
												//player is lower than point
												DetachSingleHook ();

											}
										}
									} else {
								
										Vector2 v1 = new Vector2 (lr.GetPosition (index).x, lr.GetPosition (index).y);
										Vector2 v2 = new Vector2 (lr.GetPosition (index - 1).x, lr.GetPosition (index - 1).y);
										float m = (v2.y - v1.y) / (v2.x - v1.x);
										float b = v1.y - m * v1.x;

										//if (transform.position.y > platformsToPoints [index].transform.position.y) {
										if (v1.y < platformsToPoints [index].transform.position.y) { 
											//top corner
											if (transform.position.y < m * transform.position.x + b) {
												//player is lower than point
												DetachSingleHook ();
											}
										} else {
											//bottom corner
											if (transform.position.y > m * transform.position.x + b) {
												//player is higher than point
												DetachSingleHook ();
											}
										}
									}
								}
								//EditorApplication.isPaused = true;
							}
						} 




						lr.positionCount = index + 2;
						lr.SetPosition (index, currentHook.position);
						//lr.SetPosition (index + 1, player.position - new Vector3(0,0,1));
						lr.SetPosition (index + 1, player.position - new Vector3 (0, 0, 1));

						if (target != null) {
							//player is attached as target
							lr.SetPosition (0, target.transform.position);

							if (lr.positionCount > 2) {
								dir = lr.GetPosition (1) - target.transform.position; 
								if (hit = Physics2D.Raycast (target.transform.position, dir, dir.magnitude)) {
									//conflict with current hook


									//TODO Add the new platform correctly in the same way as the position
									//
									lastPlatform = hit.transform.GetComponent<BoxCollider2D> ();
									Vector3 newPosition = GetClosestCorner (lastPlatform, hit.point);
									platformsToPoints.Add (lastPlatform);
									//

									lr.positionCount++;
									index++;

									Vector3[] positions = new Vector3[lr.positionCount];
									lr.GetPositions (positions);
									lr.SetPositions (AddAtIndex (positions, newPosition, 1));

								} else {
									//TODO add removing the fold
									dir = lr.GetPosition (2) - target.transform.position; 
									hit = Physics2D.Raycast (target.transform.position, dir, dir.magnitude);

									//TODO add the checking logic to that
									if (!hit) {
										DetachSingleHookAtIndex (1);
									}
								}
							}
						}
					}

					if (Input.GetKey (KeyCode.W)) {
						dj.distance -= pullForce;
					}
					if (Input.GetKey (KeyCode.S)) {
						dj.distance += pullForce;
					}

					if (Input.GetKey (KeyCode.A)) {
						GetComponent<Rigidbody2D> ().AddForce (-transform.right * swingForce);
					}
					if (Input.GetKey (KeyCode.D)) {
						GetComponent<Rigidbody2D> ().AddForce (transform.right * swingForce);
					}
				}

			} else {

				if (Input.GetKeyDown (KeyCode.Space)) {
					//StartCoroutine (GetHitCoroutine ());
					GetComponent<ScoreSystem>().LoosePoints(5);
				}

				//TODO better air control
				if (!invulnerable) {
					if (done && !immobile) {
						if (Input.GetKeyDown (KeyCode.W)) {
							if (nJump < 2) {
								ac.SetTrigger ("Jumping");
								GetComponent<Rigidbody2D> ().AddForce (transform.up * jumpForce);
								nJump++;
							}
						}

						if (Input.GetKey (KeyCode.A)) {
							if (grounded) {
								transform.position -= new Vector3 (speed, 0, 0);
							} else {
								GetComponent<Rigidbody2D> ().AddForce (-transform.right * airSpeed);
							}
						}
						if (Input.GetKey (KeyCode.D)) {
							if (grounded) {
								transform.position += new Vector3 (speed, 0, 0);
							} else {
								GetComponent<Rigidbody2D> ().AddForce (transform.right * airSpeed);
							}

						}
					}
				} else {
					//during invulnerability
				}
			
			}

			LineUpdate ();
		}
	}



	void Throw() {

		if (true) {
			StartCoroutine(SimulatePathNew());
		} else {
			//Debug.Log ("Throwing");

			Vector2 dir = arrow.transform.GetChild (0).position - transform.position;
			//Debug.Log (dir);
			RaycastHit2D hit;
			if (hit = Physics2D.Raycast (transform.position, dir)) {
				currentHook.position = hit.point - dir.normalized * 0.1f;
				//Debug.Log ("hit");
		
				lastPlatform = hit.transform.GetComponent<BoxCollider2D> ();
				platformsToPoints.Add (lastPlatform);

				GameObject newPoint = new GameObject ();
				newPoint.transform.position = hit.point - dir.normalized * 0.1f;
				newPoint.transform.parent = lastPlatform.transform;
				hookPoints.Add (newPoint);

				dj.distance = (newPoint.transform.position - player.position).magnitude;
				Hook ();

				if (hit.transform.gameObject.tag == "Player") {
					Debug.Log ("Player Hit");
					//start Reeling
					enemyObject = hit.transform.gameObject;
					StartCoroutine(EnemyHit());
				}

		
			}
		}

	}

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "Ground") {
			if (hooked && !enemyHit) {
				UnHook ();
			}

		} else if (coll.tag == "Player") {
			Debug.Log ("Player Collided");
			if (enemyHit) {
				enemyHit = false;
			}
		}
	}

	void UnHook() {
		hooked = false;
		dj.enabled = false;
		lr.enabled = false;
		//lr.positionCount = 0;
		index = 0;

		platformsToPoints.Clear ();

		target = null;


		foreach (GameObject go in hookPoints) {
			Destroy (go);
		}

		hookPoints.Clear ();
		enemyHit = false;

		//temporary
		//currentHook.position = hook.position;
	}

	void Hook() {
		hooked = true;
		dj.enabled = true;
		lr.enabled = true;

		//dj.distance = (currentHook.position - player.position).magnitude;

	}

	void DetachSingleHook() {
		platformsToPoints.RemoveAt(index);
		currentHook.position = lr.GetPosition (index - 1);
		lr.SetPosition (index - 1, player.position);

		lr.positionCount--;
		index--;

		currentHook.position = hookPoints[hookPoints.Count - 2].transform.position;

		//dj.distance = (currentHook.position - player.position).magnitude;
		StartCoroutine (DistanceJointCallibrating ());


		Destroy (hookPoints [hookPoints.Count - 1]);
		hookPoints.RemoveAt (hookPoints.Count - 1);
	}

	void DetachSingleHookAtIndex(int ind) {
		Vector3[] positions = new Vector3[lr.positionCount];
		lr.GetPositions (positions);
		lr.SetPositions (RemoveAtIndex (positions, ind));
		lr.positionCount--;
		index--;

		platformsToPoints.RemoveAt (ind);
	}


	List<Vector3> GetCorners (BoxCollider2D platform) {
		List<Vector3> corners = new List<Vector3> ();

		Transform trans = platform.transform;

		Vector3 angle = new Vector3 (Mathf.Cos (trans.rotation.eulerAngles.z * Mathf.PI / 180), Mathf.Sin (trans.rotation.eulerAngles.z * Mathf.PI / 180), 0f).normalized;
		Vector3 normal = new Vector3 (-Mathf.Sin (trans.rotation.eulerAngles.z * Mathf.PI / 180), Mathf.Cos (trans.rotation.eulerAngles.z * Mathf.PI / 180), 0f).normalized;
		//Debug.Log (trans.rotation.eulerAngles.z + " " + normal);

		float scale = 1;
		//for a flat platform
		//top right
		//corners.Add (new Vector3 ((trans.position.x + trans.localScale.x * platform.size.x * 0.5f + 0.01f), (trans.position.y + trans.localScale.y * platform.size.y * 0.5f + 0.01f), 0f) );
		corners.Add (new Vector3(trans.position.x, trans.position.y, 0f) + (trans.localScale.x * platform.size.x * 0.5f + 0.01f) * angle * scale + normal * (trans.localScale.y * platform.size.y * 0.5f + 0.01f) * scale);
		//currentHook.position = corners [0];
		//top left
		//corners.Add (new Vector3 ((trans.position.x - trans.localScale.x * platform.size.x * 0.5f - 0.01f), (trans.position.y + trans.localScale.y * platform.size.y * 0.5f + 0.01f), 0f));
		corners.Add (new Vector3(trans.position.x, trans.position.y, 0f) - (trans.localScale.x * platform.size.x * 0.5f + 0.01f) * angle * scale + normal * (trans.localScale.y * platform.size.y * 0.5f + 0.01f) * scale);

		//bottom right
		//corners.Add (new Vector3 ((trans.position.x + trans.localScale.x * platform.size.x * 0.5f + 0.01f), (trans.position.y - trans.localScale.y * platform.size.y * 0.5f - 0.01f), 0f));
		corners.Add (new Vector3(trans.position.x, trans.position.y, 0f) + (trans.localScale.x * platform.size.x * 0.5f + 0.01f) * angle * scale - normal * (trans.localScale.y * platform.size.y * 0.5f + 0.01f) * scale);

		//bottom left
		//corners.Add (new Vector3 ((trans.position.x - trans.localScale.x * platform.size.x * 0.5f - 0.01f), (trans.position.y - trans.localScale.y * platform.size.y * 0.5f - 0.01f), 0f));
		corners.Add (new Vector3(trans.position.x, trans.position.y, 0f) - (trans.localScale.x * platform.size.x * 0.5f + 0.01f) * angle * scale - normal * (trans.localScale.y * platform.size.y * 0.5f + 0.01f) * scale);



		return corners;
	}

	Vector3 GetClosestCorner (BoxCollider2D platform, Vector3 hitPoint) {
		List<Vector3> corners = GetCorners (platform);

		Vector3 result = new Vector3 ();
		float min = 1000;

		foreach (Vector3 v in corners) {
			//Debug.Log (v);
			if (Vector3.Distance(v, hitPoint) < min) {
				result = v - new Vector3 (0,0,1);
				min = Vector3.Distance (v, hitPoint);
			}
		}

		return result;

	}

	Vector3 GetPointAtSurface(BoxCollider2D platform, Vector3 hitPoint) {
		return new Vector3 ();
	}




	public IEnumerator SimulatePathNew() {
		//freeze the player

		if (done) {
			done = false;

			Rigidbody2D rb = GetComponent<Rigidbody2D> ();
			throwLine.enabled = true;
			previousVelocity = rb.velocity;
			rb.velocity = new Vector2 (0, 0);
			GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;

			//throw
			throwLine.positionCount = 1;
			Vector3[] segments = new Vector3[segmentCount];
			segments [0] = fire.position + new Vector3 (0, 0, -1);
			throwLine.SetPosition (0, segments [0]);


			Vector3 segVelocity = -fire.transform.up * fireStrength * 0.017f;
		//	Vector3 segVelocity = -fire.transform.up * fireStrength * Time.deltaTime;
			//Debug.Log (segVelocity + " " + Time.deltaTime);
			bool contact = false;
			int i = 0;
			for (i = 1; i < segmentCount; i++) {
				float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;
				segVelocity = segVelocity + new Vector3 (0, -9 / 2, 0) * segTime;

				if (hit = Physics2D.Raycast (segments [i - 1], segVelocity, segmentScale, contactLayer)) {

					if (hit.transform.tag == "Player" && hit.transform.gameObject.GetComponent<ExperimentalHook> ().invulnerable) {
						throwLine.positionCount++;
						segments [i] = segments [i - 1] + segVelocity * segTime;
						throwLine.SetPosition (i, segments [i]);
						yield return new WaitForFixedUpdate ();
					} else {

						segments [i] = segments [i - 1] + segVelocity.normalized * (hit.distance - 0.1f);
						throwLine.positionCount++;
						throwLine.SetPosition (i, segments [i]);

//						Debug.Log (hit.transform.tag);

						contact = true;
						break;

					}

				} else {
				
					throwLine.positionCount++;
					segments [i] = segments [i - 1] + segVelocity * segTime;
					throwLine.SetPosition (i, segments [i]);
					yield return new WaitForFixedUpdate ();
				}

			//	Debug.Log (contact);
			}
			if (contact) {
				Vector2 dir = segments [i] - transform.position;

				currentHook.position = hit.point - dir.normalized * 0.1f;
				//Debug.Log ("hit");

				lastPlatform = hit.transform.GetComponent<BoxCollider2D> ();
				platformsToPoints.Add (lastPlatform);

				GameObject newPoint = new GameObject ();
				newPoint.transform.position = hit.point - (dir.normalized * 0.1f);
				newPoint.transform.position += new Vector3 (0, 0, -1);
				newPoint.transform.parent = lastPlatform.transform;
				hookPoints.Add (newPoint);

				dj.distance = (newPoint.transform.position - player.position).magnitude;
				Hook ();

				if (hit.transform.gameObject.tag == "Player") {
					contact = false;
					Debug.Log ("Player Hit");
					//start Reeling
					enemyObject = hit.transform.gameObject;
					StartCoroutine (EnemyHit ());
				}
			}

			throwLine.enabled = false;


			GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
			rb.velocity = previousVelocity;
			done = true;

			if (interrupted && contact) { 
				UnHook (); 
			};
			interrupted = false;

		}
	}




	//TODO Copy Pasted Code -> To Rework
//	public IEnumerator SimulatePath() {
//
//		if (done) {
//
//			Rigidbody2D rb = GetComponent<Rigidbody2D> ();
//			lr.enabled = true;
//			previousVelocity = rb.velocity;
//			rb.velocity = new Vector2 (0, 0);
//			GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
//			//player._audio.Play (player._audio.woosh, true);
//			done = false;
//
//			throwLine.positionCount = 1;
//			Vector3[] segments = new Vector3[segmentCount];
//			segments [0] = fire.position + new Vector3 (0, 0, -1);
//			throwLine.SetPosition (0, segments [0]);
//
//
//			Vector3 segVelocity = -fire.transform.up * fireStrength * Time.deltaTime;
//
//			for (int i = 1; i < segmentCount; i++) {
//				float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;
//				//segVelocity = segVelocity + new Vector3(0,-9,0) * segTime;
//
//				//contact shit
//				if (hit = Physics2D.Raycast (segments [i - 1], segVelocity, segmentScale)) {
//
//					segments [i] = segments [i - 1] + segVelocity.normalized * (hit.distance -0.1f);
//					throwLine.positionCount++;
//					throwLine.SetPosition (i, segments [i]);
//
//					Debug.Log (hit.transform.tag);
//
//					if (hit.transform.tag == "Player") {
//						target = hit.transform.gameObject;
//					}
//
//
//					//if (hit.transform.gameObject.tag == "Player" && (hit.transform.gameObject.GetComponent<PlayerMovement> ().disabled)) {
//					//} else if (hit.transform.gameObject.tag == "Player" && hit.transform.gameObject.GetComponentInParent<PlayerMovement> ().playerID == player.playerID) {
//					//} else {
//
//					if (hit.transform.gameObject.tag == "Player") {
//						
//						done = true;
//					}
//
//					break;
//					//}
//				} else {
//					//Debug.Log ("Adding Segment");
//					throwLine.positionCount++;
//					segments [i] = segments [i - 1] + segVelocity * segTime;
//					throwLine.SetPosition (i, segments [i]);
//					yield return new WaitForFixedUpdate ();
//				}
//				//segments [0] = fire.position + new Vector3 (0, 0, -1);
//				//sightLine.SetPosition (0, segments [0]);
//			}
//
//			if (hit) {
//				int max = throwLine.positionCount - 1;
//
//				//currentHook.position = sightLine.GetPosition (max);
//				Vector3 curr = throwLine.GetPosition (max);
//				//_hook.transform.position = newLine.sightLine.GetPosition (max);
//				//_hook.transform.parent = contactObject.transform;
//				//hook.transform.parent = _hook.transform;
//
//				//for (int i = max; i > 1; i--) {
//				//	//newLine.sightLine.SetPosition (i - 1, newLine.sightLine.GetPosition (i));
//				//	sightLine.SetPosition (i - 1, curr);
//				//
//				//	sightLine.positionCount--;
//				//	//yield return new WaitForSeconds(1);
//				//	yield return new WaitForFixedUpdate ();
//				//}
//
//				currentHook.position = throwLine.GetPosition (max);
//				throwLine.SetPosition (1, player.position - new Vector3 (0, 0, 1));
//				throwLine.SetPosition (0, currentHook.position);
//
//
//
//				lastPlatform = hit.transform.GetComponent<BoxCollider2D> ();
//				platformsToPoints.Add (lastPlatform);
//				Hook ();
//
//
//				GameObject newPoint = new GameObject ();
//				newPoint.transform.position = GetClosestCorner (lastPlatform, hit.point);
//				newPoint.transform.parent = lastPlatform.transform;
//				hookPoints.Add (newPoint);
//			}
//
//			GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
//			rb.velocity = previousVelocity;
//
//			done = true;
//		}
//	}

	Vector3[] AddAtIndex(Vector3[] arr, Vector3 toAdd, int ind) {
		Vector3[] result = new Vector3[arr.Length + 1];

		for (int i = 0; i < ind; i++) {
			result [i] = arr [i];
			//Debug.Log ("first: " + result [i]);
		}

		result [ind] = toAdd;

		for (int i = ind; i < arr.Length - 1; i++) {
			result [i+1] = arr [i];
			//Debug.Log ( "second " + result[i+1]);
		}

		return result;
	}


	Vector3[] RemoveAtIndex(Vector3[] arr, int ind) {
		Vector3[] result = new Vector3[arr.Length - 1];

		for (int i = 0; i < ind; i++) {
			result [i] = arr [i];
			//Debug.Log ("first: " + result [i]);
		}

		for (int i = ind; i < arr.Length - 1; i++) {
			result [i] = arr [i+1];
			//Debug.Log ( "second " + result[i+1]);
		}

		return result;
	}


	void LineUpdate() {

		lr.positionCount = hookPoints.Count + 1;

		for (int i = 0; i < hookPoints.Count; i++) {
			lr.SetPosition (i, hookPoints [i].transform.position);
		}

		lr.SetPosition (hookPoints.Count, player.position);

		if (hookPoints.Count > 0) {
			currentHook.position = hookPoints [hookPoints.Count - 1].transform.position;
			//dj.distance = (currentHook.position - player.position).magnitude;

		}
	}

	IEnumerator DistanceJointCallibrating() {
		dj.autoConfigureDistance = true;
		yield return new WaitForFixedUpdate ();
		dj.autoConfigureDistance = false;

	}

	//TODO MAKE THIS MUCH BETTER
	IEnumerator EnemyHit() {
		enemyHit = true;
		immobile = true;
		//GetComponent<BoxCollider2D> ().isTrigger = true;

		int timer = 0;

		while (enemyHit) {
			dj.distance-= 0.2f;
			timer++;
			yield return new WaitForFixedUpdate ();

			if (timer >= 300) {
				break;
			}
		}
			

		//throwing happens here

		if (timer >= 300) {
			UnHook();

			GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
			enemyObject.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
			immobile = false;

		} else {
			GetComponent<Rigidbody2D> ().velocity = new Vector2(0,0);
			GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
			//GetComponent<BoxCollider2D> ().isTrigger = false;

			enemyObject.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;

			UnHook();

			yield return new WaitForSeconds (1f);
			immobile = false;

			enemyObject.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
			enemyObject.GetComponent<BoxCollider2D> ().isTrigger = true;
			Vector2 dir = arrow.transform.GetChild (0).position - transform.position;
			enemyObject.GetComponent<Rigidbody2D> ().AddForce (dir * 500);
			enemyObject.GetComponent<ExperimentalHook> ().GetHit ();

			yield return new WaitForSeconds (0.2f);
			enemyObject.GetComponent<BoxCollider2D> ().isTrigger = false;
			GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
		}


	}

	void OnCollisionEnter2D(Collision2D coll) {
//		Debug.Log (coll.gameObject.name);
		if (coll.gameObject.tag == "Ground") {
			grounded = true;
			nJump = 0;
		} else if (coll.gameObject.tag == "Player") {
			Debug.Log ("Player Collided");
			if (enemyHit) {
				enemyHit = false;
			} 
		} 
	}

	void OnCollisionExit2D(Collision2D coll) {
//		Debug.Log (coll.gameObject.name);
		if (coll.gameObject.tag == "Ground") {
			grounded = false;
			nJump = 1;
		}
	}

	public void GetHit() {
		StartCoroutine (GetHitCoroutine ());
		GetComponent<ScoreSystem> ().LoosePoints (3);
	}

	IEnumerator GetHitCoroutine() {
		invulnerable = true;
		GetComponent<SpriteRenderer> ().color = Color.red;
		yield return new WaitForSeconds (2f);
		invulnerable = false;
		GetComponent<SpriteRenderer> ().color = Color.white;

	}
}
