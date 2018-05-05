using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [HideInInspector] public GameObject grappledObject = null;

    private Transform player;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerR;

    private bool isSwinging { get { return currentHook != null && currentGrappleThrow == null; } }
    private bool playerGrapple = false;

	void Start () {
        grapple = GetComponent<LineRenderer>();
        grapple.positionCount = 2;

        player = transform.parent;
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerR = GetComponentInParent<Rigidbody2D>();
    }
	
	void Update () {
        DirectionArrow();

        //if (Input.GetMouseButtonDown(0)) {
        //    GrappleTo(arrowDirection);

        //} else if (Input.GetMouseButtonUp(0)) {
        //    grapple.enabled = false;
        //    UnGrapple();
        //}

        if (!playerMovement.frozen && (Input.GetKeyUp(SettingManager.Grapple(playerMovement.playerID)) || !Input.GetKey(SettingManager.Grapple(playerMovement.playerID)))) {
            grapple.enabled = false;
            UnGrapple();
        }

            if (!playerGrapple && Input.GetKey(SettingManager.Grapple(playerMovement.playerID)) && Input.GetKeyDown(SettingManager.Pull(playerMovement.playerID)) && currentHook != null) {
            currentGrapplePull = StartCoroutine(PullTo(currentHook.transform.position));

        } else if (!playerGrapple && Input.GetKeyUp(SettingManager.Pull(playerMovement.playerID)) && currentGrapplePull != null) {
            StopCoroutine(currentGrapplePull);
            currentGrapplePull = null;

            playerR.simulated = true;
        }

        if (playerGrapple && Input.GetKeyUp(SettingManager.Grapple(playerMovement.playerID)) && currentGrapplePull != null) {
            StopCoroutine(currentGrapplePull);
            currentGrapplePull = null;
            playerR.simulated = true;
        }

        if (isSwinging) {

            Vector3 playerPosition = new Vector3(player.position.x, player.position.y, -1);
            Vector3 hookPosition = new Vector3(currentHook.transform.position.x, currentHook.transform.position.y, -1);

            grapple.SetPosition(0, playerPosition);
            grapple.SetPosition(1, hookPosition);

            if (Input.GetKey(SettingManager.MoveRight(playerMovement.playerID))) {
                playerR.AddForce(playerR.transform.right * swingForce);
            }
            if (Input.GetKey(SettingManager.MoveLeft(playerMovement.playerID))) {
                playerR.AddForce(-playerR.transform.right * swingForce);
            }
            print(Vector2.Dot(playerR.velocity, playerR.transform.up));
            float range = 0.1f;
            if (player.position.y > currentHook.transform.position.y && (Vector2.Dot(playerR.velocity, playerR.transform.up) < range && Vector2.Dot(playerR.velocity, playerR.transform.up) > -range)) {
                Destroy(currentHook.gameObject);
                currentHook = null;
            }
        }

//        Debug.Log(playerGrapple);
    }

    void DirectionArrow() {
        arrowDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(arrowDirection.y, arrowDirection.x) * Mathf.Rad2Deg + 90f);
    }

    void GrappleTo(Vector2 grappleDir) {
        grapple.enabled = true;

        RaycastHit2D hit = Physics2D.Raycast(arrow.position, grappleDir, 100, grappleIgnoreLayer);

        grapple.SetPosition(0, transform.position);
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

    void UnGrapple() {
        if (currentHook != null) {
            Destroy(currentHook.gameObject);
            currentHook = null;
        }

        if (currentGrappleThrow != null) {
            StopCoroutine(currentGrappleThrow);
            currentGrappleThrow = null;
        }

        playerMovement.canMove = true;
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

            player.position = player.position + (currentHook.transform.position - player.position).normalized * grapplePullInc * speedMult;
            grapple.SetPosition(0, player.position);

            yield return new WaitForSeconds(grapplePullSpeed);
        }

        currentGrapplePull = null;

        if (playerGrapple) {
            yield return new WaitForSeconds(playerPushDelay);

            LaunchEnemy(((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - pos).normalized);
        } else {
            UnGrapple();
            playerR.AddForce(arrowDirection * launchForce);
        }
        
    }

    public void SecureHook(Vector2 at) {
        grapple.enabled = true;

        GameObject hookPoint = new GameObject("HookJoint");
        hookPoint.transform.position = at;
        Rigidbody2D hookPointR = hookPoint.AddComponent<Rigidbody2D>();
        hookPointR.isKinematic = true;
        DistanceJoint2D joint = hookPoint.AddComponent<DistanceJoint2D>();
        joint.maxDistanceOnly = false;
        joint.connectedBody = playerR;

        currentHook = joint;

        if (playerGrapple) {
            playerR.simulated = false;
            currentGrapplePull = StartCoroutine(PullTo(currentHook.transform.position, 2f));
        }
    }

    void LaunchEnemy(Vector2 dir) {
        Rigidbody2D otherR = grappledObject.GetComponent<Rigidbody2D>();
        if (otherR == null) {
            return;
        }

        otherR.velocity = Vector2.zero;
        otherR.AddForce(dir * 1000f);

        playerR.simulated = true;
    }
}
