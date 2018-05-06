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
        if (collison.gameObject.CompareTag("Player")) {
            StartCoroutine(DeathIndicator(collison.gameObject.GetComponentInParent<PlayerMovement>().playerID));
            gm.Respawn(collison.transform);

        } else if (collison.transform.parent != null && collison.transform.parent.gameObject.CompareTag("Player")) {
            StartCoroutine(DeathIndicator(collison.transform.parent.gameObject.GetComponentInParent<PlayerMovement>().playerID));
            gm.Respawn(collison.transform.parent);
        }
    }

    IEnumerator DeathIndicator(int playerID) {
        gm.texts[playerID - 1].text = string.Format("Player {0} PERISHED", playerID);
        yield return new WaitForSeconds(1f);
    }
}
