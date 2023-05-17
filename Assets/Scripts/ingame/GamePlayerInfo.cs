using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Fusion;

public class GamePlayerInfo : NetworkBehaviour
{
    public static GamePlayerInfo instance = null;
    public NetworkRunner runner;

    private void Awake()
    {
        instance = this;
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
    }

    [Networked] public string createId { get; set; }
    public int spNum;

    [Networked(OnChanged = nameof(ExplosionChange))] public bool Explosion { get; set; }
    public static void ExplosionChange(Changed<GamePlayerInfo> changed) => changed.Behaviour.ExplosionChange();
    void ExplosionChange() => GameDirector.instance.ExplosionEffect();

    [Networked(OnChanged = nameof(BoostChange))] public bool Boost { get; set; }
    public static void BoostChange(Changed<GamePlayerInfo> changed) => changed.Behaviour.BoostChange();
    void BoostChange() => GameDirector.instance.BoostEffect();
}
