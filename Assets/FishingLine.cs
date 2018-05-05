using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingLine : MonoBehaviour {

	public int segmentCount;
	public Transform fire;

	public float fireStrength;
	public float segmentScale = 1;

	public LineRenderer sightLine;
	private Collider2D _hitObject;
	public Collider2D hitObject { get { return _hitObject; } }

	public LineRenderer straightLine;

	// Use this for initialization
	void Start () {
		sightLine = GetComponent<LineRenderer> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//SimulatePath ();
		if (Input.GetKeyDown (KeyCode.Q)) {
			StartCoroutine (Shoot ());
		}

	}

	IEnumerator Shoot() {

		int throwing = 0;

		Vector3[] segments = new Vector3[segmentCount];
		SimulatePath (segments);

		while (throwing < segmentCount + 1) {
			for (int i = 0; i < throwing; i++) {
				sightLine.positionCount = throwing;
				sightLine.SetPosition (i, segments [i]);
			}
			throwing++;
			yield return new WaitForFixedUpdate();
		}
	}

	public void SimulatePath(Vector3[] segments) {
		//Vector3[] segments = new Vector3[segmentCount];
		segments [0] = fire.position;
		Vector3 segVelocity = -fire.transform.up * fireStrength * Time.deltaTime;
		_hitObject = null;

		for (int i = 1; i < segmentCount; i++) {
			float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;
			segVelocity = segVelocity + Physics.gravity * segTime;

			//Bounce shit
						RaycastHit2D hit;
			if (hit = Physics2D.Raycast(segments[i-1], segVelocity, segmentScale)) {
			//			if (Physics2D.Raycast (segments [i - 1], segVelocity, out hit, segmentScale)) {
			//				_hitObject = hit.collider;
			//
							segments [i] = segments [i - 1] + segVelocity.normalized * hit.distance;
							segVelocity = segVelocity - Physics.gravity * (segmentScale - hit.distance) / segVelocity.magnitude;
							segVelocity = Vector3.Reflect (segVelocity, hit.normal);

				straightLine.SetPosition (0, fire.position);
				straightLine.SetPosition (1, segments[i]);

						} else {

			segments [i] = segments [i - 1] + segVelocity * segTime;
						}
		}

		//color and transparency
		Color startColor = Color.white;
		Color endColor = startColor;
		startColor.a = 1;
		endColor.a = 0;
		sightLine.startColor = startColor;
		sightLine.endColor = endColor;

		sightLine.positionCount = segmentCount;

	}
}
