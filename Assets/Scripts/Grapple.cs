﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    public float grapplePosInc = 0.5f;
    public float grappleSpeed = 0.05f;
    public float grapplePullInc = 0.5f;
    public float grapplePullSpeed = 0.05f;

    public LayerMask grappleIgnoreLayer;

    public Transform arrow;

    private LineRenderer grapple;
    private Vector2 arrowDirection;

    private Coroutine currentGrappleThrow = null;
    private Coroutine currentGrapplePull = null;
    private DistanceJoint2D currentHook = null;

    private Transform player;
    private Rigidbody2D playerR;

    private bool isSwinging { get { return currentHook != null && currentGrappleThrow == null; } }
    private bool playerGrapple = false;

	void Start () {
        grapple = GetComponent<LineRenderer>();
        grapple.positionCount = 2;

        player = transform.parent;
        playerR = GetComponentInParent<Rigidbody2D>();
    }
	
	void Update () {
        DirectionArrow();

        if (Input.GetMouseButtonDown(0)) {
            GrappleTo(arrowDirection);

        } else if (Input.GetMouseButtonUp(0)) {
            grapple.enabled = false;
            UnGrapple();
        }

        if (!playerGrapple && Input.GetMouseButton(0) && Input.GetMouseButtonDown(1) && currentHook != null) {
            currentGrapplePull = StartCoroutine(PullTo(currentHook.transform.position));

        } else if (!playerGrapple && Input.GetMouseButtonUp(1) && currentGrapplePull != null) {
            StopCoroutine(currentGrapplePull);
            currentGrapplePull = null;
        }

        if (playerGrapple && Input.GetMouseButtonUp(1) && currentGrapplePull != null) {
            StopCoroutine(currentGrapplePull);
            currentGrapplePull = null;
            //playerR.isKinematic = false;
            //playerR.gravityScale = 1f;
            playerR.simulated = true;
        }

        if (isSwinging) {
            grapple.SetPosition(0, currentHook.connectedBody.position);
            grapple.SetPosition(1, currentHook.transform.position);
        }

        Debug.Log(playerGrapple);
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

        currentGrappleThrow = StartCoroutine(ThrowGrapple(transform.position, hit.point));
        playerR.simulated = false;

        playerGrapple = hit.collider.gameObject.tag == "Player";
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
        //playerR.isKinematic = false;
        //playerR.gravityScale = 1f;
        //playerR.mass = 1f;
        playerR.simulated = true;
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

        if (playerGrapple) {
            //playerR.isKinematic = true;
            //playerR.gravityScale = 0f;
            //playerR.mass = 0f;
            playerR.simulated = false;
            currentGrapplePull = StartCoroutine(PullTo(currentHook.transform.position, 2f));
        }
    }
}
