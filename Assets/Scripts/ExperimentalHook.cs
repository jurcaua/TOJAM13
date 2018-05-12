using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExperimentalHook : MonoBehaviour {

	[Header("Transforms")]

	//public Transform hook;
	public Transform player;

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

	[Header("Arrow")]

	public GameObject arrow;
	public Camera main;
	//public LayerMask throwMask;

	[Header("Platforms")]

	public BoxCollider2D lastPlatform;
	public List<BoxCollider2D> platformsToPoints = new List<BoxCollider2D>();


	//line renderer throw
	[Header("Throw Curve")]

	public int segmentCount;
	public Transform fire;
	public float fireStrength;
	public float segmentScale = 1;
	public LineRenderer sightLine;
	public bool done = true;



	public List<Vector3> cornertest;
	public BoxCollider2D test;

	// Use this for initialization
	void Start () {
		lr.positionCount = 2;

		//currentHook.position = hook.position;
		dj = GetComponent<DistanceJoint2D> ();
		dj.connectedBody = currentHook.GetComponent<Rigidbody2D> ();
		dj.enabled = false;
		sightLine = GetComponent<LineRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {

		cornertest = GetCorners (test);


		Vector2 dirArrow = (Camera.main.ScreenToWorldPoint (new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)) - transform.position).normalized;
		arrow.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dirArrow.y, dirArrow.x) * Mathf.Rad2Deg + 90f);

		if (!hooked && Input.GetKeyDown (KeyCode.Mouse0)) {
		//	Hook ();
			Throw ();
		}
		if (hooked && Input.GetKeyUp (KeyCode.Mouse0)) {
			UnHook ();
		}

		if (hooked && Input.GetKey (KeyCode.Mouse1)) {
			dj.distance -= pullForce;
		}



		if (hooked) {
			Vector2 dir = currentHook.position - player.position; 
			if (hit = Physics2D.Raycast (player.position, dir, dir.magnitude)) {
				//conflict with current hook

				//Debug.Log ("folding");

				Vector2 hitDir = hit.point - new Vector2 (currentHook.position.x, currentHook.position.y);
					
				lastPlatform = hit.transform.GetComponent<BoxCollider2D> ();
				currentHook.position = GetClosestCorner (lastPlatform, hit.point);

				platformsToPoints.Add (lastPlatform);

				index++;

				//mid.position = hit.point - dir.normalized * 0.1f;

				lr.positionCount = index + 2;
				lr.SetPosition (index, currentHook.position);
				lr.SetPosition (index + 1, player.position);


				//transfers the current hook to mid
				currentHook.position = currentHook.position;
				dj.distance = hitDir.magnitude;
		
			} else {

				//TODO FIX THE GO BACK
				if (index > 0) {
					dir = lr.GetPosition (index - 1) - player.position; 
					hit = Physics2D.Raycast (player.position, dir, dir.magnitude);
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
				lr.SetPosition (index, currentHook.position );
				//lr.SetPosition (index + 1, player.position - new Vector3(0,0,1));
				lr.SetPosition (index + 1, player.position - new Vector3(0,0,1));
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

		} else {

			if (done) {
				if (Input.GetKeyDown (KeyCode.W)) {
					GetComponent<Rigidbody2D> ().AddForce (transform.up * jumpForce);
				}

				if (Input.GetKey (KeyCode.A)) {
					transform.position -= new Vector3 (speed, 0, 0);
				}
				if (Input.GetKey (KeyCode.D)) {
					transform.position += new Vector3 (speed, 0, 0);

				}
			}
		}
	}


	void Throw() {

		if (true) {
			StartCoroutine(SimulatePath());
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
				Hook ();
		
			}
		}

	}

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "Ground") {
			if (hooked) {
				UnHook ();
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


		//temporary
		//currentHook.position = hook.position;
	}

	void Hook() {
		hooked = true;
		dj.enabled = true;
		lr.enabled = true;


	}

	void DetachSingleHook() {
		platformsToPoints.RemoveAt(index);
		currentHook.position = lr.GetPosition (index - 1);
		lr.SetPosition (index - 1, player.position);

		lr.positionCount--;
		index--;
		dj.distance = (currentHook.position - player.position).magnitude;
	}

	List<Vector3> GetCorners (BoxCollider2D platform) {
		List<Vector3> corners = new List<Vector3> ();

		Transform trans = platform.transform;

		Vector3 angle = new Vector3 (Mathf.Cos (trans.rotation.eulerAngles.z * Mathf.PI / 180), Mathf.Sin (trans.rotation.eulerAngles.z * Mathf.PI / 180), 0f).normalized;
		Vector3 normal = new Vector3 (-Mathf.Sin (trans.rotation.eulerAngles.z * Mathf.PI / 180), Mathf.Cos (trans.rotation.eulerAngles.z * Mathf.PI / 180), 0f).normalized;
		Debug.Log (trans.rotation.eulerAngles.z + " " + normal);

		//for a flat platform
		//top right
		//corners.Add (new Vector3 ((trans.position.x + trans.localScale.x * platform.size.x * 0.5f + 0.01f), (trans.position.y + trans.localScale.y * platform.size.y * 0.5f + 0.01f), 0f) );
		corners.Add (new Vector3(trans.position.x, trans.position.y, 0f) + (trans.localScale.x * platform.size.x * 0.5f + 0.01f) * angle + normal * (trans.localScale.y * platform.size.y * 0.5f + 0.01f));
		//currentHook.position = corners [0];
		//top left
		//corners.Add (new Vector3 ((trans.position.x - trans.localScale.x * platform.size.x * 0.5f - 0.01f), (trans.position.y + trans.localScale.y * platform.size.y * 0.5f + 0.01f), 0f));
		corners.Add (new Vector3(trans.position.x, trans.position.y, 0f) - (trans.localScale.x * platform.size.x * 0.5f + 0.01f) * angle + normal * (trans.localScale.y * platform.size.y * 0.5f + 0.01f));

		//bottom right
		//corners.Add (new Vector3 ((trans.position.x + trans.localScale.x * platform.size.x * 0.5f + 0.01f), (trans.position.y - trans.localScale.y * platform.size.y * 0.5f - 0.01f), 0f));
		corners.Add (new Vector3(trans.position.x, trans.position.y, 0f) + (trans.localScale.x * platform.size.x * 0.5f + 0.01f) * angle - normal * (trans.localScale.y * platform.size.y * 0.5f + 0.01f));

		//bottom left
		//corners.Add (new Vector3 ((trans.position.x - trans.localScale.x * platform.size.x * 0.5f - 0.01f), (trans.position.y - trans.localScale.y * platform.size.y * 0.5f - 0.01f), 0f));
		corners.Add (new Vector3(trans.position.x, trans.position.y, 0f) - (trans.localScale.x * platform.size.x * 0.5f + 0.01f) * angle - normal * (trans.localScale.y * platform.size.y * 0.5f + 0.01f));



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

	//TODO Copy Pasted Code -> To Rework
	public IEnumerator SimulatePath() {

		if (done) {

			Rigidbody2D rb = GetComponent<Rigidbody2D> ();
			lr.enabled = true;
			previousVelocity = rb.velocity;
			rb.velocity = new Vector2 (0, 0);
			GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
			//player._audio.Play (player._audio.woosh, true);
			done = false;

			sightLine.positionCount = 1;
			Vector3[] segments = new Vector3[segmentCount];
			segments [0] = fire.position + new Vector3 (0, 0, -1);
			sightLine.SetPosition (0, segments [0]);


			Vector3 segVelocity = -fire.transform.up * fireStrength * Time.deltaTime;

			for (int i = 1; i < segmentCount; i++) {
				float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;
				//segVelocity = segVelocity + new Vector3(0,-9,0) * segTime;

				//contact shit
				if (hit = Physics2D.Raycast (segments [i - 1], segVelocity, segmentScale)) {

					segments [i] = segments [i - 1] + segVelocity.normalized * (hit.distance -0.1f);
					sightLine.positionCount++;
					sightLine.SetPosition (i, segments [i]);

					//if (hit.transform.gameObject.tag == "Player" && (hit.transform.gameObject.GetComponent<PlayerMovement> ().disabled)) {
					//} else if (hit.transform.gameObject.tag == "Player" && hit.transform.gameObject.GetComponentInParent<PlayerMovement> ().playerID == player.playerID) {
					//} else {
					break;
					//}
				} else {
					//Debug.Log ("Adding Segment");
					sightLine.positionCount++;
					segments [i] = segments [i - 1] + segVelocity * segTime;
					sightLine.SetPosition (i, segments [i]);
					yield return new WaitForFixedUpdate ();
				}
				//segments [0] = fire.position + new Vector3 (0, 0, -1);
				//sightLine.SetPosition (0, segments [0]);
			}

			if (hit) {
				int max = sightLine.positionCount - 1;

				//currentHook.position = sightLine.GetPosition (max);
				Vector3 curr = sightLine.GetPosition (max);
				//_hook.transform.position = newLine.sightLine.GetPosition (max);
				//_hook.transform.parent = contactObject.transform;
				//hook.transform.parent = _hook.transform;

				//for (int i = max; i > 1; i--) {
				//	//newLine.sightLine.SetPosition (i - 1, newLine.sightLine.GetPosition (i));
				//	sightLine.SetPosition (i - 1, curr);
				//
				//	sightLine.positionCount--;
				//	//yield return new WaitForSeconds(1);
				//	yield return new WaitForFixedUpdate ();
				//}

				currentHook.position = sightLine.GetPosition (max);
				sightLine.SetPosition (1, player.position - new Vector3 (0, 0, 1));
				sightLine.SetPosition (0, currentHook.position);


				lastPlatform = hit.transform.GetComponent<BoxCollider2D> ();
				platformsToPoints.Add (lastPlatform);
				Hook ();
			}

			GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
			rb.velocity = previousVelocity;

			done = true;
		}
	}
}
