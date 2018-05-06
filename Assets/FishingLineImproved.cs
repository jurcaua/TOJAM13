using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingLineImproved : MonoBehaviour {

	public int segmentCount;
	public Transform fire;

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
		if (Input.GetKeyDown(KeyCode.Space) && done) {
			//StartCoroutine (SimulatePath ());
		}

		if (!done && hook != null) {
			hook.position = sightLine.GetPosition (sightLine.positionCount - 1);
		}
	}

	public IEnumerator SimulatePath() {
		if (done) {
			done = false;

			sightLine.positionCount = 1;
			Vector3[] segments = new Vector3[segmentCount];
			segments [0] = fire.position + new Vector3 (0, 0, -1);
			sightLine.SetPosition (0, segments [0]);


			Vector3 segVelocity = -fire.transform.up * fireStrength * Time.deltaTime;

			for (int i = 1; i < segmentCount; i++) {
				float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;
				segVelocity = segVelocity + Physics.gravity * segTime;

				//contact shit
				if (hit = Physics2D.Raycast (segments [i - 1], segVelocity, segmentScale)) {
					

					//Debug.Log ("Hitting Something: " + i);

					//bounce
					segments [i] = segments [i - 1] + segVelocity.normalized * hit.distance;
					//segVelocity = segVelocity - Physics.gravity * (segmentScale - hit.distance) / segVelocity.magnitude;
					//segVelocity = Vector3.Reflect (segVelocity, hit.normal);


					sightLine.positionCount++;
					sightLine.SetPosition (i, segments [i]);
					if (hit.transform.gameObject.tag == "Player" && hit.transform.gameObject.GetComponent<PlayerMovement>().disabled) {
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
}
