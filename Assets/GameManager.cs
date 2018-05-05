using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {

    private List<GameObject> players;
    //public GameObject player1;
    //public GameObject player2;

    //public string str;
    //public float winnerY;

    private List<int> scores;
    //public int p1Score;
    //public int p2Score;

    public List<TextMeshProUGUI> texts;
    //public Text p1Text;
    //public Text p2Text;

    public List<Slider> sliders;
    //public Slider p1Slider;
    //public Slider p2Slider;

    public GameObject playerPrefab;

    private int curHighestPlayerIndex = 0;
    private LineRenderer highestPlayerLine;


    // Use this for initialization
    void Start() {
        //StartCoroutine (Stages ());
        highestPlayerLine = GetComponent<LineRenderer>();

        players = new List<GameObject>();
        scores = new List<int>();

        if (SettingManager.HasBeenSetUp) {
            LoadPlayersFromSettings();
        } else {
            LoadPlayersFromScene();
        }
    }

    void LoadPlayersFromScene() {
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");

        SettingManager.NumberOfPlayers = foundPlayers.Length;

        LoadPlayersFromSettings();
    }

    void LoadPlayersFromSettings() {
        // Get all currently placed players in the scene
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");

        // Set the player IDs
        for (int i = 0; i < foundPlayers.Length; i++) {
            PlayerMovement tempMovement = foundPlayers[i].GetComponent<PlayerMovement>();
            if (tempMovement == null) {
                Destroy(foundPlayers[i]);
                players.Add(Instantiate(playerPrefab));
                players[players.Count - 1].GetComponent<PlayerMovement>().playerID = i - 1;
            } else {
                foundPlayers[i].GetComponent<PlayerMovement>().playerID = i + 1;
                players.Add(foundPlayers[i]);
            }
        }

        if (foundPlayers.Length < SettingManager.NumberOfPlayers) {
            // create the rest if there are not enough
            for (int i = foundPlayers.Length; i < SettingManager.NumberOfPlayers; i++) {
                players.Add(Instantiate(playerPrefab));
                players[players.Count - 1].GetComponent<PlayerMovement>().playerID = i - 1;
            }
        } else {
            // destroy the remaining if there are extra
            for (int i = SettingManager.NumberOfPlayers; i < foundPlayers.Length; i++) {
                Destroy(foundPlayers[i]);
            }

            for (int i = players.Count; i < 4; i++) {
                texts[i].transform.parent.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < players.Count; i++) {
            players[i].name = "Player" + (i + 1);
            scores.Add(0);
        }

        if (SettingManager.DebugMode) {
            for (int i = 1; i < players.Count; i++) {
                players[i].GetComponent<PlayerMovement>().enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update() {

        float maxY = float.MinValue;
        int highestPlayerIndex = -1;
        for (int i = 0; i < players.Count; i++) {
            if (players[i].transform.position.y > maxY) {
                maxY = players[i].transform.position.y;
                highestPlayerIndex = i;
            }
        }

        if (curHighestPlayerIndex != highestPlayerIndex) {
            sliders[highestPlayerIndex].value = 0;
            curHighestPlayerIndex = highestPlayerIndex;
        }

        sliders[highestPlayerIndex].value++;
        if (sliders[highestPlayerIndex].value >= sliders[highestPlayerIndex].maxValue) {
            scores[highestPlayerIndex]++;
            sliders[highestPlayerIndex].value = 0;
            texts[highestPlayerIndex].text = string.Format("P{0}: {1}", highestPlayerIndex + 1, scores[highestPlayerIndex]);
        }

        highestPlayerLine.SetPosition(0, new Vector3(-50, players[highestPlayerIndex].transform.position.y, -1f));
        highestPlayerLine.SetPosition(1, new Vector3(50, players[highestPlayerIndex].transform.position.y, -1f));
    }
}
