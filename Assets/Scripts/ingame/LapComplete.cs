using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class LapComplete : NetworkBehaviour
{
    public NetworkRunner runner;
    public GameObject RoomInfoObj;
    public GameObject myInfo;

    public GameObject LabCompleteTrig;
    public GameObject HalfLabTrig;

    public GameObject LapTimeBox;
    public GameObject LapCounter;

    public GameObject EnterCollision;
    bool enterCheck = false;
    public string CollisionID;
    public int LapsDone;

    void Start() => LapsDone = 0;

    void Update()
    {
        if (runner == null) runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
        if (myInfo == null) myInfo = GameDirector.instance.myInfoObj;
        if (RoomInfoObj == null) RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");
    }

    void OnTriggerEnter(Collider col)
    {
        if (ENB.cpuPlay != true)
        {
            EnterCollision = col.gameObject;

            if (EnterCollision != null)
            {
                if (!EnterCollision.CompareTag("AICar")) CollisionID = EnterCollision.GetComponent<GamePlayerInfo>().createId;
                enterCheck = true;
                Invoke("GoalCheck", 0.3f);
            }
        }
        else
        {
            if (col.CompareTag("AICar")) RoomInfoObj.GetComponent<RoomInfo>().EndRace = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (ENB.cpuPlay != true)
        {
            EnterCollision = other.gameObject;

            if (EnterCollision != null)
            {
                if (!EnterCollision.CompareTag("AICar")) CollisionID = EnterCollision.GetComponent<GamePlayerInfo>().createId;
                enterCheck = true;
                Invoke("GoalCheck", 0.3f);
            }
        }
        else
        {
            if (other.CompareTag("AICar")) RoomInfoObj.GetComponent<RoomInfo>().EndRace = true;
        }
    }

    void GoalCheck()
    {
        if (!EnterCollision.CompareTag("AICar")) CollisionID = EnterCollision.GetComponent<GamePlayerInfo>().createId;

        if (CollisionID.Equals(ENB.id)) // ������ ����� �����
        {
            myInfo.GetComponent<PlayerInfo>().RaceGoal = true; // ���� ������ ���� ǥ�� (Lap �ø����� ���� �ʿ�)
            LapsDone += 1;
            LapCounter.GetComponent<Text>().text = "" + LapsDone;
            HalfLabTrig.SetActive(true);
            LabCompleteTrig.SetActive(false);
        }

        CollisionID = null;
        EnterCollision = null;
    }
}


