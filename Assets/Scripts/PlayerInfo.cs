using Fusion;

public class PlayerInfo : NetworkBehaviour
{
    public PlayerRef pid;

    [Networked(OnChanged = nameof(slotChange))] public int roomNum { get; set; }
    public static void slotChange(Changed<PlayerInfo> changed) => changed.Behaviour.slotChange();
    void slotChange() => RoomPVP.instance.playerJoin();


    [Networked(OnChanged = nameof(readyCount))] public bool ready { get; set; }
    public static void readyCount(Changed<PlayerInfo> changed) => changed.Behaviour.readyCount();
    void readyCount() => RoomPVP.instance.ready();

    [Networked] public string id { get; set; }

    [Networked(OnChanged = nameof(itemChange))] public string car { get; set; }
    [Networked] public string gun { get; set; }
    public static void itemChange(Changed<PlayerInfo> changed) => changed.Behaviour.itemChange();
    void itemChange() => RoomPVP.instance.playerJoin();

    [Networked] public string team { get; set; }

    // In Game
    [Networked] public int MyLapMinute { get; set; }
    [Networked] public int MyLapSecond { get; set; }
    [Networked] public float MyLapMilli { get; set; }

    [Networked] public int MyChackPointCount { get; set; }
    [Networked(OnChanged = nameof(raceEnd))] public bool RaceGoal { get; set; }
    public static void raceEnd(Changed<PlayerInfo> changed) => changed.Behaviour.raceEnd();
    void raceEnd() => EndCountManager.instance.raceEnd();
}
