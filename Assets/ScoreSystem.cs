using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour {

	public int score;

	public GameObject Fish;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GetPoint() {
		//play animation
		score ++;
	}

	public void LoosePoints(int points) {
		score -= points;

		for (int i = 0; i < points; i++) {
			//make the points fly away
			GameObject fish = Instantiate(Fish, transform.position + Vector3.up* 2, Quaternion.identity);
			float rand = Random.Range (-2f, 2f);
			float rand2 = Random.Range (0f, 2f);
			fish.GetComponent<Rigidbody2D> ().AddForce (Vector2.up * 200 * rand2 + Vector2.right * 100 * rand);
		}
	}
}
