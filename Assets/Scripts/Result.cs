using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using WebSocketSharp;
using Fusion;

public class Result : NetworkBehaviour
{
    public NetworkRunner runner;

    private void Awake()
    {
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
    }

    Color redTeam = new Color(255 / 255f, 165 / 255f, 165 / 255f);
    Color blueTeam = new Color(165 / 255f, 165 / 255f, 255 / 255f);
    Color rank1 = new Color(255 / 255f, 215 / 255f, 0 / 255f);
    Color rank2 = new Color(192 / 255f, 192 / 255f, 192 / 255f);
    Color rank3 = new Color(164 / 255f, 124 / 255f, 109 / 255f);
    Color otherRank = new Color(193 / 255f, 193 / 255f, 193 / 255f);

    [Header("Chat UI Component")]
    public InputField ChatInput;

    [Header("Room Info Component")]
    public Text roomTitle;
    public Text roomAdmin;
    public GameObject RoomInfoObj;
    int nowPlayer;
    public GameObject infoBox;
    GameObject myInfo;

    [Header("User Info Component")]
    public Text carText;
    public Text gunText;

    [Header("Result Component")]
    public GameObject resultContent;
    public GameObject resultInstance;

    [Header("Result MyData")]
    int myRank;
    string myCar;
    string myGun;
    string myTime;
    int myMoney;

    bool TeamMod;

    int initCount;

    void Start()
    {
        if (ENB.cpuPlay != true) initCount = 1;
        else
        {
            ENB.cpuPlay = false;
        }
    }
    void Update()
    {
        if (ENB.cpuPlay != true) PvpUpdate();
        else CpuUpdate();
    }
    void OnChatEnter(InputValue value)
    {
        if (ChatInput.isFocused == false) ChatInput.Select();
    }
    void PvpUpdate()
    {
        if (RoomInfoObj == null)
        {
            RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");
            roomTitle.text = runner.SessionInfo.Properties["roomName"].PropertyValue.ToString();
            roomAdmin.text = "방장 : " + RoomInfoObj.GetComponent<RoomInfo>().roomAdmin;
            nowPlayer = RoomInfoObj.GetComponent<RoomInfo>().playerCount;
        }
        if (myInfo == null)
        {
            infoBox = GameObject.FindGameObjectWithTag("InfoBox");
            Transform[] infoList = infoBox.GetComponentsInChildren<Transform>();
            for (int i = 1; i < infoList.Length; i++)
            {
                if (infoList[i].GetComponent<PlayerInfo>().id.Equals(ENB.id))
                {
                    myInfo = infoList[i].gameObject;
                    carText.text = myInfo.GetComponent<PlayerInfo>().car;
                    gunText.text = myInfo.GetComponent<PlayerInfo>().gun;
                }
            }

            if (myInfo.GetComponent<PlayerInfo>().team.Equals("i")) TeamMod = false;
            else TeamMod = true;
        }
        if (RoomInfoObj != null && myInfo != null && initCount == 1)
        {
            PrintResult();
            initCount = 0;
        }
    }
    void CpuUpdate()
    {
        myInfo = GameDirector.instance.myInfoObj;
        if (myInfo != null && initCount == 1)
        {
            // CPU 형태 결과 출력
            initCount = 0;
        }
    }

    void PrintResult()
    {
        initCount = 0;

        int noRetire = -1;
        bool flagRetire = false;
        List<string> playerNames = new List<string>();

        for (int i = 0; i < nowPlayer; i++)
        {
            GameObject pInfo = null;
            string tmpId = null;
            switch (i)
            {
                case 0: tmpId = RoomInfoObj.GetComponent<RoomInfo>().Top1; break;
                case 1: tmpId = RoomInfoObj.GetComponent<RoomInfo>().Top2; break;
                case 2: tmpId = RoomInfoObj.GetComponent<RoomInfo>().Top3; break;
                case 3: tmpId = RoomInfoObj.GetComponent<RoomInfo>().Top4; break;
                case 4: tmpId = RoomInfoObj.GetComponent<RoomInfo>().Top5; break;
                case 5: tmpId = RoomInfoObj.GetComponent<RoomInfo>().Top6; break;
                case 6: tmpId = RoomInfoObj.GetComponent<RoomInfo>().Top7; break;
                case 7: tmpId = RoomInfoObj.GetComponent<RoomInfo>().Top8; break;
            }

            if (!tmpId.Equals(""))
            {
                GameObject tmp = GameObject.Instantiate(resultInstance, resultContent.transform);
                tmp.transform.parent = resultContent.transform;

                playerNames.Add(tmpId);
                pInfo = playerInfoId(tmpId);

                if (TeamMod)
                {
                    if (pInfo.GetComponent<PlayerInfo>().team.Equals("r")) tmp.GetComponent<Image>().color = redTeam;
                    else tmp.GetComponent<Image>().color = blueTeam;
                }
                else
                {
                    if (i == 0) tmp.GetComponent<Image>().color = rank1;
                    else if (i == 1) tmp.GetComponent<Image>().color = rank2;
                    else if (i == 2) tmp.GetComponent<Image>().color = rank3;
                    else if (i >= 3) tmp.GetComponent<Image>().color = otherRank;
                }

                tmp.transform.GetChild(0).GetComponent<Text>().text = (i + 1).ToString();
                tmp.transform.GetChild(1).GetComponent<Text>().text = pInfo.GetComponent<PlayerInfo>().id;
                tmp.transform.GetChild(2).GetComponent<Text>().text = pInfo.GetComponent<PlayerInfo>().car;
                tmp.transform.GetChild(3).GetComponent<Text>().text = pInfo.GetComponent<PlayerInfo>().gun;
                string tmpTime = pInfo.GetComponent<PlayerInfo>().MyLapMinute + ":" + pInfo.GetComponent<PlayerInfo>().MyLapSecond + ":" + pInfo.GetComponent<PlayerInfo>().MyLapMilli;
                tmp.transform.GetChild(4).GetComponent<Text>().text = tmpTime;
                int tmpMoney = (Mathf.Abs((100 * (nowPlayer * 10)) / (i + 1)));
                tmp.transform.GetChild(5).GetComponent<Text>().text = tmpMoney.ToString();

                if (pInfo.GetComponent<PlayerInfo>().id.Equals(ENB.id))
                {
                    myCar = pInfo.GetComponent<PlayerInfo>().car;
                    myGun = pInfo.GetComponent<PlayerInfo>().gun;
                    myRank = i + 1;
                    myTime = tmpTime;
                    myMoney = tmpMoney;
                }
            }
            else
            {
                if (!flagRetire)
                {
                    noRetire = i;
                    flagRetire = true;
                    break;
                }
            }
        }

        if (flagRetire)
        {
            Transform[] infoList = infoBox.GetComponentsInChildren<Transform>();

            for (int i = 1; i < infoList.Length; i++)
            {
                bool noDupli = false;
                for (int j = 0; j < noRetire; j++)
                {
                    if (playerNames[j].Equals(infoList[i].GetComponent<PlayerInfo>().id))
                    {
                        noDupli = true;
                        break;
                    }
                }

                if (!noDupli)
                {
                    GameObject tmp = GameObject.Instantiate(resultInstance, resultContent.transform);
                    tmp.transform.parent = resultContent.transform;

                    if (TeamMod)
                    {
                        if (infoList[i].GetComponent<PlayerInfo>().team.Equals("r")) tmp.GetComponent<Image>().color = redTeam;
                        else tmp.GetComponent<Image>().color = blueTeam;
                    }
                    else tmp.GetComponent<Image>().color = otherRank;

                    tmp.transform.GetChild(0).GetComponent<Text>().text = "Retire";
                    tmp.transform.GetChild(1).GetComponent<Text>().text = infoList[i].GetComponent<PlayerInfo>().id;
                    tmp.transform.GetChild(2).GetComponent<Text>().text = infoList[i].GetComponent<PlayerInfo>().car;
                    tmp.transform.GetChild(3).GetComponent<Text>().text = infoList[i].GetComponent<PlayerInfo>().gun;
                    string tmpTime = "Retire";
                    tmp.transform.GetChild(4).GetComponent<Text>().text = tmpTime;
                    int tmpMoney = (Mathf.Abs((100 * (nowPlayer * 10)) / (nowPlayer + noRetire)));
                    tmp.transform.GetChild(5).GetComponent<Text>().text = tmpMoney.ToString();

                    if (infoList[i].GetComponent<PlayerInfo>().id.Equals(ENB.id))
                    {
                        myCar = infoList[i].GetComponent<PlayerInfo>().car;
                        myGun = infoList[i].GetComponent<PlayerInfo>().gun;
                        myRank = -1;
                        myTime = tmpTime;
                        myMoney = tmpMoney;
                    }
                }
            }
        }

        initCount = 0;
        StartCoroutine(InsertResult());
    }
    GameObject playerInfoId(string id)
    {
        GameObject obj = null;
        Transform[] infoList = infoBox.GetComponentsInChildren<Transform>();

        for (int i = 1; i < infoList.Length; i++)
            if (infoList[i].GetComponent<PlayerInfo>().id.Equals(id))
                obj = infoList[i].gameObject;

        return obj;
    }

    public void ReturnRoom()
    {
        EndPlayInit();

        ENB.resultRoom = true;

        SceneManager.LoadScene("RoomPVP");
    }

    IEnumerator InsertResult()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.userinfo.id);
        if (myRank >= 1) form.AddField("result", myRank);
        else form.AddField("result", "Retire");
        form.AddField("resultCar", myCar);
        form.AddField("resultGun", myGun);

        ENB.userinfo.money += myMoney;
        form.AddField("money", ENB.userinfo.money);

        var www = UnityWebRequest.Post(GameManager.serverURL + "/resultInsert", form);
        yield return www.SendWebRequest();
    }
    void EndPlayInit()
    {
        if (runner.IsSharedModeMasterClient)
        {
            RoomInfoObj.GetComponent<RoomInfo>().WorldMinute = 0;
            RoomInfoObj.GetComponent<RoomInfo>().WorldSecond = 0;
            RoomInfoObj.GetComponent<RoomInfo>().WorldMilli = 0;

            RoomInfoObj.GetComponent<RoomInfo>().EndRace = false;
            RoomInfoObj.GetComponent<RoomInfo>().EndTime = 10;

            RoomInfoObj.GetComponent<RoomInfo>().Top1 = "";
            RoomInfoObj.GetComponent<RoomInfo>().Top2 = "";
            RoomInfoObj.GetComponent<RoomInfo>().Top3 = "";
            RoomInfoObj.GetComponent<RoomInfo>().Top4 = "";
            RoomInfoObj.GetComponent<RoomInfo>().Top5 = "";
            RoomInfoObj.GetComponent<RoomInfo>().Top6 = "";
            RoomInfoObj.GetComponent<RoomInfo>().Top7 = "";
            RoomInfoObj.GetComponent<RoomInfo>().Top8 = "";
        }
        myInfo.GetComponent<PlayerInfo>().MyLapMinute = 0;
        myInfo.GetComponent<PlayerInfo>().MyLapSecond = 0;
        myInfo.GetComponent<PlayerInfo>().MyLapMilli = 0;
        myInfo.GetComponent<PlayerInfo>().MyChackPointCount = 0;
        myInfo.GetComponent<PlayerInfo>().RaceGoal = false;
    }
}
