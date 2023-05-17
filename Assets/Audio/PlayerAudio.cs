using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerAudio : NetworkBehaviour
{
    public AudioSource StartSound;
	public AudioSource IdleSound;
	public AudioSource RunningSound;
	public AudioSource ReverseSound;
	public AudioSource Drift;
	public AudioSource Crash;
    [Range(0.1f, 1.0f)] public float RunningSoundMaxVolume = 1.0f;
    [Range(0.1f, 2.0f)] public float RunningSoundMaxPitch = 1.0f;
    [Range(0.1f, 1.0f)] public float ReverseSoundMaxVolume = 0.5f;
    [Range(0.1f, 2.0f)] public float ReverseSoundMaxPitch = 0.6f;
    [Range(0.1f, 1.0f)] public float IdleSoundMaxVolume = 0.6f;
    [Range(0.1f, 1.0f)] public float DriftMaxVolume = 0.5f;

    public AudioSource HpRestore;
    public AudioSource Berserker;
    public AudioSource Booster;
    public AudioSource AmmoRestore;
    public AudioSource Shield;

    public void HandleDriveAudio(float speed)
    {
        if (speed < 0.0f)
        {
            // In reverse
            RunningSound.volume = 0.0f;
            ReverseSound.volume = Mathf.Lerp(0.1f, ReverseSoundMaxVolume, -speed * 1.2f);
            ReverseSound.pitch = Mathf.Lerp(0.1f, ReverseSoundMaxPitch, -speed + (Mathf.Sin(Time.time) * .1f));
        }
        else
        {
            // Moving forward
            ReverseSound.volume = 0.0f;
            RunningSound.volume = Mathf.Lerp(0.1f, RunningSoundMaxVolume, speed * 1.2f);
            RunningSound.pitch = Mathf.Lerp(0.3f, RunningSoundMaxPitch, speed + (Mathf.Sin(Time.time) * .1f));
        }
    }
    public void HandleDriftAudio(float speed) => Drift.volume = Mathf.Lerp(Drift.volume, speed, Time.deltaTime * 20f);
    public void PlayCrash() => Crash.Play();

    public void PlayHpRestore() => HpRestore.Play();
    public void PlayBerserker() => Berserker.Play();
    public void PlayBooster() => Booster.Play();
    public void PlayAmmoRestore() => AmmoRestore.Play();
    public void PlayShield() => Shield.Play();
}
