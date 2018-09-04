using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour {
    
	public GameObject Fish;

	public void GetPoint(int playerId) {
        //play animation
        Debug.Log(GameManager.instance.scores[playerId]);
        GameManager.instance.AddScore(playerId, 1);
        Debug.Log(GameManager.instance.scores[playerId]);
    }

	public void LosePoints(int playerId, int points) {
        if (playerId >= 0) {
            Debug.Log(GameManager.instance.scores[playerId]);
            GameManager.instance.SubtractScore(playerId, points);
            Debug.Log(GameManager.instance.scores[playerId]);

            for (int i = 0; i < points; i++) {
                //make the points fly away
                GameObject fish = Instantiate(Fish, transform.position + Vector3.up * 2, Quaternion.identity);
                float rand = Random.Range(-2f, 2f);
                float rand2 = Random.Range(0f, 2f);
                fish.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 200 * rand2 + Vector2.right * 100 * rand);
            }
        } else {
            Debug.Log("PlayerID of -1 was passed into \"LosePoints\".");
        }
	}
}
