using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ControlType {Keyboard = 0, Controller = 1};

[CreateAssetMenu()]
public class PlayerControl : ScriptableObject {
    public int playerNum;
    public ControlType controlType;
}

public class UIController : MonoBehaviour {

    [Header("Player Control Setting Scene")]
    public bool SettingMode = true;
    public Image continueButtonImage;

    private bool canContinue = false;

    private int numPlayers = 1;
    private List<ControlType> controlSchemes;

    void Start() {

        if (SettingMode) {
            ResetControls();
            continueButtonImage.color = new Color(1f, 1f, 1f, 0.5f);
        }
    }

	public void GoTo(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void SetNumPlayers(int num) {
        numPlayers = num;
        ResetControls();
    }

    public void SetPlayer(PlayerControl toSet) {
        controlSchemes[toSet.playerNum - 1] = toSet.controlType;
        if (toSet.controlType == ControlType.Keyboard) {
            for (int i = 0; i < numPlayers; i++) {
                if (i != toSet.playerNum - 1) {
                    controlSchemes[i] = ControlType.Controller;
                }
            }
        }

        canContinue = true;
        continueButtonImage.color = Color.white;

        //PrintCurrentSettings();
    }

    public void FinalizeSettings(string nextScene) {
        if (canContinue) {
            SettingManager.NumberOfPlayers = numPlayers;
            SettingManager.ControlSchemes = controlSchemes;
            GoTo(nextScene);
        }
    }

    void ResetControls() {
        controlSchemes = new List<ControlType>();
        for (int i = 0; i < numPlayers; i++) {
            controlSchemes.Add(ControlType.Controller);
        }
    }

    void PrintCurrentSettings() {
        for (int i = 0; i < numPlayers; i++) {
            Debug.Log(string.Format("Player:{0}, Setting={1}", (i + 1).ToString(), controlSchemes[i]));
        }
    }
}
