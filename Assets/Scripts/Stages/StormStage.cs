using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormStage : Stage {

    private enum StormProgression { Initialize, FadeBackground, SpawningIcebergs };
    private StormProgression m_currentState;

    private float m_smallIcebergSpawnDelay;
    private float m_smallIcebergSpawnTimer;
    private Vector2 m_randomSpawnRange = new Vector2(2, 5);

    private SpriteRenderer[] m_clearSkySpriteRenderers;
    private SpriteRenderer[] m_stormySkySpriteRenderers;
    private float m_alphaLerpValue;
    private float m_alphaLerpInc;

    public StormStage(float stageLength, float stageTransitionSpeed = 0.05f) : base(stageLength) {
        ResetSpawnTimer();
        m_alphaLerpValue = 0;
        m_alphaLerpInc = stageTransitionSpeed;
        m_currentState = StormProgression.Initialize;
    }

    protected override void GameplayLoop(float deltaTime) {
        switch (m_currentState) {
            case StormProgression.Initialize:
                StageManager.GetInstance().ac.Play(StageManager.GetInstance().ac.rain);

                StageManager.GetInstance().sky_clear.SetActive(true);
                m_clearSkySpriteRenderers = StageManager.GetInstance().sky_clear.GetComponentsInChildren<SpriteRenderer>();

                StageManager.GetInstance().sky_stormy.SetActive(true);
                m_stormySkySpriteRenderers = StageManager.GetInstance().sky_stormy.GetComponentsInChildren<SpriteRenderer>();

                foreach (SpriteRenderer s in m_clearSkySpriteRenderers) {
                    s.color = ToAlpha1(s.color);
                }
                foreach (SpriteRenderer s in m_stormySkySpriteRenderers) {
                    s.color = ToAlpha0(s.color);
                }

                m_currentState = StormProgression.FadeBackground;
                break;

            case StormProgression.FadeBackground:
                Debug.Log(m_alphaLerpValue);
                foreach (SpriteRenderer s in m_clearSkySpriteRenderers) {
                    s.color = Color.Lerp(ToAlpha1(s.color), ToAlpha0(s.color), m_alphaLerpValue);
                }
                foreach (SpriteRenderer s in m_stormySkySpriteRenderers) {
                    s.color = Color.Lerp(ToAlpha0(s.color), ToAlpha1(s.color), m_alphaLerpValue);
                }
                m_alphaLerpValue += m_alphaLerpInc;
                
                if (m_alphaLerpValue >= 1f) {
                    StageManager.GetInstance().sky_clear.SetActive(false);
                    StageManager.GetInstance().sky_stormy.SetActive(true);

                    m_currentState = StormProgression.SpawningIcebergs;
                    m_alphaLerpValue = 0;
                }
                break;

            case StormProgression.SpawningIcebergs:
                base.GameplayLoop(deltaTime);

                m_smallIcebergSpawnTimer += deltaTime;

                if (m_smallIcebergSpawnTimer > m_smallIcebergSpawnDelay) {
                    SpawnIceberg();
                    ResetSpawnTimer();
                }
                break;

            default:
                break;
        }
    }

    private void SpawnIceberg() {
        Instantiate(
            StageManager.GetInstance().iceberg_small, 
            StageManager.GetInstance().iceberg_origin.position + new Vector3(0, Random.Range(-2f, 2f), 0), 
            StageManager.GetInstance().iceberg_origin.rotation
        );
    }

    private void ResetSpawnTimer() {
        m_smallIcebergSpawnDelay = Random.Range(m_randomSpawnRange[0], m_randomSpawnRange[1]);
        m_smallIcebergSpawnTimer = 0;
    }

    Color ToAlpha0(Color color) {
        return new Color(color.r, color.g, color.b, 0);
    }

    Color ToAlpha1(Color color) {
        return new Color(color.r, color.g, color.b, 1);
    }
}
