using UnityEngine;

public class MyRoomInfo : MonoBehaviour
{
    public static MyRoomInfo Instance = null;

    public string roomAdmin { get; set; }
    private void Awake() => Instance = this;

    public int playerCount { get; set; }

    public int map { get; set; }
    public int mapTime { get; set; }

    public bool s0 { get; set; }
    public bool s1 { get; set; }
    public bool s2 { get; set; }
    public bool s3 { get; set; }
    public bool s4 { get; set; }
    public bool s5 { get; set; }
    public bool s6 { get; set; }
    public bool s7 { get; set; }

    // In Game
    public int WorldMinute { get; set; }
    public int WorldSecond { get; set; }
    public float WorldMilli { get; set; }
    public bool EndRace { get; set; }
    public float EndTime { get; set; }

    // Top Rank Id
    public string Top1 { get; set; }
    public string Top2 { get; set; }
    public string Top3 { get; set; }
    public string Top4 { get; set; }
    public string Top5 { get; set; }
    public string Top6 { get; set; }
    public string Top7 { get; set; }
    public string Top8 { get; set; }
}
