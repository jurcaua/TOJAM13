using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public float grapplePosInc = 0.5f;
    public float grappleSpeed = 0.05f;

    public LayerMask grappleIgnoreLayer;

    private LineRenderer grapple;
    private Vector2 arrowDirection;

    private Coroutine currentGrappleThrow = null;
    private DistanceJoint2D currentHook = null;

    private Rigidbody2D playerR;

    private bool isSwinging { get { return currentHook != null; } }

	void Start () {
        grapple = GetComponent<LineRenderer>();
        grapple.positionCount = 2;

        playerR = GetComponentInParent<Rigidbody2D>();
    }
	
	void Update () {
        DirectionArrow();

        if (Input.GetMouseButtonDown(0)) {
            GrappleTo(arrowDirection);

        } else if (Input.GetMouseButtonUp(0)) {
            UnGrapple();
        }

        if (isSwinging) {
            grapple.SetPosition(0, currentHook.connectedBody.position);
            grapple.SetPosition(1, currentHook.transform.position);
        }
    }

    void DirectionArrow() {
        arrowDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(arrowDirection.y, arrowDirection.x) * Mathf.Rad2Deg + 90f);
    }

    void GrappleTo(Vector2 grappleDir) {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, grappleDir, 100, grappleIgnoreLayer);

        grapple.positionCount = 2;
        grapple.SetPosition(0, transform.position);
        grapple.SetPosition(1, hit.point);

        UnGrapple();

        currentGrappleThrow = StartCoroutine(ThrowGrapple(transform.position, hit.point));
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

        grapple.positionCount = 0;
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

        SecureHook(to);
    }

    void SecureHook(Vector2 at) {
        GameObject hookPoint = new GameObject("HookJoint");
        hookPoint.transform.position = at;
        Rigidbody2D hookPointR = hookPoint.AddComponent<Rigidbody2D>();
        hookPointR.isKinematic = true;
        DistanceJoint2D joint = hookPoint.AddComponent<DistanceJoint2D>();
        joint.connectedBody = playerR;
        joint.maxDistanceOnly = false;

        currentHook = joint;
    }
}
