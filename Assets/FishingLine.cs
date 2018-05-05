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

    private PlayerMovement player;

	// Use this for initialization
	void Start () {
		sightLine = GetComponent<LineRenderer> ();

        player = GetComponentInParent<PlayerMovement>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
		//SimulatePath ();
		if (Input.GetKeyDown (KeyCode.Q)) {

			//StartCoroutine (Shoot ());
		}

	}

	public IEnumerator Shoot(Rigidbody2D player) {

		player.constraints = RigidbodyConstraints2D.FreezeAll;

		sightLine.positionCount = 0;
		sightLine.enabled = true;
		int throwing = 0;

		Vector3[] segments = new Vector3[segmentCount];
		int k = SimulatePath (segments);

		while (throwing < k + 1) {
			for (int i = 0; i < throwing; i++) {
				sightLine.positionCount = throwing;
				sightLine.SetPosition (i, segments [i]);
			}
			throwing++;
			yield return new WaitForFixedUpdate();

		}

		//sightLine.enabled = false;
		straightLine.SetPosition (0, fire.position);
		straightLine.SetPosition (1, segments [k]);

		throwing = 0;

		for (int i = sightLine.positionCount - 1; i > 0; i--) {
			sightLine.SetPosition (i, straightLine.GetPosition(1));
			yield return new WaitForSeconds (0.01f);
		}

		player.constraints = RigidbodyConstraints2D.FreezeRotation;

	}

	public int SimulatePath(Vector3[] segments) {
		//Vector3[] segments = new Vector3[segmentCount];
		segments [0] = fire.position + new Vector3 (0,0,-1);
		Vector3 segVelocity = -fire.transform.up * fireStrength * Time.deltaTime;
		_hitObject = null;

		for (int i = 1; i < segmentCount; i++) {
			float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;
			segVelocity = segVelocity + Physics.gravity * segTime;

			//Bounce shit
			RaycastHit2D hit;
			if (hit = Physics2D.Raycast (segments [i - 1], segVelocity, segmentScale)) {
				//			if (Physics2D.Raycast (segments [i - 1], segVelocity, out hit, segmentScale)) {
				//				_hitObject = hit.collider;
				//
				segments [i] = segments [i - 1] + segVelocity.normalized * hit.distance;
				segVelocity = segVelocity - Physics.gravity * (segmentScale - hit.distance) / segVelocity.magnitude;
				segVelocity = Vector3.Reflect (segVelocity, hit.normal);

                if (hit.collider.gameObject.tag == "Player") {
                    player.grapple.grappledObject = hit.collider.gameObject;
                    player.grapple.playerGrapple = true;
                } else {
                    player.grapple.playerGrapple = false;
                    if (hit.collider.gameObject.tag == "Ground") {
                        player.grapple.grappledObject = hit.collider.gameObject;

                    } else {
                        player.grapple.grappledObject = null;

                    }
                }


                return i;
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
		return segmentCount - 1;
	}
}
