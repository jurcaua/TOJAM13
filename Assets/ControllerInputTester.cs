using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerInputTester : MonoBehaviour {

	void Update() {
        if (SettingManager.Jump(1)) {
            Debug.Log("Jump");
        }
        if (SettingManager.Left(1)) {
            Debug.Log("Left");
        }
        if (SettingManager.Right(1)) {
            Debug.Log("Right");
        }
        if (SettingManager.GrappleDown(1)) {
            Debug.Log("GrappleDown");
        }
        if (SettingManager.GrappleUp(1)) {
            Debug.Log("GrappleUp");
        }
        if (SettingManager.PullDown(1)) {
            Debug.Log("PullDown");
        }
        if (SettingManager.PullUp(1)) {
            Debug.Log("PullUp");
        }
        if (GamepadInput.GamePad.GetButtonDown(GamepadInput.GamePad.Button.RightStick, GamepadInput.GamePad.Index.One) || Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log(SettingManager.GetAimVector(1, Vector3.zero).ToString());
        }
    }
}
