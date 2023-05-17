using System.Collections.Generic;
using UnityEngine;
using Fusion;

public static class ENB
{
    public static string id;
    public static int loginFlag;

    public static bool gamePlay;

    public static DBClass.userinfo userinfo;
    public static DBClass.gameinfo gameinfo;
    public static List<DBClass.usecar> usecars = new List<DBClass.usecar>();
    public static List<DBClass.usegun> useguns = new List<DBClass.usegun>();
    public static List<DBClass.inviinfo> inviinfos = new List<DBClass.inviinfo>();
    public static List<DBClass.gameinfo> friends = new List<DBClass.gameinfo>();

    public static List<DBClass.history> history = new List<DBClass.history>();

    public static List<DBClass.gameinfo> userlist = new List<DBClass.gameinfo>();

    public static List<DBClass.track> tracks = new List<DBClass.track>();
    public static List<DBClass.carinfo> carinfos = new List<DBClass.carinfo>();
    public static List<DBClass.guninfo> guninfos = new List<DBClass.guninfo>();
    public static List<DBClass.dbEnb> dbEnbs = new List<DBClass.dbEnb> ();

    public static List<SessionInfo> myList = new List<SessionInfo>();

    public static string nowSceneName;

    public static int userListUpdateFlag;
    public static int invitation;
    public static int inviState;

    // ENB
    public static float maxAmmo;
    public static float maxAtk;
    public static float maxDef;
    public static float maxHp;
    public static float maxRpm;
    public static float maxSpeed;
    public static float itemAmmo;
    public static float itemBerserker;
    public static float itemBooster;
    public static float itemHpRestore;
    public static float itemShield;

    // Room Lobby
    public static PlayerRef pid;
    public static NetworkObject myNO; // Room Player PID
    public static bool roomGetOut;

    // Room Ingame
    public static int myRoomNum;
    public static int rank;

    // Result Room Move
    public static bool resultRoom;

    // CPU Mode
    public static bool cpuPlay;
}