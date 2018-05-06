using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class GameManager : MonoBehaviour {

    public List<GameObject> players;
    private List<int> scores;
    public List<TextMeshProUGUI> texts;
    public List<Slider> sliders;

    public GameObject playerPrefab;

    private int curHighestPlayerIndex = 0;
    private LineRenderer highestPlayerLine;

    [Header("Camera Following")]
    public CinemachineTargetGroup cameraTargetGroup;
    private List<CinemachineTargetGroup.Target> cameraTargets;
    public float playerTargetRadius = 4f;
    public float playerTargetWeight = 1f;
    public float hookTargetRadius = 2f;
    public float hookTargetWeight = 0.5f;

    [Header("Spawn Points")]
    public GameObject[] boatSpawnPoints;
    public GameObject[] stormSpawnPoints;
    public GameObject[] icebergSpawnPoints;

    [Header("Level Boundaries")]
    public GameObject[] boatBoundaries;
    public GameObject[] stormBoundaries;
    public GameObject[] icebergBoundaries;

    private StageManager stageManager;
    
    void Awake() {
        highestPlayerLine = GetComponent<LineRenderer>();
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();

        players = new List<GameObject>();
        scores = new List<int>();

        if (SettingManager.HasBeenSetUp) {
            LoadPlayersFromSettings();
        } else {
            LoadPlayersFromScene();
        }
        InitSpawnPlayers();

        InitCameraFollowing();

        //SetBoundaries();
    }

    public void SetBoundaries() {
        if (stageManager.state == StageManager.GameState.Boat) {
            SetActiveBoatBoundaries(true);
            SetActiveStormBoundaries(false);
            SetActiveIcebergBoundaries(false);

        } else if (stageManager.state == StageManager.GameState.Storm) {
            SetActiveBoatBoundaries(false);
            SetActiveStormBoundaries(true);
            SetActiveIcebergBoundaries(false);

        } else if (stageManager.state == StageManager.GameState.Iceberg) {
            SetActiveBoatBoundaries(false);
            SetActiveStormBoundaries(false);
            SetActiveIcebergBoundaries(true);

        } else {
            Debug.Log(string.Format("State with name \"{0}\" found...", stageManager.state.ToString()));
        }
    }

    void SetActiveBoatBoundaries(bool active) {
        for (int i = 0; i < boatBoundaries.Length; i++) {
            boatBoundaries[i].SetActive(active);
        }
    }

    void SetActiveStormBoundaries(bool active) {
        for (int i = 0; i < stormBoundaries.Length; i++) {
            stormBoundaries[i].SetActive(active);
        }
    }

    void SetActiveIcebergBoundaries(bool active) {
        for (int i = 0; i < icebergBoundaries.Length; i++) {
            icebergBoundaries[i].SetActive(active);
        }
    }

    void InitSpawnPlayers() {
        int randomStartIndex = Random.Range(0, boatSpawnPoints.Length);
        for (int i = 0; i < players.Count; i++) {
            players[i].transform.position = boatSpawnPoints[randomStartIndex].transform.position;
            randomStartIndex = (randomStartIndex + 1) % boatSpawnPoints.Length;
        }
    }

    public void Respawn(Transform playerToRespawn) {
        if (stageManager.state == StageManager.GameState.Boat) {
            playerToRespawn.position = boatSpawnPoints[Random.Range(0, boatSpawnPoints.Length)].transform.position;
        } else if (stageManager.state == StageManager.GameState.Storm) {
            playerToRespawn.position = stormSpawnPoints[Random.Range(0, stormSpawnPoints.Length)].transform.position;
        } else {
            playerToRespawn.position = icebergSpawnPoints[Random.Range(0, icebergSpawnPoints.Length)].transform.position;
        }
    }

    void InitCameraFollowing() {
        cameraTargets = new List<CinemachineTargetGroup.Target>();

        cameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[players.Count];
        for (int i = 0; i < players.Count; i++) {
            AddCameraPlayerTarget(players[i].transform);
        }
    }

    public void AddCameraPlayerTarget(Transform toAdd) {
        CinemachineTargetGroup.Target curTarget = new CinemachineTargetGroup.Target();
        curTarget.target = toAdd;
        curTarget.weight = playerTargetWeight;
        curTarget.radius = playerTargetRadius;
        cameraTargets.Add(curTarget);
        cameraTargetGroup.m_Targets = cameraTargets.ToArray();
    }

    public void AddCameraHookTarget(Transform toAdd) {
        CinemachineTargetGroup.Target curTarget = new CinemachineTargetGroup.Target();
        curTarget.target = toAdd;
        curTarget.weight = hookTargetWeight;
        curTarget.radius = hookTargetRadius;
        cameraTargets.Add(curTarget);
        cameraTargetGroup.m_Targets = cameraTargets.ToArray();
    }

    public void RemoveCameraTarget(Transform toRemove) {
        for (int i = 0; i < cameraTargets.Count; i++) {
            if (cameraTargets[i].target == toRemove) {
                cameraTargets.RemoveAt(i);
                cameraTargetGroup.m_Targets = cameraTargets.ToArray();
                return;
            }
        }
        Debug.Log(string.Format("Object with name \"{0}\" not found in camera targets.", toRemove.name));
    }

    void LoadPlayersFromScene() {
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");

        SettingManager.NumberOfPlayers = foundPlayers.Length;
        if (SettingManager.NumberOfPlayers >= 1) {

            SettingManager.ControlSchemes.Add(ControlType.Keyboard);

            for (int i = 1; i < SettingManager.NumberOfPlayers; i++) {
                SettingManager.ControlSchemes.Add(ControlType.Controller);
            }
        }
        LoadPlayersFromSettings();
    }

    void LoadPlayersFromSettings() {
        // Get all currently placed players in the scene
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < foundPlayers.Length; i++) {
            Destroy(foundPlayers[i]);
        }
        for (int i = 0; i < SettingManager.NumberOfPlayers; i++) {
            GameObject newPlayer = Instantiate(playerPrefab);
            newPlayer.GetComponent<PlayerMovement>().playerID = i + 1;
            players.Add(newPlayer);
        }

        /*
        // Set the player IDs
        for (int i = 0; i < foundPlayers.Length; i++) {
            PlayerMovement tempMovement = foundPlayers[i].GetComponent<PlayerMovement>();
            if (tempMovement == null) {
                Destroy(foundPlayers[i]);
                GameObject newPlayer = Instantiate(playerPrefab);
                newPlayer.GetComponent<PlayerMovement>().playerID = i + 1;
                players.Add(newPlayer);
            } else {
                foundPlayers[i].GetComponent<PlayerMovement>().playerID = i + 1;
                players.Add(foundPlayers[i]);
            }
        }

        if (foundPlayers.Length < SettingManager.NumberOfPlayers) {
            // create the rest if there are not enough
            for (int i = foundPlayers.Length; i < SettingManager.NumberOfPlayers; i++) {
                players.Add(Instantiate(playerPrefab));
                players[players.Count - 1].GetComponent<PlayerMovement>().playerID = i + 1;
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
        */

        for (int i = 0; i < players.Count; i++) {
            players[i].name = "Player" + (i + 1);
            scores.Add(0);
        }
        for (int i = players.Count; i < 4; i++) {
            texts[i].transform.parent.gameObject.SetActive(false);
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
