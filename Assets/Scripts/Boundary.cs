using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Boundary : MonoBehaviour {

    public bool isThreshold = false;
    public bool isWater = false;

    GameManager gm;
    AudioController audioC;

    void Start() {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        audioC = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioController>();

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
    }

	void OnTriggerEnter2D(Collider2D collison) {

        if (collison.gameObject.GetComponent<PlayerMovement>() != null) {
            if (isThreshold) {
                foreach (StormCloudBounce s in FindObjectsOfType<StormCloudBounce>()) {
                    s.Flash();
                }
                audioC.Play(audioC.thunder, true);
            }
            if (isWater) {
                audioC.Play(audioC.bigSplash, true);
            }
            StartCoroutine(DeathIndicator(collison.gameObject.GetComponentInParent<PlayerMovement>().playerID));
            gm.Respawn(collison.transform);
        }

        /*
        if (collison.gameObject.CompareTag("Player")) {
            if (isThreshold) {
                foreach(StormCloudBounce s in FindObjectsOfType<StormCloudBounce>()) {
                    s.Flash();
                }
            }
            if (isWater) {

            }
            StartCoroutine(DeathIndicator(collison.gameObject.GetComponentInParent<PlayerMovement>().playerID));
            gm.Respawn(collison.transform);

        } else if (collison.transform.parent != null && collison.transform.parent.gameObject.CompareTag("Player")) {
            if (isThreshold) {
                foreach (StormCloudBounce s in FindObjectsOfType<StormCloudBounce>()) {
                    s.Flash();
                }
            }
            StartCoroutine(DeathIndicator(collison.transform.parent.gameObject.GetComponentInParent<PlayerMovement>().playerID));
            gm.Respawn(collison.transform.parent);
        }
        */
    }

    IEnumerator DeathIndicator(int playerID) {
        gm.texts[playerID - 1].text = string.Format("Player {0} PERISHED", playerID);
        yield return new WaitForSeconds(1f);
    }
}
