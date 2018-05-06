using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingLineImproved : MonoBehaviour {

	public int segmentCount;
	public Transform fire;
	public Transform fireDir;

	public float fireStrength;
	public float segmentScale = 1;

	public LineRenderer sightLine;
	public RaycastHit2D hit;
	 
	public bool done = true;

	public Transform hook;
	//public LineRenderer straightLine;

	private PlayerMovement player;

	// Use this for initialization
	void Start () {
		sightLine = GetComponent<LineRenderer> ();
		player = GetComponentInParent<PlayerMovement>();


	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) {
			//StartCoroutine (SimulatePathTest ());
		}

		if (!done && hook != null) {
			hook.position = sightLine.GetPosition (sightLine.positionCount - 1);
		}
	}

	public IEnumerator SimulatePath() {

		if (done) {

			player._audio.Play (player._audio.woosh, true);

			done = false;

			sightLine.positionCount = 1;
			Vector3[] segments = new Vector3[segmentCount];
			segments [0] = fire.position + new Vector3 (0, 0, -1);
			sightLine.SetPosition (0, segments [0]);


			Vector3 segVelocity = -fireDir.transform.up * fireStrength * Time.deltaTime;

			for (int i = 1; i < segmentCount; i++) {
				float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;
				//segVelocity = segVelocity + Physics.gravity * segTime;
				segVelocity = segVelocity + new Vector3(0,-9,0) * segTime;

				//contact shit
				if (hit = Physics2D.Raycast (segments [i - 1], segVelocity, segmentScale)) {
					

					//Debug.Log ("Hitting Something: " + i);

					//bounce
					segments [i] = segments [i - 1] + segVelocity.normalized * hit.distance;
					//segVelocity = segVelocity - Physics.gravity * (segmentScale - hit.distance) / segVelocity.magnitude;
					//segVelocity = Vector3.Reflect (segVelocity, hit.normal);


					sightLine.positionCount++;
					sightLine.SetPosition (i, segments [i]);
					if (hit.transform.gameObject.tag == "Player" && (hit.transform.gameObject.GetComponent<PlayerMovement> ().disabled)) {
					} else if (hit.transform.gameObject.tag == "Player" && hit.transform.gameObject.GetComponentInParent<PlayerMovement> ().playerID == player.playerID) {
					} else {
					break;
					}
				} else {
					//Debug.Log ("Adding Segment");
					sightLine.positionCount++;
					segments [i] = segments [i - 1] + segVelocity * segTime;
					sightLine.SetPosition (i, segments [i]);
					//yield return new WaitForSeconds (0.5f);
					yield return new WaitForFixedUpdate ();
				}
			}

			if (!hit) {
				Destroy (hook.gameObject);
			}
			done = true;
		}
	}

	public IEnumerator SimulatePathTest() {
		while (true) {
			Vector3[] segments = new Vector3[segmentCount];
			segments [0] = fire.position + new Vector3 (0, 0, -1);
			//sightLine.SetPosition (0, segments [0]);

			int threshold = segmentCount;

			Vector3 segVelocity = -fire.transform.up * fireStrength * Time.deltaTime;

			for (int i = 1; i < segmentCount; i++) {
				float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;
				//segVelocity = segVelocity + Physics.gravity * segTime;
				segVelocity = segVelocity + new Vector3(0,-9,0) * segTime;
				//contact shit
				if (hit = Physics2D.Raycast (segments [i - 1], segVelocity, segmentScale)) {


					//Debug.Log ("Hitting Something: " + i);

					//bounce
					segments [i] = segments [i - 1] + segVelocity.normalized * hit.distance;
					//segVelocity = segVelocity - Physics.gravity * (segmentScale - hit.distance) / segVelocity.magnitude;
					//segVelocity = Vector3.Reflect (segVelocity, hit.normal);

					threshold = i;
					break;
					//sightLine.positionCount++;
					//sightLine.SetPosition (i, segments [i]);
				} else {
					//Debug.Log ("Adding Segment");
					//sightLine.positionCount++;
					segments [i] = segments [i - 1] + segVelocity * segTime;
					//sightLine.SetPosition (i, segments [i]);
					//yield return new WaitForSeconds (0.5f);
				}
			}
			sightLine.positionCount = threshold;

			for (int i = 0; i < threshold; i++) {
				sightLine.SetPosition (i, segments [i]);
			}
			yield return new WaitForFixedUpdate();

		}
	}
}
