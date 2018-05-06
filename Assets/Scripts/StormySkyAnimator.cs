using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormySkyAnimator : MonoBehaviour {

    private Animator a;

    void Awake() {
        a = GetComponent<Animator>();
    }

    void OnEnable() {
        a.Play("fade_in_sky");
    }
}
