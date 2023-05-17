using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Fusion;

public class EndCountManager : NetworkBehaviour
{
    public static EndCountManager instance = null;

    public NetworkRunner runner;
    public GameObject RoomInfoObj;
    public GameObject myInfo;

    public AudioClip music;

    private void Awake() => instance = this;

    public GameObject infoBox;
    int rankIndex = 1;

    public int check;
    public bool isPlay = false;
    public bool GameStart = false;
    public int PlayCount = 0;
    public GameObject LapCompleteTrigger;
    public GameObject LastCount;
    public GameObject LapTime;

    void Update()
    {
        if (ENB.cpuPlay != true) PvpUpdate();
        else CpuUpdate();
    }
    void PvpUpdate()
    {
        if (runner == null) runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
        if (myInfo == null) myInfo = GameDirector.instance.myInfoObj;
        if (infoBox == null) infoBox = GameObject.FindGameObjectWithTag("InfoBox");
        if (RoomInfoObj == null)
        {
            RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");
            if (runner.IsSharedModeMasterClient) RoomInfoObj.GetComponent<RoomInfo>().EndTime = 10.0f;
        }
        if (RoomInfoObj.GetComponent<RoomInfo>().EndRace)
        {
            isPlay = true;
            if (runner.IsSharedModeMasterClient)
            {
                if (RoomInfoObj.GetComponent<RoomInfo>().EndTime > 0)
                {
                    RoomInfoObj.GetComponent<RoomInfo>().EndTime -= Time.deltaTime;
                }
            }
            if (RoomInfoObj.GetComponent<RoomInfo>().EndTime <= 0)
            {
                ENB.gamePlay = false;
                LastCount.GetComponent<AudioSource>().Stop();
                myInfo.GetComponent<PlayerInfo>().RaceGoal = true;
                LapTime.SetActive(false);
                SceneManager.LoadScene("Result");
            }
            if (isPlay == true && PlayCount == 0)
            {
                LastCount.GetComponent<AudioSource>().Play();
                PlayCount++;
            }
            LastCount.GetComponent<Text>().text = Mathf.Round(RoomInfoObj.GetComponent<RoomInfo>().EndTime).ToString();
        }
    }

    void CpuUpdate()
    {
        if (runner == null) runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
        if (myInfo == null) myInfo = GameDirector.instance.myInfoObj;
        if (infoBox == null) infoBox = GameObject.FindGameObjectWithTag("InfoBox");

        if (RoomInfoObj == null)
        {
            RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");
            RoomInfoObj.GetComponent<RoomInfo>().EndTime = 10.0f;
        }
        if (RoomInfoObj.GetComponent<RoomInfo>().EndRace)
        {
            isPlay = true;
            if (RoomInfoObj.GetComponent<RoomInfo>().EndTime > 0)
            {
                RoomInfoObj.GetComponent<RoomInfo>().EndTime -= Time.deltaTime;
            }
            if (RoomInfoObj.GetComponent<RoomInfo>().EndTime <= 0)
            {
                ENB.gamePlay = false;
                LastCount.GetComponent<AudioSource>().Stop();
                myInfo.GetComponent<PlayerInfo>().RaceGoal = true;
                LapTime.SetActive(false);
                SceneManager.LoadScene("Result");
            }
            if (isPlay == true && PlayCount == 0)
            {
                LastCount.GetComponent<AudioSource>().Play();
                PlayCount++;
            }
            LastCount.GetComponent<Text>().text = Mathf.Round(RoomInfoObj.GetComponent<RoomInfo>().EndTime).ToString();
        }
    }

    // Lap을 늘리려면 RaceGoal의 변조 필요
    public void raceEnd()
    {
        if (ENB.cpuPlay != true)
        {
            if (runner.IsSharedModeMasterClient) Invoke("endCheck", 0.3f);
        }
        else Invoke("endCheck", 0.3f);
    }
    void endCheck()
    {
        Transform[] infoList = infoBox.GetComponentsInChildren<Transform>();
        for (int i = 1; i < infoList.Length; i++)
        {
            if (infoList[i].GetComponent<PlayerInfo>().RaceGoal)
            {
                RoomInfoObj.GetComponent<RoomInfo>().EndRace = true;
                RankID(infoList[i].GetComponent<PlayerInfo>().id);
            }
        }
    }
    void RankID(string id)
    {
        int pNum = RoomInfoObj.GetComponent<RoomInfo>().playerCount;
        bool pFlag = false;
        for (int i = 0; i < pNum; i++)
        {
            switch (i)
            {
                case 0: if (RoomInfoObj.GetComponent<RoomInfo>().Top1.Equals(id)) pFlag = true; break;
                case 1: if (RoomInfoObj.GetComponent<RoomInfo>().Top2.Equals(id)) pFlag = true; break;
                case 2: if (RoomInfoObj.GetComponent<RoomInfo>().Top3.Equals(id)) pFlag = true; break;
                case 3: if (RoomInfoObj.GetComponent<RoomInfo>().Top4.Equals(id)) pFlag = true; break;
                case 4: if (RoomInfoObj.GetComponent<RoomInfo>().Top5.Equals(id)) pFlag = true; break;
                case 5: if (RoomInfoObj.GetComponent<RoomInfo>().Top6.Equals(id)) pFlag = true; break;
                case 6: if (RoomInfoObj.GetComponent<RoomInfo>().Top7.Equals(id)) pFlag = true; break;
                case 7: if (RoomInfoObj.GetComponent<RoomInfo>().Top8.Equals(id)) pFlag = true; break;
            }
        }

        if (pFlag == false)
        {
            switch (rankIndex)
            {
                case 1: RoomInfoObj.GetComponent<RoomInfo>().Top1 = id; break;
                case 2: RoomInfoObj.GetComponent<RoomInfo>().Top2 = id; break;
                case 3: RoomInfoObj.GetComponent<RoomInfo>().Top3 = id; break;
                case 4: RoomInfoObj.GetComponent<RoomInfo>().Top4 = id; break;
                case 5: RoomInfoObj.GetComponent<RoomInfo>().Top5 = id; break;
                case 6: RoomInfoObj.GetComponent<RoomInfo>().Top6 = id; break;
                case 7: RoomInfoObj.GetComponent<RoomInfo>().Top7 = id; break;
                case 8: RoomInfoObj.GetComponent<RoomInfo>().Top8 = id; break;
            }
            rankIndex++;
        }
    }
}
