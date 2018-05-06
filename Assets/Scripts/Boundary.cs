using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Boundary : MonoBehaviour {

    GameManager gm;

    void Start() {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
    }

	void OnTriggerEnter2D(Collider2D collison) {
        print("collided with: " + collison.gameObject.name);
        if (collison.gameObject.CompareTag("Player")) {
            gm.Respawn(collison.transform);
        }
    }
}
