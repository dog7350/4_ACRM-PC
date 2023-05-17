using Fusion;

public class RoomInfo : NetworkBehaviour
{
    [Networked] public string roomAdmin { get; set; }
    [Networked] public bool roomInfoChange { get; set; }
    [Networked] public bool ingameExitAdmin { get; set; }
    [Networked] public string getOut { get; set; }
    [Networked] public int playerCount { get; set; }

    [Networked] public bool mapChange { get; set; }
    [Networked] public int map { get; set; }
    [Networked] public int mapTime { get; set; }

    [Networked(OnChanged = nameof(slotOC))] public bool s0 { get; set; }
    [Networked(OnChanged = nameof(slotOC))] public bool s1 { get; set; }
    [Networked(OnChanged = nameof(slotOC))] public bool s2 { get; set; }
    [Networked(OnChanged = nameof(slotOC))] public bool s3 { get; set; }
    [Networked(OnChanged = nameof(slotOC))] public bool s4 { get; set; }
    [Networked(OnChanged = nameof(slotOC))] public bool s5 { get; set; }
    [Networked(OnChanged = nameof(slotOC))] public bool s6 { get; set; }
    [Networked(OnChanged = nameof(slotOC))] public bool s7 { get; set; }
    public static void slotOC(Changed<RoomInfo> changed) => changed.Behaviour.slotOC();
    void slotOC() => RoomPVP.instance.slotOC();

    // In Game
    [Networked] public int WorldMinute { get; set; }
    [Networked] public int WorldSecond { get; set; }
    [Networked] public float WorldMilli { get; set; }
    [Networked] public bool EndRace { get; set; }
    [Networked] public float EndTime { get; set; }

    // Top Rank Id
    [Networked] public string Top1 { get; set; }
    [Networked] public string Top2 { get; set; }
    [Networked] public string Top3 { get; set; }
    [Networked] public string Top4 { get; set; }
    [Networked] public string Top5 { get; set; }
    [Networked] public string Top6 { get; set; }
    [Networked] public string Top7 { get; set; }
    [Networked] public string Top8 { get; set; }

    public string CpuCar;
    public string CpuGun;
    public float CpuHp;
    public float CpuDef;
    public float CpuSpeed;
    public float CpuAtk;
    public float CpuAmmo;
    public float CpuRpm;
}
