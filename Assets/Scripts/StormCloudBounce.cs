using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class StormCloudBounce : MonoBehaviour {

    private Animator a;

    void Awake() {
        a = GetComponent<Animator>();
    }

	void OnEnable () {
        a.Play("cloud_drop_down", -1, Random.Range(0f, 50f));
	}

    public void Flash() {
        a.SetTrigger("Flash");
    }
}
