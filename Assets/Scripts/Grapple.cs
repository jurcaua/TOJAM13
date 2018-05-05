using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public float grapplePosInc = 0.5f;
    public float grappleSpeed = 0.05f;
    public float grapplePullInc = 0.5f;
    public float grapplePullSpeed = 0.05f;

    public float playerPushDelay = 0.4f;

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

        if (!playerMovement.frozen && (Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0))) {
            grapple.enabled = false;
            UnGrapple();
        }

            if (!playerGrapple && Input.GetMouseButton(0) && Input.GetMouseButtonDown(1) && currentHook != null) {
            currentGrapplePull = StartCoroutine(PullTo(currentHook.transform.position));

        } else if (!playerGrapple && Input.GetMouseButtonUp(1) && currentGrapplePull != null) {
            StopCoroutine(currentGrapplePull);
            currentGrapplePull = null;

            playerR.simulated = true;
        }

        if (playerGrapple && Input.GetMouseButtonUp(0) && currentGrapplePull != null) {
            StopCoroutine(currentGrapplePull);
            currentGrapplePull = null;
            playerR.simulated = true;
        }

        if (isSwinging) {

            Vector3 playerPosition = new Vector3(currentHook.connectedBody.position.x, currentHook.connectedBody.position.y, -1);
            Vector3 hookPosition = new Vector3(currentHook.transform.position.x, currentHook.transform.position.y, -1);

            grapple.SetPosition(0, playerPosition);
            grapple.SetPosition(1, hookPosition);
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
        while (currentHook != null && currentHook.distance > 0.5f) {

            player.position = player.position + (currentHook.transform.position - player.position) * grapplePullInc * speedMult;
            grapple.SetPosition(0, player.position);

            yield return new WaitForSeconds(grapplePullSpeed);
        }

        currentGrapplePull = null;

        if (playerGrapple) {
            yield return new WaitForSeconds(playerPushDelay);

            LaunchEnemy(((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - pos).normalized);
        }
        
    }

    public void SecureHook(Vector2 at) {
        grapple.enabled = true;

        GameObject hookPoint = new GameObject("HookJoint");
        hookPoint.transform.position = at;
        Rigidbody2D hookPointR = hookPoint.AddComponent<Rigidbody2D>();
        hookPointR.isKinematic = true;
        DistanceJoint2D joint = hookPoint.AddComponent<DistanceJoint2D>();
        joint.connectedBody = playerR;
        joint.maxDistanceOnly = false;

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
