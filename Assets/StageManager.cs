﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour {

	public enum GameState {Boat, Storm, Iceberg};
	public GameState state;
	public int gameDuration = 180;

	public GameObject iceberg_small;
	public Transform iceberg_origin;

	public GameObject sky_clear;
	public GameObject sky_stormy;

	// Use this for initialization
	void Start () {
		StartCoroutine (Stages ());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator Boat() {
		sky_clear.SetActive (true);
		sky_stormy.SetActive (false);
		while (state == GameState.Boat) {
			//send seaguls
			yield return new WaitForSeconds(1f);
		}
	}

	IEnumerator Storm() {
		sky_clear.SetActive (false);
		sky_stormy.SetActive (true);
		while (state == GameState.Storm) {
			//send small icebergs
			yield return new WaitForSeconds(5f);

			Instantiate (iceberg_small, iceberg_origin.position + new Vector3(0, Random.Range(-2f,2f), 0), iceberg_origin.rotation);
		}
	}

	IEnumerator Iceberg() {
		while (state == GameState.Iceberg) {
			//do the whole shizzle
			yield return new WaitForSeconds(1f);
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

		//gameover

	}
}
