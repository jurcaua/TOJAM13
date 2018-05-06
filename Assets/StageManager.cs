using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour {

	public enum GameState {Boat, Storm, Iceberg};
	//public GameManager GM;
	public GameState state;
	public int gameDuration = 180;

	public GameObject seagull;
	public Transform seagull_origin;

	public GameObject iceberg_small;
	public Transform iceberg_origin;

	public GameObject sky_clear;
	public GameObject sky_stormy;


	public Animator boat;
	public GameObject[] platforms;
	public List<Rigidbody2D> players;
	public GameObject iceberg;

	// Use this for initialization
	void Start () {
		StartCoroutine (Stages ());
		players = new List<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator Boat() {
		sky_clear.SetActive (true);
		sky_stormy.SetActive (false);
		while (state == GameState.Boat) {
			//send seaguls
			yield return new WaitForSeconds(Random.Range(5,10));
			Instantiate (seagull, seagull_origin.position + new Vector3(0, Random.Range(0,15f), 0), seagull_origin.rotation);

		}
	}

	IEnumerator Storm() {
		sky_clear.SetActive (false);
		sky_stormy.SetActive (true);
		while (state == GameState.Storm) {
			//send small icebergs
			yield return new WaitForSeconds(Random.Range(2,5));

			Instantiate (iceberg_small, iceberg_origin.position + new Vector3(0, Random.Range(-2f,2f), 0), iceberg_origin.rotation);
		}
	}

	IEnumerator Iceberg() {
		yield return new WaitForSeconds(1f);

		iceberg.SetActive (true);
		boat.SetTrigger ("Iceberg");

		yield return new WaitForSeconds(1.15f);
		//int children = boat.transform.childCount;
		foreach (GameObject child in platforms) {
			child.SetActive (false);
		}

		foreach (Rigidbody2D rb in players) {
			rb.AddForce (Vector2.up * 5000);

		while (state == GameState.Iceberg) {
			//do the whole shizzle
				yield return new WaitForSeconds(1f);

			}

		}
	}



	IEnumerator Stages() {

		state = GameState.Boat;
		StartCoroutine (Boat ());
		yield return new WaitForSeconds (gameDuration / 3);

		state = GameState.Storm;
		StartCoroutine (Storm ());
		yield return new WaitForSeconds (gameDuration / 3);

		state = GameState.Iceberg;
		StartCoroutine (Iceberg ());
		yield return new WaitForSeconds (gameDuration / 3);
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.tag == "Player") {
			players.Add (coll.gameObject.GetComponent<Rigidbody2D> ());
		}
	}

	void OnTriggerExit2D(Collider2D coll) {
		if (coll.tag == "Player") {
			players.Remove (coll.gameObject.GetComponent<Rigidbody2D> ());
		}
	}
}
