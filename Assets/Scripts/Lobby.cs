using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Fusion;

public class Lobby : MonoBehaviour
{
    public static Lobby instance = null;
    public NetworkRunner runner;

    public AudioClip music;

    private void Awake()
    {
        instance = this;
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
    }

    [Header("Menu List Component")]
    public Text carText;
    public Text gunText;
    public Button logout;
    public Button cash;
    public Button itemShop;
    public Button garage;

    [Header("Room List Component")]
    public GameObject roomContent;
    public GameObject roomInstance;
    public Button createIndivi;
    public Button createTeam;
    public Button createCPU;
    public Button joinRoom;
    string mode;

    [Header("Room Alarm Component")]
    public GameObject AlarmPanel;
    public InputField roomName;
    public Button roomCancle;
    public Text roomAlarm;

    [Header("Err Alarm Component")]
    public GameObject ErrPanel;
    public Text ErrAlarm;

    [Header("User List Component")]
    public Text UserCount;
    public GameObject ListContent;
    public GameObject ListUser;
    public Button friend;

    [Header("Chat List Component")]
    public InputField chatInput;

    [Header("History UI Component")]
    public Text HistId;
    public Text HistCar;
    public Text HistGun;
    public GameObject HistoryPanel;
    public GameObject HistoryContent;
    public GameObject HistoryList;

    void Start()
    {
        ENB.nowSceneName = SceneManager.GetActiveScene().name;

        GameManager.instance.U_UserStatusUpdate();

        if (ENB.roomGetOut == true)
        {
            ErrPanel.SetActive(true);
            ErrAlarm.text = "방에서 강퇴 당하셨습니다.";

            ENB.roomGetOut = false;
        }

        MyListRenewal();
        ENB.userListUpdateFlag = 1;

        carText.text = ENB.gameinfo.usecar;
        gunText.text = ENB.gameinfo.usegun;
    }

    void Update()
    {
        if (ENB.userListUpdateFlag == 1)
        {
            ENB.userlist.Clear();
            U_ListInsert();
            ENB.userListUpdateFlag -= 1;
        }
    }
    void OnChatEnter(InputValue value)
    {
        if (LobbyChat.instance.ChatInput.isFocused == false) LobbyChat.instance.ChatInput.Select();
    }

    /* Room List */
    public void MyListRenewal()
    {
        Transform[] childList = roomContent.GetComponentsInChildren<Transform>();

        if(childList != null) for (int i = 1; i < childList.Length; i++) if(childList[i] != transform) Destroy(childList[i].gameObject);

        for (int i = 0; i < ENB.myList.Count; i++)
        {
            GameObject tmp = GameObject.Instantiate(roomInstance, roomContent.transform);
            tmp.transform.parent = roomContent.transform;

            string name = ENB.myList[i].Properties["roomName"].PropertyValue.ToString();
            string state = ENB.myList[i].Properties["State"].PropertyValue.ToString();

            if (state.Equals("게임중") || state.Equals("로딩중")) tmp.GetComponent<Button>().interactable = false;

            tmp.transform.GetChild(3).GetComponent<Text>().text = ENB.myList[i].PlayerCount + " / " + ENB.myList[i].Properties["maxPlayer"].PropertyValue;
            tmp.transform.GetChild(2).GetComponent<Text>().text = state;
            tmp.transform.GetChild(1).GetComponent<Text>().text = ENB.myList[i].Properties["mode"].PropertyValue.ToString() == "1" ? "개인전" : "팀전";
            tmp.transform.GetChild(0).GetComponent<Text>().text = name;

            tmp.GetComponent<Button>().onClick.AddListener(delegate { U_RoomBtnClick(name); });
        }
    }
    public void U_RoomBtnClick(string name)
    {
        SessionInfo room = NRunner.instance.RoomInfoRet(name);

        if (room.PlayerCount >= int.Parse(room.Properties["maxPlayer"].PropertyValue.ToString()))
        {
            ErrPanel.SetActive(true);
            ErrAlarm.text = "방 인원이 최대입니다.";
        }
        else if (ENB.gameinfo.usegun.Equals("") || ENB.gameinfo.usecar.Equals(""))
        {
            ErrPanel.SetActive(true);
            ErrAlarm.text = "차와 총을 선택하신 후 입장하세요.";
        }
        else
        {
            GameManager.instance.U_InviReset();
            NRunner.instance.JoinRoom(runner, name);

            ENB.gameinfo.room = "o";
            GameManager.instance.U_LRChange("o");

            SceneManager.LoadScene("RoomPVP");
        }
    }

    /* User List */
    public void U_ListInsert() => StartCoroutine(userListInsert());
    IEnumerator userListInsert()
    {
        ENB.userlist.Clear();

        var www = UnityWebRequest.Get(GameManager.serverURL + "/lobbyUserList");
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            ENB.userlist.Add(JsonUtility.FromJson<DBClass.gameinfo>(data[i]));

        if (SceneManager.GetActiveScene().name.Equals("Lobby"))
        {
            clearList();
            U_UserListUpdate();
        }
    }
    void U_UserListUpdate()
    {
        clearList();

        GameObject tmp;
        Text t;
        for (int i = 0; i < ENB.userlist.Count; i++)
        {
            tmp = GameObject.Instantiate(ListUser, ListContent.transform);
            tmp.transform.parent = ListContent.transform;

            t = tmp.transform.GetChild(2).GetComponent<Text>();
            t.text = ENB.userlist[i].usegun;
            t = tmp.transform.GetChild(1).GetComponent<Text>();
            t.text = ENB.userlist[i].usecar;

            string id = ENB.userlist[i].id;
            t = tmp.transform.GetChild(0).GetComponent<Text>();
            t.text = id;

            tmp.GetComponent<Button>().onClick.AddListener(delegate { U_HistoryOpen(id); });
        }

        UserCount.text = "현재 접속 인원 : " + ENB.userlist.Count.ToString();
    }
    void clearList()
    {
        Transform[] childList = ListContent.GetComponentsInChildren<Transform>();

        if (childList != null) for (int i = 1; i < childList.Length; i++) if (childList[i] != transform) Destroy(childList[i].gameObject);
    }

    /* Menu Button */
    public void U_GarageClick() => SceneManager.LoadScene("Garage");
    public void U_ItemShopClick() => SceneManager.LoadScene("Shop");
    public void U_LogOut()
    {
        GameManager.instance.U_UserDisconnected();
        GameManager.instance.U_InviReset();
        LobbyChat.instance.ws.Send("/GameUserUpdate");
        GameManager.instance.userInfoClear();

        GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>().Shutdown();
        SceneManager.LoadScene("Login");
    }
    public void U_CashClick() => Application.OpenURL(GameManager.cashURL);
    public void U_FriendsClick() => SceneManager.LoadScene("Friends");

    // Room Create Menu
    public void U_GameIndiviCreate()
    {
        mode = "1";
        AlarmPanel.SetActive(true);
        roomName.Select();
    }
    public void U_GameTeamCreate()
    {
        mode = "2";
        AlarmPanel.SetActive(true);
        roomName.Select();
    }
    public void U_GameCPUCreate()
    {
        mode = "3";
        U_GameAlarmOk();
    }
    public void U_GameJoin()
    {
        mode = "Join";
        AlarmPanel.SetActive(true);
        roomName.Select();
    }

    // Alarm
    public void U_GameAlarmCancle()
    {
        roomAlarm.text = "";
        mode = "";
        roomName.text = "";
        AlarmPanel.SetActive(false);
    }
    public void U_GameAlarmOk()
    {
        bool roomFlag = true;

        if (mode.Equals("Join"))
        {
            if (ENB.gameinfo.usegun.Equals("") || ENB.gameinfo.usecar.Equals(""))
            {
                roomAlarm.text = "차와 총을 선택하신 후 입장하세요.";
            }
            else
            {
                SessionInfo room = NRunner.instance.RoomInfoRet(roomName.text);
                if (room)
                {
                    if (room.PlayerCount >= int.Parse(room.Properties["maxPlayer"].PropertyValue.ToString()))
                        roomAlarm.text = "방 인원이 최대입니다.";
                    else if (room.Properties["State"].PropertyValue.ToString().Equals("게임중"))
                        roomAlarm.text = "게임중입니다.";
                    else
                    {
                        GameManager.instance.U_InviReset();
                        NRunner.instance.JoinRoom(runner, roomName.text);
                        AlarmPanel.SetActive(false);

                        ENB.gameinfo.room = "o";
                        GameManager.instance.U_LRChange("o");
                        roomAlarm.text = "";
                        mode = "";
                        roomName.text = "";
                        SceneManager.LoadScene("RoomPVP");
                    }
                }
            }

            roomAlarm.text = "존재하지 않는 방입니다.";
        }
        else if (mode.Equals("3")) // CPU
        {
            if (ENB.gameinfo.usegun.Equals("") || ENB.gameinfo.usecar.Equals(""))
            {
                ErrPanel.SetActive(true);
                ErrAlarm.text = "차와 총을 선택하신 후 입장하세요.";
            }
            else
            {
                GameManager.instance.U_InviReset();
                NRunner.instance.CPURoom(runner);
                AlarmPanel.SetActive(false);

                ENB.gameinfo.room = "o";
                GameManager.instance.U_LRChange("o");
                roomAlarm.text = "";
                mode = "";
                roomName.text = "";
                SceneManager.LoadScene("RoomCPU");
            }
        }
        else
        {
            if (roomName.text.Equals(""))
            {
                roomAlarm.text = "방 제목을 입력하세요.";
                roomFlag = false;
            }
            else if (roomName.text.Contains(" "))
            {
                roomAlarm.text = "공백을 제거해주세요.";
                roomFlag = false;
            }
            else
            {
                if (NRunner.instance.RoomInfoRet(roomName.text))
                {
                    roomAlarm.text = "이미 존재하는 방 제목입니다.";
                    roomFlag = false;
                }
            }

            if (roomFlag == true)
            {
                if (ENB.gameinfo.usegun.Equals("") || ENB.gameinfo.usecar.Equals(""))
                {
                    ErrPanel.SetActive(true);
                    ErrAlarm.text = "차와 총을 선택하신 후 입장하세요.";
                }
                else
                {
                    GameManager.instance.U_InviReset();
                    NRunner.instance.CreateRoom(runner, roomName.text, int.Parse(mode));
                    AlarmPanel.SetActive(false);

                    ENB.gameinfo.room = "o";
                    GameManager.instance.U_LRChange("o");
                    roomAlarm.text = "";
                    mode = "";
                    roomName.text = "";
                    SceneManager.LoadScene("RoomPVP");
                }
            }
        }
    }
    public void U_ErrBtn()
    {
        ErrAlarm.text = "";
        ErrPanel.gameObject.SetActive(false);
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

            ErrPanel.SetActive(true);
            ErrAlarm.text = "추가되었습니다.";
        }
    }
    public void U_HistEnd()
    {
        HistId.text = "";
        HistCar.text = "";
        HistGun.text = "";
        HistoryPanel.gameObject.SetActive(false);
    }
    public void U_HistoryOpen(string id)
    {
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
        float y = 0;
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

            y += 62f;
        }
    }
}
