using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SettingManager {
    public static int NumberOfPlayers = 1;
    public static List<ControlType> ControlSchemes = new List<ControlType>();

    public static KeyCode MoveLeft(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID && ControlSchemes[playerID-1] == ControlType.Keyboard) {
            return KeyCode.A;
        } else {
            return KeyCode.A;
        }
    }

    public static KeyCode MoveRight(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID && ControlSchemes[playerID - 1] == ControlType.Keyboard) {
            return KeyCode.D;
        } else {
            return KeyCode.D;
        }
    }

    public static KeyCode Jump(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID && ControlSchemes[playerID - 1] == ControlType.Keyboard) {
            return KeyCode.W;
        } else {
            return KeyCode.W;
        }
    }

    public static KeyCode Grapple(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID && ControlSchemes[playerID - 1] == ControlType.Keyboard) {
            return KeyCode.Mouse0;
        } else {
            return KeyCode.Mouse0;
        }
    }

    public static KeyCode Pull(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID && ControlSchemes[playerID - 1] == ControlType.Keyboard) {
            return KeyCode.Mouse1;
        } else {
            return KeyCode.Mouse1;
        }
    }
}
