using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public GameObject player1;
	public GameObject player2;

	public string str;
	public float winnerY;

	public int p1Score;
	public int p2Score;

	public Text p1Text;
	public Text p2Text;

	public Slider p1Slider;
	public Slider p2Slider;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (player1.transform.position.y == player2.transform.position.y) {
			//tie
			str = "tie";
		} else if (player1.transform.position.y < player2.transform.position.y) {
			//player2 wins
			str = "player2";
			winnerY = player2.transform.position.y;

			p2Slider.value++;
			if (p2Slider.value == p2Slider.maxValue) {
				p2Score++;
				p2Slider.value = 0;
				p2Text.text = "P2: " + p2Score;
			}
			p1Slider.value = 0;


		} else {
			//player1 wins
			str = "player1";
			winnerY = player1.transform.position.y;
			p1Slider.value++;
			if (p1Slider.value == p1Slider.maxValue) {
				p1Score++;
				p1Slider.value = 0;
				p1Text.text = "P1: " + p1Score;
			}
			p2Slider.value = 0;



		}

		if (str != "tie") {
			GetComponent<LineRenderer> ().SetPosition (0, new Vector3(-18, winnerY, 0));
			GetComponent<LineRenderer> ().SetPosition (1, new Vector3(18, winnerY, 0));
		}
			
	}
}
