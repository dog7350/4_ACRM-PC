using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Friends : MonoBehaviour
{
    public static Friends instance;

    void Awake() => instance = this;

    [Header("Friend UI Component")]
    public InputField friendId;
    public Text Alarm;
    float aTime = 3;

    [Header("User Info Component")]
    public Text carText;
    public Text gunText;

    [Header("List Component")]
    public List<DBClass.gameinfo> friendlist = new List<DBClass.gameinfo>();
    public GameObject ListContent;
    public GameObject ListUser;

    [Header("History UI Component")]
    public Text HistId;
    public Text HistCar;
    public Text HistGun;
    public GameObject HistoryPanel;
    public GameObject HistoryContent;
    public GameObject HistoryList;
    public GameObject HistoryFriend;

    void Start()
    {
        ENB.nowSceneName = SceneManager.GetActiveScene().name;
        GameManager.instance.U_UserStatusUpdate();

        U_FriendListUpdate();

        carText.text = ENB.gameinfo.usecar;
        gunText.text = ENB.gameinfo.usegun;
    }

    void Update()
    {
        if (!Alarm.text.Equals(""))
        {
            aTime -= Time.deltaTime;
            if (aTime <= 0)
            {
                Alarm.text = "";
                aTime = 3;
            }
        }

        if (ENB.userListUpdateFlag == 1)
        {
            ENB.friends.Clear();
            Invoke("U_ListInsert", 0.2f);
            ENB.userListUpdateFlag = 0;
        }
    }
    void OnChatEnter(InputValue value)
    {
        if (LobbyChat.instance.ChatInput.isFocused == false) LobbyChat.instance.ChatInput.Select();
    }

    public void U_GarageClick() => SceneManager.LoadScene("Garage");

    public void U_ItemShopClick() => SceneManager.LoadScene("Shop");

    public void U_CashClick() => Application.OpenURL(GameManager.cashURL);

    public void U_LobbyClick() => SceneManager.LoadScene("Lobby");

    public void U_FriendAdd()
    {
        StartCoroutine(friendAdd());
        friendId.text = "";
        Alarm.text = "친구 추가되었습니다.";
        Invoke("U_ListInsert", 0.5f);
    }
    IEnumerator friendAdd()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.userinfo.id);
        form.AddField("fid", friendId.text);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/friendAdd", form);
        yield return www.SendWebRequest();
    }

    public void U_FriendSub()
    {
        StartCoroutine(friendSub());
        friendId.text = "";
        Alarm.text = "친구 삭제되었습니다.";
        Invoke("U_ListInsert", 0.5f);
    }
    IEnumerator friendSub()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.userinfo.id);
        form.AddField("fid", friendId.text);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/friendSub", form);
        yield return www.SendWebRequest();
    }

    public void U_ListInsert() => StartCoroutine(friendListInsert()); // 친구 리스트 삽입
    IEnumerator friendListInsert()
    {
        ENB.friends.Clear();

        var form = new WWWForm();
        form.AddField("id", ENB.userinfo.id);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/friendList", form);
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            ENB.friends.Add(JsonUtility.FromJson<DBClass.gameinfo>(data[i]));

        clearList();
        U_FriendListUpdate();
    }

    public void U_FriendListUpdate() // 친구 리스트 출력
    {
        clearList();
        GameObject tmp;
        Text t;
        for (int i = 0; i < ENB.friends.Count; i++)
        {
            tmp = GameObject.Instantiate(ListUser, ListContent.transform);
            tmp.transform.parent = ListContent.transform;

            if (ENB.friends[i] == null)
            {
                t = tmp.transform.GetChild(0).GetComponent<Text>();
                t.text = "친구 없음 ㅠㅠ";
                tmp.GetComponent<Button>().interactable = false;
                break;
            }
            else
            {
                if (ENB.friends[i].connect.Equals("o")) tmp.GetComponent<Image>().color = new Color(191 / 255f, 255 / 255f, 205 / 255f);
                else tmp.GetComponent<Image>().color = new Color(174 / 255f, 174 / 255f, 174 / 255f);

                t = tmp.transform.GetChild(2).GetComponent<Text>();
                t.text = ENB.friends[i].usegun;
                t = tmp.transform.GetChild(1).GetComponent<Text>();
                t.text = ENB.friends[i].usecar;

                string id = ENB.friends[i].id;
                t = tmp.transform.GetChild(0).GetComponent<Text>();
                t.text = id;

                tmp.GetComponent<Button>().onClick.AddListener(delegate { U_HistoryOpen(id); });
            }
        }
    }
    void clearList()
    {
        Transform[] childList = ListContent.GetComponentsInChildren<Transform>();

        if (childList != null) for (int i = 1; i < childList.Length; i++) if (childList[i] != transform) Destroy(childList[i].gameObject);
    }

    public void U_InfoOpen() => U_HistoryOpen(friendId.text);
    public void U_HistFriend() => StartCoroutine(HistfriendAdd());
    IEnumerator HistfriendAdd()
    {
        if (ENB.history[0] != null)
        {
            var form = new WWWForm();
            form.AddField("id", ENB.userinfo.id);
            form.AddField("fid", ENB.history[0].id);
            var www = UnityWebRequest.Post(GameManager.serverURL + "/friendAdd", form);
            yield return www.SendWebRequest();
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
        ENB.history.Clear();

        HistoryPanel.gameObject.SetActive(true);
        HistoryFriend.gameObject.SetActive(true);

        if(ENB.friends[0] != null) for (int i = 0; i < ENB.friends.Count; i++) if (ENB.friends[i].id.Equals(id)) HistoryFriend.gameObject.SetActive(false);

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
}
