using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {

    [Header("Clips")]
    public AudioClip[] seagullClips;
    public AudioClip bigSplashClip;
    public AudioClip smallSplashClip;
    public AudioClip menuSelectMetalClip;
    public AudioClip[] playerHitClips;
    public AudioClip rainClip;
    public AudioClip[] thunderClips;
    public AudioClip wooshClip;
    public AudioClip hookHitClip;
    public AudioClip grappleClip;
    public AudioClip deathClip;
    public AudioClip landingClip;
    public AudioClip jumpClip;

    [HideInInspector] public AudioSource seagull;
    [HideInInspector] public AudioSource bigSplash;
    [HideInInspector] public AudioSource smallSplash;
    [HideInInspector] public AudioSource menuSelectMetal;
    [HideInInspector] public AudioSource playerHit;
    [HideInInspector] public AudioSource rain;
    [HideInInspector] public AudioSource thunder;
    [HideInInspector] public AudioSource woosh;
    [HideInInspector] public AudioSource hookHit;
    [HideInInspector] public AudioSource grapple;
    [HideInInspector] public AudioSource death;
    [HideInInspector] public AudioSource landing;
    [HideInInspector] public AudioSource jump;

    void Awake() {
        seagull = AddAudio(seagullClips[0], false, false, 0.65f);
        bigSplash = AddAudio(bigSplashClip, false, false, 0.7f);
        smallSplash = AddAudio(smallSplashClip, false, false, 0.7f);
        menuSelectMetal = AddAudio(menuSelectMetalClip, false, false, 0.7f);
        playerHit = AddAudio(playerHitClips[0], false, false, 0.7f);
        rain = AddAudio(rainClip, false, false, 0.5f);
        thunder = AddAudio(thunderClips[0], false, false, 0.6f);
        woosh = AddAudio(wooshClip, false, false, 0.7f);
        hookHit = AddAudio(hookHitClip, false, false, 0.7f);
        grapple = AddAudio(grappleClip, false, false, 0.7f);
        death = AddAudio(deathClip, false, false, 0.7f);
        landing = AddAudio(landingClip, false, false, 0.7f);
        jump = AddAudio(jumpClip, false, false, 0.7f);
    }

    AudioSource AddAudio(AudioClip clip, bool loop, bool playAwake, float vol) {
        AudioSource newAudio = gameObject.AddComponent<AudioSource>();
        newAudio.clip = clip;
        newAudio.loop = loop;
        newAudio.playOnAwake = playAwake;
        newAudio.volume = vol;
        return newAudio;
     }

    public void Play(AudioSource toPlay, bool randomPitch = false) {
        if (toPlay == seagull) {
            toPlay.clip = seagullClips[Random.Range(0, seagullClips.Length)];

        } else if (toPlay == playerHit) {
            toPlay.clip = playerHitClips[Random.Range(0, playerHitClips.Length)];

        } else if (toPlay == thunder) {
            toPlay.clip = thunderClips[Random.Range(0, thunderClips.Length)];
        }

        if (randomPitch) {
            toPlay.pitch = Random.Range(0.8f, 1.2f);
        }
        toPlay.Play();
        toPlay.pitch = 1f;
    }

    public float PlayBigUIClick() {
        Play(menuSelectMetal);
        return menuSelectMetalClip.length;
    }

    public float PlayerSmallUIClick() {
        Play(smallSplash, true);
        return smallSplashClip.length;
    }

    public void QuickPlayBigUIClick() {
        Play(menuSelectMetal);
    }

    public void QuickPlayerSmallUIClick() {
        Play(smallSplash, true);
    }
}
