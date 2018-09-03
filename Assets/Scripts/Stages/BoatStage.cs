using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatStage : Stage {

    private float m_seagullSpawnDelay;
    private float m_seagullSpawnTimer;
    private Vector2 m_randomSpawnRange = new Vector2(5, 10);

    private bool m_initialized;

    public BoatStage(float stageLength) : base(stageLength) {
        ResetSpawnTimer();
        m_initialized = false;
    }

    protected override void GameplayLoop(float deltaTime) {
        if (!m_initialized) {
            StageManager.GetInstance().sky_clear.SetActive(true);
            StageManager.GetInstance().sky_stormy.SetActive(false);

            m_initialized = true;
        }

        base.GameplayLoop(deltaTime);

        m_seagullSpawnTimer += deltaTime;

        if (m_seagullSpawnTimer > m_seagullSpawnDelay) {
            SpawnSeagull();
            ResetSpawnTimer();
        }
    }

    private void SpawnSeagull() {
        Instantiate(
            StageManager.GetInstance().seagull, 
            StageManager.GetInstance().seagull_origin.position + new Vector3(0, Random.Range(0, 15f), 0), 
            StageManager.GetInstance().seagull_origin.rotation
        );
    }

    private void ResetSpawnTimer() {
        m_seagullSpawnDelay = Random.Range(m_randomSpawnRange[0], m_randomSpawnRange[1]);
        m_seagullSpawnTimer = 0;
    }
}
