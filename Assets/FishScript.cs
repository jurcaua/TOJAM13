using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishScript : MonoBehaviour {

	public LayerMask ignore;
	public bool colliding = false;
	// Use this for initialization
	void Start () {
		//Physics2D.IgnoreLayerCollision (10, 10, true);
		//Debug.Log(gameObject.layer);
		StartCoroutine(StartColliding());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D coll) {
	//	Debug.Log (coll.tag);
		if (colliding) {
			if (coll.tag == "Player") {  
				coll.GetComponent<ScoreSystem> ().GetPoint ();
				Destroy (transform.parent.gameObject);

			}
		}
	}

	IEnumerator StartColliding() {
		yield return new WaitForSeconds (2f);
		colliding = true;
	}
}
