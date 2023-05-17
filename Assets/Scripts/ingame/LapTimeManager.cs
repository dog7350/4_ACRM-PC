using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class LapTimeManager : NetworkBehaviour
{
    public NetworkRunner runner;
    public GameObject RoomInfoObj;
    public GameObject myInfo;

    public static string MilliDisplay;

    public GameObject MinuteBox;
    public GameObject SecondBox;
    public GameObject MilliBox;
    public GameObject LapTime;

    void Start()
    {
        LapTime.SetActive(true);
    }
    void Update()
    {
        if (ENB.cpuPlay != true) PvpUpdate();
        else CpuUpdate();
    }
    void PvpUpdate()
    {
        if (runner == null) runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
        if (myInfo == null) myInfo = GameDirector.instance.myInfoObj;

        if (RoomInfoObj == null) RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");
        else // 모든 필요 오브젝트가 들어왔다면
        {
            if (runner.IsSharedModeMasterClient) RoomInfoObj.GetComponent<RoomInfo>().WorldMilli += Time.deltaTime * 10;

            MilliDisplay = RoomInfoObj.GetComponent<RoomInfo>().WorldMilli.ToString("F0");
            MilliBox.GetComponent<Text>().text = "" + MilliDisplay;

            if (RoomInfoObj.GetComponent<RoomInfo>().WorldMilli >= 9)
            {
                if (runner.IsSharedModeMasterClient)
                {
                    RoomInfoObj.GetComponent<RoomInfo>().WorldMilli = 0;
                    RoomInfoObj.GetComponent<RoomInfo>().WorldSecond += 1;
                }
            }

            if (RoomInfoObj.GetComponent<RoomInfo>().WorldSecond <= 9)
            {
                SecondBox.GetComponent<Text>().text = "0" + RoomInfoObj.GetComponent<RoomInfo>().WorldSecond + ".";
            }
            else
            {
                SecondBox.GetComponent<Text>().text = "" + RoomInfoObj.GetComponent<RoomInfo>().WorldSecond + ".";
            }

            if (RoomInfoObj.GetComponent<RoomInfo>().WorldSecond >= 60)
            {
                if (runner.IsSharedModeMasterClient)
                {
                    RoomInfoObj.GetComponent<RoomInfo>().WorldSecond = 0;
                    RoomInfoObj.GetComponent<RoomInfo>().WorldMinute += 1;
                }
            }

            if (RoomInfoObj.GetComponent<RoomInfo>().WorldMinute <= 9)
            {
                MinuteBox.GetComponent<Text>().text = "0" + RoomInfoObj.GetComponent<RoomInfo>().WorldMinute + ":";
            }
            else
            {
                MinuteBox.GetComponent<Text>().text = "" + RoomInfoObj.GetComponent<RoomInfo>().WorldMinute + ":";
            }

            if (myInfo.GetComponent<PlayerInfo>().RaceGoal != true) // 내가 현재 골인한 것이 아니라면
            {
                myInfo.GetComponent<PlayerInfo>().MyLapMilli = RoomInfoObj.GetComponent<RoomInfo>().WorldMilli;
                myInfo.GetComponent<PlayerInfo>().MyLapSecond = RoomInfoObj.GetComponent<RoomInfo>().WorldSecond;
                myInfo.GetComponent<PlayerInfo>().MyLapMinute = RoomInfoObj.GetComponent<RoomInfo>().WorldMinute;
            }
        }
    }

    void CpuUpdate()
    {
        if (runner == null) runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
        if (myInfo == null) myInfo = GameDirector.instance.myInfoObj;

        if (RoomInfoObj == null) RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");
        else // 모든 필요 오브젝트가 들어왔다면
        {
            RoomInfoObj.GetComponent<RoomInfo>().WorldMilli += Time.deltaTime * 10;

            MilliDisplay = RoomInfoObj.GetComponent<RoomInfo>().WorldMilli.ToString("F0");
            MilliBox.GetComponent<Text>().text = "" + MilliDisplay;

            if (RoomInfoObj.GetComponent<RoomInfo>().WorldMilli >= 9)
            {
                RoomInfoObj.GetComponent<RoomInfo>().WorldMilli = 0;
                RoomInfoObj.GetComponent<RoomInfo>().WorldSecond += 1;
            }

            if (RoomInfoObj.GetComponent<RoomInfo>().WorldSecond <= 9)
            {
                SecondBox.GetComponent<Text>().text = "0" + RoomInfoObj.GetComponent<RoomInfo>().WorldSecond + ".";
            }
            else
            {
                SecondBox.GetComponent<Text>().text = "" + RoomInfoObj.GetComponent<RoomInfo>().WorldSecond + ".";
            }

            if (RoomInfoObj.GetComponent<RoomInfo>().WorldSecond >= 60)
            {
                RoomInfoObj.GetComponent<RoomInfo>().WorldSecond = 0;
                RoomInfoObj.GetComponent<RoomInfo>().WorldMinute += 1;
            }

            if (RoomInfoObj.GetComponent<RoomInfo>().WorldMinute <= 9)
            {
                MinuteBox.GetComponent<Text>().text = "0" + RoomInfoObj.GetComponent<RoomInfo>().WorldMinute + ":";
            }
            else
            {
                MinuteBox.GetComponent<Text>().text = "" + RoomInfoObj.GetComponent<RoomInfo>().WorldMinute + ":";
            }

            if (myInfo.GetComponent<PlayerInfo>().RaceGoal != true) // 내가 현재 골인한 것이 아니라면
            {
                myInfo.GetComponent<PlayerInfo>().MyLapMilli = RoomInfoObj.GetComponent<RoomInfo>().WorldMilli;
                myInfo.GetComponent<PlayerInfo>().MyLapSecond = RoomInfoObj.GetComponent<RoomInfo>().WorldSecond;
                myInfo.GetComponent<PlayerInfo>().MyLapMinute = RoomInfoObj.GetComponent<RoomInfo>().WorldMinute;
            }
        }
    }
}

