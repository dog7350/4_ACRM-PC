using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using WebSocketSharp;
using Fusion;

public class RoomPVP : NetworkBehaviour
{
    public static RoomPVP instance = null;
    public NetworkRunner runner;

    public AudioClip music;

    private void Awake()
    {
        instance = this;
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
    }

    int initCount = 1;

    Color individual = new Color(193 / 255f, 193 / 255f, 193 / 255f);
    Color redTeam = new Color(255 / 255f, 165 / 255f, 165 / 255f);
    Color blueTeam = new Color(165 / 255f, 165 / 255f, 255 / 255f);

    float[,] itemPosition = new float[,]
    {
        { 110, -20, -130 }, { 110, 20, -130 }, { 40, -20, -130 }, { 40, 20, -130 }, { -40, -20, -130 }, { -40, 20, -130 }, { -110, -20, -130 }, { -110, 20, -130 }
    };
    float[,] itemRotation = new float[,]
    {
        { 20, -180, 0 }, { 3, -180, 0 }, { 17, 200, 3 }, { 3, 200, -3 }, { 15, 160, -6 }, { 1, 160, 0 }, { 18, 180, 0 }, { 1, 180, 0 }
    };
    float[,] itemScale = new float[,]
    {
        { 25, 25, 25 }, { 25, 28, 25 }, { 30, 26, 28 }, { 30, 27, 28 }, { 30, 26, 28 }, { 30, 27, 28 }, { 25, 25, 25 }, { 25, 28, 25 }
    };

    [Header("LodingPanel")]
    public GameObject LoadingPanel;
    public Sprite img1;
    public Sprite img2;

    [Header("Chat UI Component")]
    public InputField ChatInput;

    [Header("Alarm")]
    public GameObject AlarmPanel;
    public Text AlarmText;

    [Header("Room Info Component")]
    public Text roomTitle;
    public Text roomAdmin;
    public Text roomMode;
    public Text roomPlayer;
    public GameObject RoomInfoObj;
    int adminChange;
    int readyCount;
    int nowPlayer;
    bool readyFlag = false;

    [Header("Room Info Setting")]
    public GameObject roomSetPanel;
    public InputField roomSetName;
    public Dropdown roomSetMode;
    bool roomInfoFalg = false;
    List<int> slotoc = new List<int>();
    int maxPlayer = 8;
    bool resultRoomFlag = false;

    [Header("Room Map Setting")]
    public GameObject mapSetPanel;
    public GameObject mapListInstance;
    public GameObject mapScrollContent;
    public Text mapTitle;
    public Image mapImage;
    public Text mapContent;
    public Dropdown mapTime;
    public Image roomMap;
    public Text roomTime;
    GameObject prevMapBtn;
    int selectMap = 0;

    [Header("User Info Component")]
    public Text carText;
    public Text gunText;

    [Header("Player Panel")]
    public GameObject PlayerPanel;
    public GameObject infoBox;
    public GameObject myInfo;
    public Button[] Playerinfos;

    public int joinCount = 1; // Join
    Transform[] localPlayerList; // child

    [Header("History UI Component")]
    public Text HistId;
    public Text HistCar;
    public Text HistGun;
    public GameObject HistoryPanel;
    public GameObject HistoryContent;
    public GameObject HistoryList;

    [Header("Garage UI Component")]
    public GameObject cargunInstance;
    public GameObject GaragePanel;
    public Text carTitle;
    public Text carPrice;
    public Text carCnM;
    public Image HpImg;
    public Image DefImg;
    public Image SpdImg;
    public Text carContent;
    public GameObject carScrollContent;
    public Text gunTitle;
    public Text gunPrice;
    public Text gunCnM;
    public Image AtkImg;
    public Image AmmoImg;
    public Image RpmImg;
    public Text gunContent;
    public GameObject gunScrollContent;
    GameObject prevCarBtn = null;
    GameObject prevGunBtn = null;
    DBClass.carinfo car = null;
    DBClass.guninfo gun = null;

    [Header("Invitation UI Component")]
    public GameObject InviListInstance;
    public GameObject InviHistoryInstance;
    public GameObject InviStateInstance;
    public GameObject InvitationPanel;
    public Button lobbySelectBtn;
    public Button friendSelectBtn;
    public GameObject InviListContent;
    public GameObject InviHistoryContent;
    public GameObject InviStateContent;
    List<DBClass.inviuser> inviList = new List<DBClass.inviuser>();
    List<DBClass.inviinfo> inviState = new List<DBClass.inviinfo>();
    string inviusername;
    GameObject inviPrevBtn;

    void Start()
    {
        int number = Random.Range(1, 3);
        if (number == 1) LoadingPanel.GetComponent<Image>().sprite = img1;
        else LoadingPanel.GetComponent<Image>().sprite = img2;

        infoBox = GameObject.FindGameObjectWithTag("InfoBox");

        if (ENB.resultRoom)
        {
            joinCount = 0;
            resultRoomFlag = true;
            ENB.resultRoom = false;
        }

        ENB.nowSceneName = SceneManager.GetActiveScene().name;
        GameManager.instance.U_UserStatusUpdate();

        carText.text = ENB.gameinfo.usecar;
        gunText.text = ENB.gameinfo.usegun;
    }

    void Update()
    {
        if (initCount == 1 && runner.IsPlayer)
        {
            if (runner.SessionInfo.Name != "")
            {
                roomTitle.text = runner.SessionInfo.Properties["roomName"].PropertyValue.ToString();
                roomMode.text = runner.SessionInfo.Properties["mode"].PropertyValue.ToString() == "1" ? "개인전" : "팀전";
                roomPlayer.text = runner.SessionInfo.Properties["maxPlayer"].PropertyValue.ToString();
                LobbyChat.instance.RoomChatText.text += "[" + roomTitle.text + "] 방에 입장하셨습니다.";
                modeSlot(roomMode.text);
                initCount = 0;
            }
            LoadingPanel.SetActive(false);
        }
        
        if (infoBox == null) infoBox = GameObject.FindGameObjectWithTag("InfoBox");

        if (RoomInfoObj == null)
        {
            RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");
            roomAdmin.text = "방장 : " + RoomInfoObj.GetComponent<RoomInfo>().roomAdmin;
        }

        if (resultRoomFlag)
        {
            if (runner.IsSharedModeMasterClient)
            {
                int mode = roomMode.text.Equals("개인전") ? 1 : 2;
                var roomOpt = new Dictionary<string, SessionProperty>() {
                    {"roomName", roomTitle.text}, {"mode", mode}, {"maxPlayer", maxPlayer}, {"State", "대기중"}
                };

                runner.SessionInfo.UpdateCustomProperties(roomOpt);
            }

            playerJoin();
            mapPrint();
            resultRoomFlag = false;
            readyFlag = false;
            readyCount = 0;
        }

        if (myInfo.GetComponent<PlayerInfo>().roomNum == -1)
        {
            playerJoin();
        }

        if (RoomInfoObj.GetComponent<RoomInfo>().roomInfoChange == true)
        {
            roomInfoPrint();

            roomTitle.text = runner.SessionInfo.Properties["roomName"].PropertyValue.ToString();
            roomMode.text = runner.SessionInfo.Properties["mode"].PropertyValue.ToString() == "1" ? "개인전" : "팀전";
            roomPlayer.text = runner.SessionInfo.Properties["maxPlayer"].PropertyValue.ToString();
            mapPrint();
            myRoomSync();
        }
        if (RoomInfoObj.GetComponent<RoomInfo>().mapChange == true) mapPrint();

        if (RoomInfoObj.GetComponent<RoomInfo>().getOut != "")
        {
            if (RoomInfoObj.GetComponent<RoomInfo>().getOut.Equals(ENB.id))
            {
                ENB.roomGetOut = true;
                U_LeaveRoom();
            }
        }

        if (ENB.inviState == 1)
        {
            U_InviStateList();
            ENB.inviState = 0;
        }

        if (readyFlag == true)
        {
            if (readyCount == nowPlayer)
            {
                int mode = roomMode.text.Equals("개인전") ? 1 : 2;
                var roomOpt = new Dictionary<string, SessionProperty>() {
                    {"roomName", roomTitle.text}, {"mode", mode}, {"maxPlayer", maxPlayer}, {"State", "게임중"}
                };

                runner.SessionInfo.UpdateCustomProperties(roomOpt);

                if (runner.IsSharedModeMasterClient)
                {
                    if (RoomInfoObj.GetComponent<RoomInfo>().map == 0) RoomInfoObj.GetComponent<RoomInfo>().map = int.Parse(Random.Range(1, 4).ToString());
                    if (RoomInfoObj.GetComponent<RoomInfo>().mapTime == 0) RoomInfoObj.GetComponent<RoomInfo>().mapTime = int.Parse(Random.Range(1, 6).ToString());

                    RoomInfoObj.GetComponent<RoomInfo>().playerCount = nowPlayer;

                    Invoke("StartGame", 0.5f);
                }
                else
                {
                    Invoke("StartGame", 0.5f);
                }
            }
            else readyFlag = false;
        }
    }
    void OnChatEnter(InputValue value)
    {
        if (ChatInput.isFocused == false) ChatInput.Select();
    }

    void StartGame()
    {
        localPlayerList = infoBox.GetComponentsInChildren<Transform>();
        for (int i = 1; i < localPlayerList.Length; i++)
            if (localPlayerList[i].GetComponent<PlayerInfo>().id.Equals(ENB.id))
                localPlayerList[i].GetComponent<PlayerInfo>().ready = false;

        ENB.gamePlay = true;
        DontDestroyOnLoad(infoBox);
        DontDestroyOnLoad(RoomInfoObj);

        if (RoomInfoObj.GetComponent<RoomInfo>().map == 1) SceneManager.LoadScene("G_RaceTrack");
        else if (RoomInfoObj.GetComponent<RoomInfo>().map == 2) SceneManager.LoadScene("G_Mountain");
        else if (RoomInfoObj.GetComponent<RoomInfo>().map == 3) SceneManager.LoadScene("G_City");
    }

    public void U_LeaveRoom()
    {
        GameObject obj = myInfoObject();
        Destroy(obj);

        GameManager.instance.U_InviReset();
        ENB.gameinfo.room = "x";

        runner.Shutdown();

        GameManager.instance.U_LRChange("x");
        SceneManager.LoadScene("Loading");
    }

    /* RoomInfoObject Init */
    public void roomInfoInit(NetworkObject obj)
    {
        obj.GetComponent<RoomInfo>().roomAdmin = ENB.id;
        obj.GetComponent<RoomInfo>().map = 0;
        obj.GetComponent<RoomInfo>().mapTime = 0;
    }
    public void myRoomSync()
    {
        MyRoomInfo.Instance.map = RoomInfoObj.GetComponent<RoomInfo>().map;
        MyRoomInfo.Instance.mapTime = RoomInfoObj.GetComponent<RoomInfo>().mapTime;
        MyRoomInfo.Instance.s0 = RoomInfoObj.GetComponent<RoomInfo>().s0;
        MyRoomInfo.Instance.s1 = RoomInfoObj.GetComponent<RoomInfo>().s1;
        MyRoomInfo.Instance.s2 = RoomInfoObj.GetComponent<RoomInfo>().s2;
        MyRoomInfo.Instance.s3 = RoomInfoObj.GetComponent<RoomInfo>().s3;
        MyRoomInfo.Instance.s4 = RoomInfoObj.GetComponent<RoomInfo>().s4;
        MyRoomInfo.Instance.s5 = RoomInfoObj.GetComponent<RoomInfo>().s5;
        MyRoomInfo.Instance.s6 = RoomInfoObj.GetComponent<RoomInfo>().s6;
        MyRoomInfo.Instance.s7 = RoomInfoObj.GetComponent<RoomInfo>().s7;
    }
    public void roomInfoSync(NetworkObject obj)
    {
        obj.GetComponent<RoomInfo>().map = MyRoomInfo.Instance.map;
        obj.GetComponent<RoomInfo>().mapTime = MyRoomInfo.Instance.mapTime;
        obj.GetComponent<RoomInfo>().s0 = MyRoomInfo.Instance.s0;
        obj.GetComponent<RoomInfo>().s1 = MyRoomInfo.Instance.s1;
        obj.GetComponent<RoomInfo>().s2 = MyRoomInfo.Instance.s2;
        obj.GetComponent<RoomInfo>().s3 = MyRoomInfo.Instance.s3;
        obj.GetComponent<RoomInfo>().s4 = MyRoomInfo.Instance.s4;
        obj.GetComponent<RoomInfo>().s5 = MyRoomInfo.Instance.s5;
        obj.GetComponent<RoomInfo>().s6 = MyRoomInfo.Instance.s6;
        obj.GetComponent<RoomInfo>().s7 = MyRoomInfo.Instance.s7;
    }
    GameObject myInfoObject()
    {
        localPlayerList = infoBox.GetComponentsInChildren<Transform>();

        for (int i = 1; i < localPlayerList.Length; i++)
            if (localPlayerList[i].GetComponent<PlayerInfo>().id.Equals(ENB.id))
                return localPlayerList[i].gameObject;

        return null;
    }

    /* Room Setting */
    public void U_RoomSettingOpne()
    {
        if (runner.IsSharedModeMasterClient)
        {
            unSetCarGun();
            roomSetPanel.SetActive(true);
        }
        else AlarmOpen("방장만 이용할 수 있습니다.");
    }
    public void U_RoomSettingClose()
    {
        setCarGun();
        roomSetName.text = "";
        roomSetPanel.SetActive(false);
    }
    public void U_RoomSettingApply()
    {
        roomInfoFalg = true;
        roomInfoSetting();
    }
    void roomInfoPrint()
    {
        roomTitle.text = runner.SessionInfo.Properties["roomName"].PropertyValue.ToString();
        roomMode.text = runner.SessionInfo.Properties["mode"].PropertyValue.ToString() == "1" ? "개인전" : "팀전";
        roomPlayer.text = runner.SessionInfo.Properties["maxPlayer"].PropertyValue.ToString();
        modeSlot(roomMode.text);
    }
    void roomInfoSetting()
    {
        if (roomInfoFalg == true)
        {
            if (roomSetName.text.Equals("")) // Slot OC
            {
                int mode = roomMode.text.Equals("개인전") ? 1 : 2;
                var roomOpt = new Dictionary<string, SessionProperty>() {
                    {"roomName", roomTitle.text}, {"mode", mode}, {"maxPlayer", maxPlayer}, {"State", "대기중"}
                };

                runner.SessionInfo.UpdateCustomProperties(roomOpt);

                RoomInfoObj.GetComponent<RoomInfo>().roomInfoChange = true;
                Invoke("roomSettingEnd", 0.3f);
            }
            else // Room Setting
            {
                int mode = roomSetMode.options[roomSetMode.value].text.Equals("개인전") ? 1 : 2;
                var roomOpt = new Dictionary<string, SessionProperty>() {
                    {"roomName", roomSetName.text}, {"mode", mode}, {"maxPlayer", maxPlayer}, {"State", "대기중"}
                };

                runner.SessionInfo.UpdateCustomProperties(roomOpt);

                RoomInfoObj.GetComponent<RoomInfo>().roomInfoChange = true;
                Invoke("roomSettingEnd", 0.3f);

                U_RoomSettingClose();
            }

            roomInfoFalg = false;
        }

        roomTitle.text = runner.SessionInfo.Properties["roomName"].PropertyValue.ToString();
        roomMode.text = runner.SessionInfo.Properties["mode"].PropertyValue.ToString() == "1" ? "개인전" : "팀전";
        roomPlayer.text = runner.SessionInfo.Properties["maxPlayer"].PropertyValue.ToString();
        modeSlot(roomMode.text);
    }
    void roomSettingEnd() => RoomInfoObj.GetComponent<RoomInfo>().roomInfoChange = false;

    /* Map Setting */
    public void U_MapSettingOpen()
    {
        if (runner.IsSharedModeMasterClient)
        {
            unSetCarGun();
            mapSetPanel.SetActive(true);

            childDestroy(mapScrollContent);

            GameObject random = GameObject.Instantiate(mapListInstance, mapScrollContent.transform);
            random.transform.parent = mapScrollContent.transform;

            random.transform.GetChild(0).GetComponent<Text>().text = "RANDOM";
            random.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/RANDOM");
            random.GetComponent<Button>().onClick.AddListener(delegate { U_MapSelect("RANDOM", random); });

            for (int i = 0; i < ENB.tracks.Count; i++)
            {
                GameObject tmp = GameObject.Instantiate(mapListInstance, mapScrollContent.transform);
                tmp.transform.parent = mapScrollContent.transform;

                string tname = ENB.tracks[i].tname;

                tmp.transform.GetChild(0).GetComponent<Text>().text = ENB.tracks[i].tname;
                tmp.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + ENB.tracks[i].tname);
                tmp.GetComponent<Button>().onClick.AddListener(delegate { U_MapSelect(tname, tmp); });
            }
        }
        else AlarmOpen("방장만 이용할 수 있습니다.");
    }
    public void U_MapSettingClose()
    {
        setCarGun();
        mapSetPanel.SetActive(false);
    }
    public void U_MapSettingApply()
    {
        int time = mapTimeReturn(-1);

        RoomInfoObj.GetComponent<RoomInfo>().map = selectMap;
        RoomInfoObj.GetComponent<RoomInfo>().mapTime = time;
        RoomInfoObj.GetComponent<RoomInfo>().mapChange = true;
        Invoke("mapSettingEnd", 0.3f);
    }
    void U_MapSelect(string name, GameObject obj)
    {
        if (prevMapBtn != null)
        {
            prevMapBtn.GetComponent<Button>().interactable = true;
            Color c = prevMapBtn.transform.GetChild(1).GetComponent<Image>().color;
            c.a = 1f;
            prevMapBtn.transform.GetChild(1).GetComponent<Image>().color = c;
        }
        prevMapBtn = obj;
        obj.GetComponent<Button>().interactable = false;
        Color color = obj.transform.GetChild(1).GetComponent<Image>().color;
        color.a = 0.5f;
        obj.transform.GetChild(1).GetComponent<Image>().color = color;

        if (name.Equals("RANDOM"))
        {
            selectMap = 0;
            mapTitle.text = "RANDOM";
            mapContent.text = "RANDOM";
            mapImage.sprite = Resources.Load<Sprite>("Sprites/RANDOM");
            return;
        }
        else
        {
            for(int i = 0; i < ENB.tracks.Count; i++)
            {
                if (ENB.tracks[i].tname.Equals(name))
                {
                    selectMap = i + 1;
                    mapTitle.text = ENB.tracks[i].tname;
                    mapContent.text = ENB.tracks[i].content;
                    mapImage.sprite = Resources.Load<Sprite>("Sprites/" + ENB.tracks[i].tname);
                }
            }
        }
    }
    public void mapApply() => Invoke("mapPrint", 0.3f);
    void mapPrint()
    {
        if (RoomInfoObj.GetComponent<RoomInfo>().map == 0) roomMap.sprite = Resources.Load<Sprite>("Sprites/RANDOM");
        else roomMap.sprite = Resources.Load<Sprite>("Sprites/" + ENB.tracks[RoomInfoObj.GetComponent<RoomInfo>().map - 1].tname);
        mapTimeReturn(0);
    }
    int mapTimeReturn(int num)
    {
        int time = 0;
        if (num == -1)
        {
            string str = mapTime.options[mapTime.value].text;
            switch(str)
            {
                case "랜덤 시간": time = 0; break;
                case "아침 시간": time = 1; break;
                case "오전 시간": time = 2; break;
                case "낮 시간": time = 3; break;
                case "오후 시간": time = 4; break;
                case "저녁 시간": time = 5; break;
            }
        }
        else
        {
            time = RoomInfoObj.GetComponent<RoomInfo>().mapTime;
            switch(time)
            {
                case 0: roomTime.text = "랜덤 시간"; break;
                case 1: roomTime.text = "아침 시간"; break;
                case 2: roomTime.text = "오전 시간"; break;
                case 3: roomTime.text = "낮 시간"; break;
                case 4: roomTime.text = "오후 시간"; break;
                case 5: roomTime.text = "저녁 시간"; break;
            }
        }

        return time;
    }
    void mapSettingEnd() => RoomInfoObj.GetComponent<RoomInfo>().mapChange = false;

    /* Ready */
    public void U_ReadyBtn()
    {
        GameObject obj = myInfoObject();

        if (obj.GetComponent<PlayerInfo>().ready == true) obj.GetComponent<PlayerInfo>().ready = false;
        else obj.GetComponent<PlayerInfo>().ready = true;
    }
    public void ready()
    {
        readyCount = 0;

        localPlayerList = infoBox.GetComponentsInChildren<Transform>();
        nowPlayer = localPlayerList.Length - 1;

        for (int i = 0; i < 8; i++) Playerinfos[i].transform.GetChild(7).gameObject.SetActive(false);

        for (int i = 1; i < localPlayerList.Length; i++)
        {
            int num = localPlayerList[i].GetComponent<PlayerInfo>().roomNum;

            if (localPlayerList[i].GetComponent<PlayerInfo>().ready == true)
            {
                Playerinfos[num].transform.GetChild(7).gameObject.SetActive(true);
                readyCount++;
            }
            else Playerinfos[num].transform.GetChild(7).gameObject.SetActive(false);
        }

        readyFlag = true;
    }

    /* Alarm */
    public void AlarmOpen(string str)
    {
        unSetCarGun();
        AlarmPanel.SetActive(true);
        AlarmText.text = str;
    }
    public void U_AlarmClose()
    {
        setCarGun();
        AlarmText.text = "";
        AlarmPanel.SetActive(false);
    }
    void unSetCarGun()
    {
        for (int i = 0; i < 8; i++)
            Playerinfos[i].transform.GetChild(4).gameObject.SetActive(false);
    }
    void setCarGun()
    {
        for (int i = 0; i < 8; i++)
            Playerinfos[i].transform.GetChild(4).gameObject.SetActive(true);
    }

    /* User Get Out */
    public void U_GetOut(int num)
    {
        if (runner.IsSharedModeMasterClient)
        {
            string id = Playerinfos[num].transform.GetChild(0).GetComponent<Text>().text;

            if (!id.Equals("ID"))
            {
                if (id.Equals(ENB.id)) AlarmOpen("자신을 강퇴할 수 없습니다.");
                else
                {
                    RoomInfoObj.GetComponent<RoomInfo>().getOut = id;
                    Invoke("masterGetOut", 0.5f);
                }
            }
        }
        else AlarmOpen("방장만 강퇴할 수 있습니다.");
    }
    void masterGetOut() => RoomInfoObj.GetComponent<RoomInfo>().getOut = "";

    /* User Join And Left */
    public void U_joinPlayer() => Invoke("playerJoin", 0.3f);
    public void playerJoin()
    {
        infoBox = GameObject.FindGameObjectWithTag("InfoBox");
        Transform[] infoList = infoBox.GetComponentsInChildren<Transform>();
        for (int i = 1; i < infoList.Length; i++)
            if (infoList[i].GetComponent<PlayerInfo>().id.Equals(ENB.id))
                myInfo = infoList[i].gameObject;

        GameObject[] infos = GameObject.FindGameObjectsWithTag("PlayerInfo");
        for (int i = 0; i < infos.Length; i++)
        {
            if (infos[i].GetComponent<NetworkObject>().StateAuthority.IsNone) Destroy(infos[i]);
            else infos[i].transform.parent = infoBox.transform;
        }

        localPlayerList = infoBox.GetComponentsInChildren<Transform>();

        slotClean(-1);

        int max = int.Parse(runner.SessionInfo.Properties["maxPlayer"].PropertyValue.ToString());

        for (int i = 1; i < localPlayerList.Length; i++)
        {
            if (localPlayerList[i].GetComponent<PlayerInfo>().roomNum == -1) // join user
            {
                for (int j = 0; j < 8; j++)
                {
                    if (slotoc.Contains(j)) continue;

                    if (Playerinfos[j].transform.GetChild(0).GetComponent<Text>().text.Equals("ID")) // empty
                    {
                        Playerinfos[j].transform.GetChild(0).GetComponent<Text>().text = localPlayerList[i].GetComponent<PlayerInfo>().id;
                        Playerinfos[j].transform.GetChild(0).GetComponent<Text>().fontSize = 20;
                        Playerinfos[j].transform.GetChild(1).gameObject.SetActive(true);
                        Playerinfos[j].transform.GetChild(2).GetComponent<Text>().text = localPlayerList[i].GetComponent<PlayerInfo>().car;
                        Playerinfos[j].transform.GetChild(3).GetComponent<Text>().text = localPlayerList[i].GetComponent<PlayerInfo>().gun;
                        Playerinfos[j].transform.GetChild(6).gameObject.SetActive(false);

                        if (roomMode.text.Equals("개인전")) localPlayerList[i].GetComponent<PlayerInfo>().team = "i";
                        else if (j % 2 == 0) localPlayerList[i].GetComponent<PlayerInfo>().team = "r";
                        else localPlayerList[i].GetComponent<PlayerInfo>().team = "b";

                        GameObject tmp = Resources.Load("Car/" + localPlayerList[i].GetComponent<PlayerInfo>().car) as GameObject;
                        GameObject item = Instantiate(tmp, Playerinfos[j].transform.GetChild(4).transform);

                        item.transform.parent = Playerinfos[j].transform.GetChild(4).transform;
                        item.transform.localPosition = new Vector3(itemPosition[j, 0], itemPosition[j, 1], itemPosition[j, 2]);
                        item.transform.rotation = Quaternion.Euler(itemRotation[j, 0], itemRotation[j, 1], itemRotation[j, 2]);
                        item.transform.localScale = new Vector3(itemScale[j, 0], itemScale[j, 1], itemScale[j, 2]);

                        localPlayerList[i].GetComponent<PlayerInfo>().roomNum = j;
                        break;
                    }
                }
            }
            else // player
            {
                for (int j = 0; j < 8; j++)
                {
                    if (localPlayerList[i].GetComponent<PlayerInfo>().roomNum == j)
                    {
                        Playerinfos[j].transform.GetChild(0).GetComponent<Text>().text = localPlayerList[i].GetComponent<PlayerInfo>().id;
                        Playerinfos[j].transform.GetChild(0).GetComponent<Text>().fontSize = 20;
                        Playerinfos[j].transform.GetChild(1).gameObject.SetActive(true);
                        Playerinfos[j].transform.GetChild(2).GetComponent<Text>().text = localPlayerList[i].GetComponent<PlayerInfo>().car;
                        Playerinfos[j].transform.GetChild(3).GetComponent<Text>().text = localPlayerList[i].GetComponent<PlayerInfo>().gun;
                        Playerinfos[j].transform.GetChild(6).gameObject.SetActive(false);

                        if (roomMode.text.Equals("개인전")) localPlayerList[i].GetComponent<PlayerInfo>().team = "i";
                        else if (j % 2 == 0) localPlayerList[i].GetComponent<PlayerInfo>().team = "r";
                        else localPlayerList[i].GetComponent<PlayerInfo>().team = "b";

                        GameObject tmp = Resources.Load("Car/" + localPlayerList[i].GetComponent<PlayerInfo>().car) as GameObject;
                        GameObject item = Instantiate(tmp, Playerinfos[j].transform.GetChild(4).transform);

                        item.transform.parent = Playerinfos[j].transform.GetChild(4).transform;
                        item.transform.localPosition = new Vector3(itemPosition[j, 0], itemPosition[j, 1], itemPosition[j, 2]);
                        item.transform.rotation = Quaternion.Euler(itemRotation[j, 0], itemRotation[j, 1], itemRotation[j, 2]);
                        item.transform.localScale = new Vector3(itemScale[j, 0], itemScale[j, 1], itemScale[j, 2]);

                        break;
                    }
                }
            }
        }

        ready();
    }
    public void U_leftPlayer() => playerLeft();
    void playerLeft()
    {
        infoBox = GameObject.FindGameObjectWithTag("InfoBox");

        localPlayerList = infoBox.GetComponentsInChildren<Transform>();

        int roomNum;
        adminChange = 1;

        for (int i = 1; i < localPlayerList.Length; i++)
        {
            if (runner.IsSharedModeMasterClient)
            {
                if (!RoomInfoObj.GetComponent<RoomInfo>().roomAdmin.Equals(ENB.id))
                {
                    if (adminChange == 1)
                    {
                        Destroy(RoomInfoObj);
                        NRunner.instance.RoomPvpChangeAdmin(runner, ENB.pid);
                        adminChange = 0;
                    }
                }
                else adminChange = 0;
            }
            if (localPlayerList[i].GetComponent<NetworkObject>().StateAuthority.IsNone)
            {
                Playerinfos[i].transform.GetChild(1).gameObject.SetActive(false);
                Playerinfos[i].transform.GetChild(6).gameObject.SetActive(true);
                roomNum = localPlayerList[i].GetComponent<PlayerInfo>().roomNum;
                Destroy(localPlayerList[i].gameObject);
                slotClean(roomNum);
            }
            if (localPlayerList[i].GetComponent<NetworkObject>().StateAuthority != localPlayerList[i].GetComponent<NetworkObject>().InputAuthority)
            {
                Playerinfos[i].transform.GetChild(1).gameObject.SetActive(false);
                Playerinfos[i].transform.GetChild(6).gameObject.SetActive(true);
                roomNum = localPlayerList[i].GetComponent<PlayerInfo>().roomNum;
                Destroy(localPlayerList[i].gameObject);
                slotClean(roomNum);
            }
        }
    }
    void modeSlot(string mode)
    {
        if (mode.Equals("팀전"))
            for (int i = 0; i < Playerinfos.Length; i++)
            {
                if (i % 2 == 0) Playerinfos[i].GetComponent<Image>().color = redTeam;
                else Playerinfos[i].GetComponent<Image>().color = blueTeam;
            }
        else
            for (int i = 0; i < Playerinfos.Length; i++)
                Playerinfos[i].GetComponent<Image>().color = individual;
    }
    void slotClean(int num)
    {
        if (num != -1)
        {
            Playerinfos[num].transform.GetChild(0).GetComponent<Text>().text = "ID";
            Playerinfos[num].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
            Playerinfos[num].transform.GetChild(1).gameObject.SetActive(false);
            Playerinfos[num].transform.GetChild(2).GetComponent<Text>().text = "";
            Playerinfos[num].transform.GetChild(3).GetComponent<Text>().text = "";
            Playerinfos[num].transform.GetChild(6).gameObject.SetActive(true);
            Playerinfos[num].transform.GetChild(7).gameObject.SetActive(false);

            Transform[] child = Playerinfos[num].transform.GetChild(4).GetComponentsInChildren<Transform>();
            if (child != null) for (int j = 1; j < child.Length; j++) if (child[j] != transform) Destroy(child[j].gameObject);
        }
        else
        {
            for (int i = 0; i < Playerinfos.Length; i++)
            {
                Playerinfos[i].transform.GetChild(0).GetComponent<Text>().text = "ID";
                Playerinfos[i].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
                Playerinfos[i].transform.GetChild(1).gameObject.SetActive(false);
                Playerinfos[i].transform.GetChild(2).GetComponent<Text>().text = "";
                Playerinfos[i].transform.GetChild(3).GetComponent<Text>().text = "";
                Playerinfos[i].transform.GetChild(6).gameObject.SetActive(true);
                Playerinfos[i].transform.GetChild(7).gameObject.SetActive(false);

                Transform[] child = Playerinfos[i].transform.GetChild(4).GetComponentsInChildren<Transform>();
                if (child != null) for (int j = 1; j < child.Length; j++) if (child[j] != transform) Destroy(child[j].gameObject);
            }
        }
        slotOC();
    }

    /* Slot Change */
    public void U_HistAndOCSlot(int num)
    {
        if (Playerinfos[num].transform.GetChild(0).GetComponent<Text>().text.Equals("ID"))
        {
            if (runner.IsSharedModeMasterClient)
                switch (num)
                {
                    case 0: RoomInfoObj.GetComponent<RoomInfo>().s0 = !RoomInfoObj.GetComponent<RoomInfo>().s0; break;
                    case 1: RoomInfoObj.GetComponent<RoomInfo>().s1 = !RoomInfoObj.GetComponent<RoomInfo>().s1; break;
                    case 2: RoomInfoObj.GetComponent<RoomInfo>().s2 = !RoomInfoObj.GetComponent<RoomInfo>().s2; break;
                    case 3: RoomInfoObj.GetComponent<RoomInfo>().s3 = !RoomInfoObj.GetComponent<RoomInfo>().s3; break;
                    case 4: RoomInfoObj.GetComponent<RoomInfo>().s4 = !RoomInfoObj.GetComponent<RoomInfo>().s4; break;
                    case 5: RoomInfoObj.GetComponent<RoomInfo>().s5 = !RoomInfoObj.GetComponent<RoomInfo>().s5; break;
                    case 6: RoomInfoObj.GetComponent<RoomInfo>().s6 = !RoomInfoObj.GetComponent<RoomInfo>().s6; break;
                    case 7: RoomInfoObj.GetComponent<RoomInfo>().s7 = !RoomInfoObj.GetComponent<RoomInfo>().s7; break;
                }
        }
        else U_HistoryOpen(Playerinfos[num].transform.GetChild(0).GetComponent<Text>().text);
    }
    public void slotOC()
    {
        maxPlayer = 0;

        if (RoomInfoObj.GetComponent<RoomInfo>().s0 == true)
        {
            slotoc.Add(0);
            Playerinfos[0].transform.GetChild(5).gameObject.SetActive(true);
            Playerinfos[0].transform.GetChild(6).gameObject.SetActive(false);
            Playerinfos[0].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
        }
        else
        {
            if (slotoc.Contains(0))
            {
                slotoc.Remove(0);
                Playerinfos[0].transform.GetChild(5).gameObject.SetActive(false);
                Playerinfos[0].transform.GetChild(6).gameObject.SetActive(true);
                Playerinfos[0].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
            }
            if (runner.IsSharedModeMasterClient) maxPlayer++;
        }

        if (RoomInfoObj.GetComponent<RoomInfo>().s1 == true)
        {
            slotoc.Add(1);
            Playerinfos[1].transform.GetChild(5).gameObject.SetActive(true);
            Playerinfos[1].transform.GetChild(6).gameObject.SetActive(false);
            Playerinfos[1].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
        }
        else
        {
            if (slotoc.Contains(1))
            {
                slotoc.Remove(1);
                Playerinfos[1].transform.GetChild(5).gameObject.SetActive(false);
                Playerinfos[1].transform.GetChild(6).gameObject.SetActive(true);
                Playerinfos[1].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
            }
            if (runner.IsSharedModeMasterClient) maxPlayer++;
        }

        if (RoomInfoObj.GetComponent<RoomInfo>().s2 == true)
        {
            slotoc.Add(2);
            Playerinfos[2].transform.GetChild(5).gameObject.SetActive(true);
            Playerinfos[2].transform.GetChild(6).gameObject.SetActive(false);
            Playerinfos[2].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
        }
        else
        {
            if (slotoc.Contains(2))
            {
                slotoc.Remove(2);
                Playerinfos[2].transform.GetChild(5).gameObject.SetActive(false);
                Playerinfos[2].transform.GetChild(6).gameObject.SetActive(true);
                Playerinfos[2].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
            }
            if (runner.IsSharedModeMasterClient) maxPlayer++;
        }

        if (RoomInfoObj.GetComponent<RoomInfo>().s3 == true)
        {
            slotoc.Add(3);
            Playerinfos[3].transform.GetChild(5).gameObject.SetActive(true);
            Playerinfos[3].transform.GetChild(6).gameObject.SetActive(false);
            Playerinfos[3].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
        }
        else
        {
            if (slotoc.Contains(3))
            {
                slotoc.Remove(3);
                Playerinfos[3].transform.GetChild(5).gameObject.SetActive(false);
                Playerinfos[3].transform.GetChild(6).gameObject.SetActive(true);
                Playerinfos[3].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
            }
            if (runner.IsSharedModeMasterClient) maxPlayer++;
        }

        if (RoomInfoObj.GetComponent<RoomInfo>().s4 == true)
        {
            slotoc.Add(4);
            Playerinfos[4].transform.GetChild(5).gameObject.SetActive(true);
            Playerinfos[4].transform.GetChild(6).gameObject.SetActive(false);
            Playerinfos[4].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
        }
        else
        {
            if (slotoc.Contains(4))
            {
                slotoc.Remove(4);
                Playerinfos[4].transform.GetChild(5).gameObject.SetActive(false);
                Playerinfos[4].transform.GetChild(6).gameObject.SetActive(true);
                Playerinfos[4].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
            }
            if (runner.IsSharedModeMasterClient) maxPlayer++;
        }

        if (RoomInfoObj.GetComponent<RoomInfo>().s5 == true)
        {
            slotoc.Add(5);
            Playerinfos[5].transform.GetChild(5).gameObject.SetActive(true);
            Playerinfos[5].transform.GetChild(6).gameObject.SetActive(false);
            Playerinfos[5].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
        }
        else
        {
            if (slotoc.Contains(5))
            {
                slotoc.Remove(5);
                Playerinfos[5].transform.GetChild(5).gameObject.SetActive(false);
                Playerinfos[5].transform.GetChild(6).gameObject.SetActive(true);
                Playerinfos[5].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
            }
            if (runner.IsSharedModeMasterClient) maxPlayer++;
        }

        if (RoomInfoObj.GetComponent<RoomInfo>().s6 == true)
        {
            slotoc.Add(6);
            Playerinfos[6].transform.GetChild(5).gameObject.SetActive(true);
            Playerinfos[6].transform.GetChild(6).gameObject.SetActive(false);
            Playerinfos[6].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
        }
        else
        {
            if (slotoc.Contains(6))
            {
                slotoc.Remove(6);
                Playerinfos[6].transform.GetChild(5).gameObject.SetActive(false);
                Playerinfos[6].transform.GetChild(6).gameObject.SetActive(true);
                Playerinfos[6].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
            }
            if (runner.IsSharedModeMasterClient) maxPlayer++;
        }

        if (RoomInfoObj.GetComponent<RoomInfo>().s7 == true)
        {
            slotoc.Add(7);
            Playerinfos[7].transform.GetChild(5).gameObject.SetActive(true);
            Playerinfos[7].transform.GetChild(6).gameObject.SetActive(false);
            Playerinfos[7].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
        }
        else
        {
            if (slotoc.Contains(7))
            {
                slotoc.Remove(7);
                Playerinfos[7].transform.GetChild(5).gameObject.SetActive(false);
                Playerinfos[7].transform.GetChild(6).gameObject.SetActive(true);
                Playerinfos[7].transform.GetChild(0).GetComponent<Text>().fontSize = -100;
            }
            if (runner.IsSharedModeMasterClient) maxPlayer++;
        }

        if (runner.IsSharedModeMasterClient)
        {
            roomInfoFalg = true;
            roomInfoSetting();
        }
    }
    public void U_slotChange(int num)
    {
        GameObject obj = myInfoObject();

        obj.GetComponent<PlayerInfo>().roomNum = num;
    }

    /* History */
    public void U_HistFriend() => StartCoroutine(friendAdd());
    IEnumerator friendAdd()
    {
        if (ENB.history[0] != null)
        {
            var form = new WWWForm();
            form.AddField("id", ENB.userinfo.id);
            form.AddField("fid", ENB.history[0].id);
            var www = UnityWebRequest.Post(GameManager.serverURL + "/friendAdd", form);
            yield return www.SendWebRequest();

            AlarmOpen("추가되었습니다.");
        }
    }
    public void U_HistEnd()
    {
        setCarGun();
        HistId.text = "";
        HistCar.text = "";
        HistGun.text = "";
        HistoryPanel.gameObject.SetActive(false);
    }
    public void U_HistoryOpen(string id)
    {
        unSetCarGun();
        HistoryPanel.gameObject.SetActive(true);

        ENB.history.Clear();

        StartCoroutine(HistoryInsert(id));

        Invoke("HistoryListUpdate", 0.3f);
    }
    IEnumerator HistoryInsert(string id)
    {
        var form = new WWWForm();
        form.AddField("id", id);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/historyList", form);
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            ENB.history.Add(JsonUtility.FromJson<DBClass.history>(data[i]));
    }
    void HistoryListUpdate()
    {
        Transform[] childList = HistoryContent.GetComponentsInChildren<Transform>();

        if (childList != null) for (int i = 1; i < childList.Length; i++) if (childList[i] != transform) Destroy(childList[i].gameObject);

        GameObject tmp;
        for (int i = 0; i < ENB.history.Count; i++)
        {
            tmp = GameObject.Instantiate(HistoryList, HistoryContent.transform);
            tmp.transform.parent = HistoryContent.transform;

            if (ENB.history[i] == null)
            {
                tmp.transform.GetChild(4).GetComponent<Text>().text = "대전 기록이 존재하지 않습니다.";
                tmp.GetComponent<Image>().color = new Color(174 / 255f, 174 / 255f, 174 / 255f);
                break;
            }
            else
            {
                if (ENB.history[i].result.Equals("1")) tmp.GetComponent<Image>().color = new Color(255 / 255f, 215 / 255f, 0 / 255f);
                else if (ENB.history[i].result.Equals("2")) tmp.GetComponent<Image>().color = new Color(192 / 255f, 192 / 255f, 192 / 255f);
                else if (ENB.history[i].result.Equals("3")) tmp.GetComponent<Image>().color = new Color(164 / 255f, 124 / 255f, 109 / 255f);
                else tmp.GetComponent<Image>().color = new Color(123 / 255f, 123 / 255f, 123 / 255f);

                HistId.text = ENB.history[i].id;
                HistCar.text = ENB.history[i].usecar;
                HistGun.text = ENB.history[i].usegun;

                string time = ENB.history[i].matchDate.Replace("T", " ");
                time = time.Replace(".000Z", "");

                tmp.transform.GetChild(0).GetComponent<Text>().text = time;
                tmp.transform.GetChild(1).GetComponent<Text>().text = ENB.history[i].result + " 등";
                tmp.transform.GetChild(2).GetComponent<Text>().text = ENB.history[i].resultcar;
                tmp.transform.GetChild(3).GetComponent<Text>().text = ENB.history[i].resultgun;
            }
        }
    }

    /* Garage */
    public void U_GarageRoom()
    {
        unSetCarGun();
        GaragePanel.SetActive(true);

        childDestroy(carScrollContent);

        for (int i = 0; i < ENB.carinfos.Count; i++)
        {
            if (useCar(i) == 1)
            {
                GameObject tmp = GameObject.Instantiate(cargunInstance, carScrollContent.transform);
                tmp.transform.parent = carScrollContent.transform;

                string carname = ENB.carinfos[i].cname;

                if (car != null && car.cname.Equals(carname))
                {
                    prevCarBtn = tmp;
                    tmp.GetComponent<Button>().interactable = false;
                }
                else if (car == null && ENB.gameinfo.usecar.Equals(carname))
                {
                    prevCarBtn = tmp;
                    tmp.GetComponent<Button>().interactable = false;
                }

                tmp.transform.GetChild(0).GetComponent<Text>().text = carname;
                tmp.GetComponent<Button>().onClick.AddListener(delegate { U_SelectCar(carname, tmp); });
            }
        }

        childDestroy(gunScrollContent);

        for (int i = 0; i < ENB.guninfos.Count; i++)
        {
            if (useGun(i) == 1)
            {
                GameObject tmp = GameObject.Instantiate(cargunInstance, gunScrollContent.transform);
                tmp.transform.parent = gunScrollContent.transform;

                string gunname = ENB.guninfos[i].gname;

                if (gun != null && gun.gname.Equals(gunname))
                {
                    prevGunBtn = tmp;
                    tmp.GetComponent<Button>().interactable = false;
                }
                else if (gun == null && ENB.gameinfo.usegun.Equals(gunname))
                {
                    prevGunBtn = tmp;
                    tmp.GetComponent<Button>().interactable = false;
                }

                tmp.transform.GetChild(0).GetComponent<Text>().text = gunname;
                tmp.GetComponent<Button>().onClick.AddListener(delegate { U_SelectGun(gunname, tmp); });
            }
        }
    }
    int useCar(int num)
    {
        if (ENB.usecars[0] == null) return 0;

        for (int i = 0; i < ENB.usecars.Count; i++)
            if (ENB.usecars[i].cnum == num + 1) return 1;

        return 0;
    }
    int useGun(int num)
    {
        if (ENB.useguns[0] == null) return 0;

        for (int i = 0; i < ENB.useguns.Count; i++)
            if (ENB.useguns[i].gnum == num + 1) return 1;

        return 0;
    }
    void U_SelectCar(string carname, GameObject obj)
    {
        if (prevCarBtn != null) prevCarBtn.GetComponent<Button>().interactable = true;
        prevCarBtn = obj;
        obj.GetComponent<Button>().interactable = false;

        for (int i = 0; i < ENB.carinfos.Count; i++)
            if (ENB.carinfos[i].cname.Equals(carname))
            {
                car = ENB.carinfos[i];
                break;
            }

        carTitle.text = car.cname;
        if (car.cash.Equals("o")) carCnM.text = "캐시";
        else carCnM.text = "게임머니";
        carPrice.text = car.price.ToString();
        carContent.text = car.content;

        HpImg.fillAmount = (car.hp * 0.01f) / (ENB.maxHp * 0.01f);
        DefImg.fillAmount = (car.def * 0.01f) / (ENB.maxDef * 0.01f);
        SpdImg.fillAmount = (car.speed * 0.01f) / (ENB.maxSpeed * 0.01f);
    }
    void U_SelectGun(string gunname, GameObject obj)
    {
        if (prevGunBtn != null) prevGunBtn.GetComponent<Button>().interactable = true;
        prevGunBtn = obj;
        obj.GetComponent<Button>().interactable = false;

        for (int i = 0; i < ENB.guninfos.Count; i++)
            if (ENB.guninfos[i].gname.Equals(gunname))
            {
                gun = ENB.guninfos[i];
                break;
            }

        gunTitle.text = gun.gname;
        if (gun.cash.Equals("o")) gunCnM.text = "캐시";
        else gunCnM.text = "게임머니";
        gunPrice.text = gun.price.ToString();
        gunContent.text = gun.content;

        AtkImg.fillAmount = (gun.atk * 0.01f) / (ENB.maxAtk * 0.01f);
        AmmoImg.fillAmount = (gun.ammo * 0.01f) / (ENB.maxAmmo * 0.01f);
        RpmImg.fillAmount = (gun.rpm * 0.01f) / (ENB.maxRpm * 0.01f);
    }
    public void U_GarageClose()
    {
        setCarGun();
        GaragePanel.SetActive(false);
    }
    public void U_GarageApply()
    {
        if (car != null) ENB.gameinfo.usecar = car.cname;
        if (gun != null) ENB.gameinfo.usegun = gun.gname;

        StartCoroutine(useItem());

        Invoke("usingItem", 0.3f);
    }
    IEnumerator useItem()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.userinfo.id);
        form.AddField("usecar", ENB.gameinfo.usecar);
        form.AddField("usegun", ENB.gameinfo.usegun);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/usingItem", form);
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        ENB.userinfo = JsonUtility.FromJson<DBClass.userinfo>(str);
    }
    void usingItem()
    {
        carText.text = ENB.gameinfo.usecar;
        gunText.text = ENB.gameinfo.usegun;

        GameObject obj = myInfoObject();
        obj.GetComponent<PlayerInfo>().car = ENB.gameinfo.usecar;
        obj.GetComponent<PlayerInfo>().gun = ENB.gameinfo.usegun;
    }

    /* Invitation */
    public void U_InvitationOpen()
    {
        if (runner.IsSharedModeMasterClient)
        {
            unSetCarGun();
            InvitationPanel.SetActive(true);
        }
        else AlarmOpen("방장만 이용할 수 있습니다.");
    }
    public void U_InvitationClose()
    {
        setCarGun();
        inviList.Clear();
        inviState.Clear();
        childDestroy(InviListContent);
        childDestroy(InviHistoryContent);
        childDestroy(InviStateContent);
        InvitationPanel.SetActive(false);
    }
    public void U_Invitation()
    {
        if (inviusername == null) AlarmOpen("초대할 유저를 선택해주세요.");
        else
        {
            Destroy(inviPrevBtn);
            LobbyChat.instance.ws.Send("/Invitation " + inviusername);
            StartCoroutine(Invitation());
            Invoke("U_InviStateList", 0.3f);
        }
    }
    IEnumerator Invitation()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.userinfo.id);
        form.AddField("inviId", inviusername);
        form.AddField("room", roomTitle.text);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/invitationAdd", form);
        yield return www.SendWebRequest();
    }
    void childDestroy(GameObject obj)
    {
        Transform[] childList = obj.GetComponentsInChildren<Transform>();

        if (childList != null) for (int i = 1; i < childList.Length; i++) if (childList[i] != transform) Destroy(childList[i].gameObject);
    }
    public void U_InviLobbyList()
    {
        inviList.Clear();
        childDestroy(InviListContent);
        StartCoroutine(InviLobbyList());
        Invoke("InviListUpdate", 0.3f);
    }
    IEnumerator InviLobbyList()
    {
        var www = UnityWebRequest.Get(GameManager.serverURL + "/invitationUserList");
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            inviList.Add(JsonUtility.FromJson<DBClass.inviuser>(data[i]));
    }
    public void U_InviFriendList()
    {
        inviList.Clear();
        childDestroy(InviListContent);
        StartCoroutine(InviFriendList());
        Invoke("InviListUpdate", 0.3f);
    }
    IEnumerator InviFriendList()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.userinfo.id);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/invitationFriendList", form);
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            inviList.Add(JsonUtility.FromJson<DBClass.inviuser>(data[i]));
    }
    void InviListUpdate()
    {
        for (int i = 0; i < inviList.Count; i++)
        {
            GameObject tmp = GameObject.Instantiate(InviListInstance, InviListContent.transform);
            tmp.transform.parent = InviListContent.transform;

            if (inviList[i] == null)
            {
                tmp.transform.GetChild(3).GetComponent<Text>().text = "초대할 수 있는 유저가 없습니다.";
                break;
            }
            else
            {
                string id = inviList[i].id;

                tmp.transform.GetChild(0).GetComponent<Text>().text = id;
                tmp.transform.GetChild(1).GetComponent<Text>().text = inviList[i].usecar;
                tmp.transform.GetChild(2).GetComponent<Text>().text = inviList[i].usegun;
                tmp.GetComponent<Button>().onClick.AddListener(delegate { InviSelectUser(id, tmp); });
            }
        }
    }
    void InviSelectUser(string id, GameObject obj)
    {
        if (inviPrevBtn != null) inviPrevBtn.GetComponent<Button>().interactable = true;
        inviPrevBtn = obj;
        inviPrevBtn.GetComponent<Button>().interactable = false;

        ENB.history.Clear();

        inviusername = id;
        childDestroy(InviHistoryContent);
        StartCoroutine(HistoryInsert(id));
        Invoke("InviHistoryUpdate", 0.3f);
    }
    void InviHistoryUpdate()
    {
        childDestroy(InviHistoryContent);

        for (int i = 0; i < ENB.history.Count; i++)
        {
            GameObject tmp = GameObject.Instantiate(InviHistoryInstance, InviHistoryContent.transform);
            tmp.transform.parent = InviHistoryContent.transform;

            if (ENB.history[i] == null)
            {
                tmp.transform.GetChild(4).GetComponent<Text>().text = "대전 기록이 존재하지 않습니다.";
                tmp.GetComponent<Image>().color = new Color(174 / 255f, 174 / 255f, 174 / 255f);
                break;
            }
            else
            {
                if (ENB.history[i].result.Equals("1")) tmp.GetComponent<Image>().color = new Color(255 / 255f, 215 / 255f, 0 / 255f);
                else if (ENB.history[i].result.Equals("2")) tmp.GetComponent<Image>().color = new Color(192 / 255f, 192 / 255f, 192 / 255f);
                else if (ENB.history[i].result.Equals("3")) tmp.GetComponent<Image>().color = new Color(164 / 255f, 124 / 255f, 109 / 255f);
                else tmp.GetComponent<Image>().color = new Color(123 / 255f, 123 / 255f, 123 / 255f);

                string time = ENB.history[i].matchDate.Replace("T", " ");
                time = time.Replace(".000Z", "");

                tmp.transform.GetChild(0).GetComponent<Text>().text = time;
                tmp.transform.GetChild(1).GetComponent<Text>().text = ENB.history[i].result + " 등";
                tmp.transform.GetChild(2).GetComponent<Text>().text = ENB.history[i].resultcar;
                tmp.transform.GetChild(3).GetComponent<Text>().text = ENB.history[i].resultgun;
            }
        }
    }
    public void U_InviStateList()
    {
        inviState.Clear();
        childDestroy(InviStateContent);
        StartCoroutine(InviStateList());
        Invoke("InviStateUpdate", 0.3f);
    }
    IEnumerator InviStateList()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.userinfo.id);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/invitationSelect", form);
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            inviState.Add(JsonUtility.FromJson<DBClass.inviinfo>(data[i]));
    }
    void InviStateUpdate()
    {
        for (int i = 0; i < inviState.Count; i++)
        {
            GameObject tmp = GameObject.Instantiate(InviStateInstance, InviStateContent.transform);
            tmp.transform.parent = InviStateContent.transform;

            if (inviState[i] == null)
            {
                tmp.transform.GetChild(1).GetComponent<Text>().text = "초대한 유저가 없습니다.";
                break;
            }
            else
            {
                string inviId = inviState[i].invitation;

                tmp.transform.GetChild(0).GetComponent<Text>().text = inviId;
                tmp.GetComponent<Button>().onClick.AddListener(delegate { InviStateUser(inviId, tmp); });
            }
        }
    }
    void InviStateUser(string id, GameObject obj)
    {
        childDestroy(InviHistoryContent);
        StartCoroutine(HistoryInsert(id));
        Invoke("InviHistoryUpdate", 0.3f);
    }
}
