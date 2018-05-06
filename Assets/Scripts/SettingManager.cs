﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamepadInput;

public class SettingManager : MonoBehaviour {

    public static bool HasBeenSetUp = false;
    public static int NumberOfPlayers = 1;
    public static List<ControlType> ControlSchemes = new List<ControlType>();

    public static bool DebugMode = true;

    private static bool[] AxisInUse = { false, false, false, false };

    // NEW FUNCTIONS --> RETURN BOOL

    public static GamePad.Index GetIndex(int playerID) {
        if (playerID == 1) {
            return GamePad.Index.One;

        } else if (playerID == 2) {
            return GamePad.Index.Two;

        } else if (playerID == 3) {
            return GamePad.Index.Three;

        } else {
            return GamePad.Index.Four;
        }
    }

    public static bool Left(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID) {
            if (ControlSchemes[playerID - 1] == ControlType.Keyboard) {
                return Input.GetKey(KeyCode.A);
            } else {
                Vector2 leftStickAxis = GamePad.GetAxis(GamePad.Axis.LeftStick, GetIndex(playerID));
                return leftStickAxis.x < 0 && Mathf.Abs(leftStickAxis.x) > Mathf.Abs(leftStickAxis.y);
            }
        } else {
            return Input.GetKeyDown(KeyCode.A);
        }
    }

    public static bool Right(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID) {
            if (ControlSchemes[playerID - 1] == ControlType.Keyboard) {
                return Input.GetKey(KeyCode.D);
            } else {
                Vector2 leftStickAxis = GamePad.GetAxis(GamePad.Axis.LeftStick, GetIndex(playerID));
                return leftStickAxis.x > 0 && Mathf.Abs(leftStickAxis.x) > Mathf.Abs(leftStickAxis.y);
            }
        } else {
            return Input.GetKeyDown(KeyCode.D);
        }
    }

    public static bool Jumpp(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID) {
            if (ControlSchemes[playerID - 1] == ControlType.Keyboard) {
                return Input.GetKey(KeyCode.W);
            } else {
                return GamePad.GetButtonDown(GamePad.Button.A, GetIndex(playerID));

                //For up joystick for jump (below)
                //Vector2 leftStickAxis = GamePad.GetAxis(GamePad.Axis.LeftStick, GetIndex(playerID));
                //return leftStickAxis.y > 0 && Mathf.Abs(leftStickAxis.y) > Mathf.Abs(leftStickAxis.x);
            }
        } else {
            return Input.GetKeyDown(KeyCode.W);
        }
    }

    public static bool GrappleDown(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID) {
            if (ControlSchemes[playerID - 1] == ControlType.Keyboard) {
                return Input.GetKeyDown(KeyCode.Mouse0);
            } else {
                return GamePad.GetButtonDown(GamePad.Button.LeftShoulder, GetIndex(playerID));
            }
        } else {
            return Input.GetKeyDown(KeyCode.Mouse0);
        }
    }

    public static bool GrappleUp(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID) {
            if (ControlSchemes[playerID - 1] == ControlType.Keyboard) {
                return Input.GetKeyUp(KeyCode.Mouse0);
            } else {
                return GamePad.GetButtonUp(GamePad.Button.LeftShoulder, GetIndex(playerID));
            }
        } else {
            return Input.GetKeyUp(KeyCode.Mouse0);
        }
    }

    public static bool PullDown(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID) {
            if (ControlSchemes[playerID - 1] == ControlType.Keyboard) {
                return Input.GetKeyDown(KeyCode.Mouse1);
            } else {
                return GamePad.GetButtonDown(GamePad.Button.RightShoulder, GetIndex(playerID));
            }
        } else {
            return Input.GetKeyDown(KeyCode.Mouse1);
        }
    }

    public static bool PullUp(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID) {
            if (ControlSchemes[playerID - 1] == ControlType.Keyboard) {
                return Input.GetKeyUp(KeyCode.Mouse1);
            } else {
                return GamePad.GetButtonUp(GamePad.Button.RightShoulder, GetIndex(playerID));
            }
        } else {
            return Input.GetKeyUp(KeyCode.Mouse1);
        }
    }

    public static Vector2 GetAimVector(int playerID, Vector3 playerPosition) {
        if (playerID > 0 && ControlSchemes.Count >= playerID) {
            if (ControlSchemes[playerID - 1] == ControlType.Keyboard) {
                return (Camera.main.ScreenToWorldPoint(Input.mousePosition) - playerPosition).normalized;
            } else {
                return GamePad.GetAxis(GamePad.Axis.RightStick, GetIndex(playerID)).normalized;
            }
        } else {
            return (Camera.main.ScreenToWorldPoint(Input.mousePosition) - playerPosition).normalized;
        }
    }

    // OLD FUNCTIONS --> USING KEYCODES

    public static KeyCode MoveLeft(int playerID) {
        if (playerID > 0 && ControlSchemes.Count >= playerID && ControlSchemes[playerID - 1] == ControlType.Keyboard) {
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
