using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamepadInput;

public class ControllerInputTester : MonoBehaviour {

    GamePad.Index[] controllerIndicies = { GamePad.Index.One, GamePad.Index.Two, GamePad.Index.Three, GamePad.Index.Four };

    void Start() {
        int numControllers = 0;
        string[] joysticks = Input.GetJoystickNames();
        List<GamePad.Index> controllers = new List<GamePad.Index>(); 
        for (int i = 0; i < joysticks.Length; i++) {
            if (joysticks[i] != string.Empty) {
                numControllers++;
                controllers.Add(controllerIndicies[i]);
            }
        }
        Debug.Log(numControllers + " controllers found!");
        Debug.Log("Using the following GamePad indicies:");
        foreach (GamePad.Index i in controllers) {
            Debug.Log(i);
        }
        Debug.Log("----------");
    }

	void Update() {

        foreach (GamePad.Index i in controllerIndicies) {
            if (GamePad.GetButtonDown(GamePad.Button.A, i)) {
                Debug.Log("A - " + i);
            }
        }

        /*
        for (int i = 1; i < 5; i++) {
            if (SettingManager.Jump(i)) {
                Debug.Log("Jump" + i);
            }
            if (SettingManager.Left(i)) {
                Debug.Log("Left" + i);
            }
            if (SettingManager.Right(i)) {
                Debug.Log("Right" + i);
            }
            if (SettingManager.GrappleDown(i)) {
                Debug.Log("GrappleDown" + i);
            }
            if (SettingManager.GrappleUp(i)) {
                Debug.Log("GrappleUp" + i);
            }
            if (SettingManager.PullDown(i)) {
                Debug.Log("PullDown" + i);
            }
            if (SettingManager.PullUp(i)) {
                Debug.Log("PullUp" + i);
            }
            /*
            if (GamepadInput.GamePad.GetButtonDown(GamepadInput.GamePad.Button.RightStick, SettingManager.GetIndex(i)) || Input.GetKeyDown(KeyCode.Space)) {
                Debug.Log(i.ToString() + "-" + SettingManager.GetAimVector(1, Vector3.zero).ToString());
            }
            
        }*/
    }
}
