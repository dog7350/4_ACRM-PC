using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using WebSocketSharp;
using Fusion;

public class CpuRoomInfo : MonoBehaviour
{
    public static CpuRoomInfo instance = null;
    public NetworkRunner runner;

    public AudioClip music;
    private void Awake()
    {
        instance = this;
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
    }

    public string CpuCar;
    public string CpuGun;
    public float CpuHp;
    public float CpuDef;
    public float CpuSpeed;
    public float CpuAtk;
    public float CpuAmmo;
    public float CpuRpm;

    public int WorldMinute;
    public int WorldSecond;
    public float WorldMilli;
    public bool EndRace;
    public float EndTime;

    public bool RaceGoal;

    public int rank;
}
