using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class Grapple : MonoBehaviour {

    public float grapplePosInc = 0.5f;
    public float grappleSpeed = 0.05f;
    public float grapplePullInc = 0.5f;
    public float grapplePullSpeed = 0.05f;

    public float playerPushDelay = 0.4f;

    public float swingForce = 10f;
    public float launchForce = 100f;

    public LayerMask grappleIgnoreLayer;

    public Transform arrow;

    private LineRenderer grapple;
    private Vector2 arrowDirection;

    private Coroutine currentGrappleThrow = null;
    private Coroutine currentGrapplePull = null;
    private DistanceJoint2D currentHook = null;
    public GameObject grappledObject = null;

    private Transform player;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerR;

    private bool isSwinging { get { return currentHook != null && currentGrappleThrow == null; } }
    public bool playerGrapple = false;

	private GameManager GM;
	public Image loading;

	public Transform rod;

	void Start () {
        grapple = GetComponent<LineRenderer>();
        grapple.positionCount = 2;

        player = transform.parent;
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerR = GetComponentInParent<Rigidbody2D>();
		GM = GameObject.Find ("GameManager").GetComponent<GameManager>();
		rod = playerMovement.rod.transform.GetChild (0);
		rod.parent.GetComponent<SpriteRenderer> ().enabled = false;
    }
	
	void Update () {
        DirectionArrow();

        //if (Input.GetMouseButtonDown(0)) {
        //    GrappleTo(arrowDirection);

        //} else if (Input.GetMouseButtonUp(0)) {
        //    grapple.enabled = false;
        //    UnGrapple();
        //}
		if (SettingManager.GrappleUp(playerMovement.playerID) && grappledObject != null && !playerGrapple) {
			playerMovement.ac.SetTrigger ("Unhook");
		}

        if (!playerMovement.frozen && (SettingManager.GrappleUp(playerMovement.playerID)) && !playerGrapple || !SettingManager.Grapple(playerMovement.playerID) && !playerGrapple) {

            UnGrapple();
        }

          //  if (!playerGrapple && SettingManager.Grapple(playerMovement.playerID) && SettingManager.PullDown(playerMovement.playerID) && currentHook != null) {
		if (!playerGrapple && SettingManager.Grapple(playerMovement.playerID) && SettingManager.Pull(playerMovement.playerID) && currentHook != null) {
			if (currentGrapplePull == null) {
				currentGrapplePull = StartCoroutine (PullTo (currentHook.transform.position));
			}

        } else if (!playerGrapple && SettingManager.PullUp(playerMovement.playerID) && currentGrapplePull != null) {
			StopCoroutine(currentGrapplePull);
            currentGrapplePull = null;

            playerR.simulated = true;
        }

        if (playerGrapple && SettingManager.GrappleUp(playerMovement.playerID) && currentGrapplePull != null && !playerGrapple) {
            StopCoroutine(currentGrapplePull);
            currentGrapplePull = null;
            playerR.simulated = true;
        }

		if (isSwinging) {

			if (arrow.gameObject.activeSelf) {
				arrow.gameObject.SetActive (false);
				rod.parent.GetComponent<SpriteRenderer> ().enabled = true;

			}
			Vector3 playerPosition = new Vector3 (player.position.x, player.position.y, -1);
			Vector3 hookPosition = new Vector3 (currentHook.transform.position.x, currentHook.transform.position.y, -1);

			grapple.SetPosition (0, rod.position);
			grapple.SetPosition (1, hookPosition);

			if (SettingManager.Right (playerMovement.playerID)) {
				playerR.AddForce (playerR.transform.right * swingForce);
			}
			if (SettingManager.Left (playerMovement.playerID)) {
				playerR.AddForce (-playerR.transform.right * swingForce);
			}
			/*
            float range = 0.1f;
			if (player.position.y > currentHook.transform.position.y && (Vector2.Dot(playerR.velocity, playerR.transform.up) < range && Vector2.Dot(playerR.velocity, playerR.transform.up) > -range) && !playerGrapple) {
                Destroy(currentHook.gameObject);
                currentHook = null;
            }
            */

			player.GetComponent<SpriteRenderer> ().sortingOrder = 17;
		} else {
			if (!arrow.gameObject.activeSelf) {
				arrow.gameObject.SetActive (true);
				rod.parent.GetComponent<SpriteRenderer> ().enabled = false;

			}

			player.GetComponent<SpriteRenderer> ().sortingOrder = 14;

		}

        //Debug.Log(playerGrapple);
    }

    void DirectionArrow() {
        arrowDirection = SettingManager.GetAimVector(playerMovement.playerID, player.position);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(arrowDirection.y, arrowDirection.x) * Mathf.Rad2Deg + 90f);
    }

    void GrappleTo(Vector2 grappleDir) {
        grapple.enabled = true;

        RaycastHit2D hit = Physics2D.Raycast(arrow.position, grappleDir, 100, grappleIgnoreLayer);

        grapple.SetPosition(0, rod.position);
        grapple.SetPosition(1, hit.point);

        UnGrapple();

        playerGrapple = hit.collider.gameObject.tag == "Player";

        if (playerGrapple) {
            grappledObject = hit.collider.gameObject;
        } else {
            grappledObject = null;
        }
        currentGrappleThrow = StartCoroutine(ThrowGrapple(transform.position, hit.point));

        playerR.simulated = false;
    }

    public void UnGrapple() {

        grapple.enabled = false;

        if (currentHook != null) {
            Destroy(currentHook.gameObject);
            currentHook = null;
        }

        if (currentGrappleThrow != null) {
            StopCoroutine(currentGrappleThrow);
            currentGrappleThrow = null;
        }

        playerR.simulated = true;
        playerMovement.canMove = true;

		grappledObject = null;
    }

    IEnumerator ThrowGrapple(Vector2 from, Vector2 to) {

        float offset = 0f;
        float ropeLength = (to - from).magnitude;
        Vector2 grappleDir = (to - from).normalized;

        while (offset < ropeLength) {
            grapple.SetPosition(1, from + grappleDir * offset);
            offset += grapplePosInc;

            yield return new WaitForSeconds(grappleSpeed);
        }

        currentGrappleThrow = null;

        playerR.simulated = true;

       SecureHook(to);
    }

    IEnumerator PullTo(Vector2 pos, float speedMult = 1f) {
        while (currentHook != null && currentHook.distance > 1f) {


			Debug.Log (currentHook.distance);

			if (playerGrapple) {
				//grappledObject.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
			}

			Vector3 hookPosFixed = new Vector3 (currentHook.transform.position.x, currentHook.transform.position.y, 0);
			player.position = player.position + (hookPosFixed - player.position).normalized * grapplePullInc * speedMult;
			//player.position = player.position + (currentHook.transform.position - Vector3.back - player.position).normalized * grapplePullInc * speedMult;
            grapple.SetPosition(0, rod.position);

            yield return new WaitForSeconds(grapplePullSpeed);
        }

		//Debug.Log (currentHook.distance);

        currentGrapplePull = null;

        if (playerGrapple) {

			//player with the least score wins
			if (playerMovement.hit && playerMovement.playerID == grappledObject.GetComponent<PlayerMovement> ().hitBy) {
				//both hit
				if (GM.scores [playerMovement.playerID-1] >= GM.scores [grappledObject.GetComponent<PlayerMovement> ().playerID-1]) {
					//player loses
					Debug.Log("enemy wins");
					UnGrapple ();
				} else {
					//player wins
					Debug.Log("player wins");

					grappledObject.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
					grappledObject.GetComponent<PlayerMovement> ().frozen = true;

					grappledObject.GetComponent<PlayerMovement> ().paralized = true;

					//do the loading thing

					yield return new WaitForSeconds (playerPushDelay);


					grappledObject.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
					grappledObject.GetComponent<PlayerMovement> ().frozen = false;

					//TODO ADD THE VELOCITY CHANGES TO THE TARGET
					grappledObject.GetComponent<PlayerMovement> ().hit = false;
					grappledObject.GetComponent<PlayerMovement> ().paralized = false;


					//LaunchEnemy (((Vector2)Camera.main.ScreenToWorldPoint (Input.mousePosition) - pos).normalized);
					grappledObject.GetComponent<PlayerMovement>().grapple.UnGrapple();
					LaunchEnemy (SettingManager.GetAimVector(playerMovement.playerID,playerR.position));
					StartCoroutine (grappledObject.GetComponent<PlayerMovement> ().Disable ());

					UnGrapple ();

				}
			} else {
				//only enemy hit
				grappledObject.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
				grappledObject.GetComponent<PlayerMovement> ().frozen = true;

				grappledObject.GetComponent<PlayerMovement> ().paralized = true;

				//do the loading thing

				//yield return new WaitForSeconds (playerPushDelay);
				while (loading.fillAmount < 1) {
					loading.fillAmount += playerPushDelay / 15;
					yield return new WaitForFixedUpdate();
				}
				loading.fillAmount = 0;


				grappledObject.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
				grappledObject.GetComponent<PlayerMovement> ().frozen = false;

				//TODO ADD THE VELOCITY CHANGES TO THE TARGET
				grappledObject.GetComponent<PlayerMovement> ().hit = false;
				grappledObject.GetComponent<PlayerMovement> ().paralized = false;


				//LaunchEnemy (((Vector2)Camera.main.ScreenToWorldPoint (Input.mousePosition) - pos).normalized);
				grappledObject.GetComponent<PlayerMovement>().grapple.UnGrapple();
				LaunchEnemy (SettingManager.GetAimVector(playerMovement.playerID,playerR.position));
				StartCoroutine (grappledObject.GetComponent<PlayerMovement> ().Disable ());

				UnGrapple ();

			
			}


        } else {
            UnGrapple();

            playerR.AddForce(arrowDirection * launchForce);
			playerMovement.ac.SetTrigger ("Unhook");

        }
        
    }
	public void SecureHookImproved(GameObject hookPoint) {
		grapple.enabled = true;

		Rigidbody2D hookPointR = hookPoint.AddComponent<Rigidbody2D>();
		hookPointR.isKinematic = true;
		DistanceJoint2D joint = hookPoint.AddComponent<DistanceJoint2D>();
		joint.maxDistanceOnly = false;
		joint.connectedBody = playerR;


		currentHook = joint;

		//EditorApplication.isPaused = true;

		//add player grapple

		if (hookPoint.transform.parent.tag == "Player") {
			playerGrapple = true;
			grappledObject.GetComponent<PlayerMovement> ().hit = true;
			grappledObject.GetComponent<PlayerMovement> ().hitBy = playerMovement.playerID;
			currentGrapplePull = StartCoroutine(PullTo(currentHook.transform.position, 2f));
		} else {
			playerGrapple = false;
		}
	}


    public GameObject SecureHook(Vector2 at) {
        grapple.enabled = true;

        GameObject hookPoint = new GameObject("HookJoint");
        hookPoint.transform.position = at;

		if (grappledObject != null) {
			hookPoint.transform.parent = grappledObject.transform;
		}

        Rigidbody2D hookPointR = hookPoint.AddComponent<Rigidbody2D>();
        hookPointR.isKinematic = true;
        DistanceJoint2D joint = hookPoint.AddComponent<DistanceJoint2D>();
        joint.maxDistanceOnly = false;
        joint.connectedBody = playerR;

        currentHook = joint;

        if (playerGrapple) {
            //playerR.simulated = false;
            currentGrapplePull = StartCoroutine(PullTo(currentHook.transform.position, 2f));
        }

		return hookPoint;
    }

    void LaunchEnemy(Vector2 dir) {
		playerMovement.ac.SetTrigger ("Unhook");

        Rigidbody2D otherR = grappledObject.GetComponent<Rigidbody2D>();
        if (otherR == null) {
            return;
        }

        otherR.velocity = Vector2.zero;
        otherR.AddForce(dir.normalized * 1000f);

        playerR.simulated = true;
		playerGrapple = false;


		playerMovement.frozen = false;
		playerR.constraints = RigidbodyConstraints2D.FreezeRotation;

		playerR.velocity = playerMovement.previousVel;
		playerMovement.previousVel = Vector2.zero;
    }

	public void Reset() {
		playerMovement.ac.SetTrigger ("Unhook");

		Destroy(grappledObject.GetComponentInChildren<HookBehaviour> ().gameObject);
		playerGrapple = false;
		playerR.constraints = RigidbodyConstraints2D.FreezeRotation;
		playerR.velocity = playerMovement.previousVel;
		playerMovement.previousVel = Vector2.zero;
		currentHook = null;
		grappledObject = null;
		currentGrapplePull = null;
		playerMovement.canMove = true;
		playerMovement.frozen = false;
	}
}
