using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Fusion;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public NetworkRunner runner;

    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.SetResolution(1280, 720, false);
        instance = this;
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
    }

    [Header("Server Info")]
    // 13.125.77.214
    public static string serverURL;
    public static string chatURL;
    public static string cashURL = serverURL + "/main?ips";
    public int middlePort = 8800;
    public int chatPort = 8888;

    public Text userStatus;

    void Start()
    {
        Application.targetFrameRate = 30;

        if (ENB.userinfo.id != null) ENB.id = ENB.userinfo.id;
    }

    void Update()
    {
        if (ENB.invitation == 1)
        {
            inviFlag = true;
            inviPanel.SetActive(true);
            U_InviStateList();

            ENB.invitation = 0;
        }

        if (inviFlag == true) Invoke("inviCountPrint", 0.3f);
    }

    void OnApplicationQuit()
    {
        if (!ENB.gamePlay)
        {
            U_InviReset();
            U_LRChange("x");
            U_UserDisconnected();
            LobbyChat.instance.ws.Send("/GameUserUpdate");
            LobbyChat.instance.ws.Close();
        }
        else
        {
            runner.Despawn(GameDirector.instance.myCar);
            runner.Despawn(GameDirector.instance.myGun);
            runner.Despawn(GameDirector.instance.myInfoDes);

            U_InviReset();
            U_LRChange("x");
            U_UserDisconnected();
            LobbyChat.instance.ws.Send("/GameUserUpdate");
            LobbyChat.instance.ws.Close();
        }

        if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    void OnApplicationPause(bool pause)
    {
        if (!ENB.gamePlay)
        {
            U_InviReset();
            U_LRChange("x");
            U_UserDisconnected();
            LobbyChat.instance.ws.Send("/GameUserUpdate");
            LobbyChat.instance.ws.Close();
        }
        else
        {
            runner.Despawn(GameDirector.instance.myCar);
            runner.Despawn(GameDirector.instance.myGun);
            runner.Despawn(GameDirector.instance.myInfoDes);

            U_InviReset();
            U_LRChange("x");
            U_UserDisconnected();
            LobbyChat.instance.ws.Send("/GameUserUpdate");
            LobbyChat.instance.ws.Close();
        }

        if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    [Header("Invitation UI Component")]
    public GameObject inviPanel;
    public GameObject inviInstance;
    public GameObject inviContent;
    List<DBClass.inviinfo> inviState = new List<DBClass.inviinfo>();
    bool inviFlag = false;
    string refusalid;

    /* Invitation */
    public void U_InviOpen() => inviPanel.SetActive(true);
    public void U_InviClose()
    {
        childDestroy(inviContent);
        inviPanel.SetActive(false);
    }
    public void U_InviReset()
    {
        StartCoroutine(InviReset());
        Invoke("resetSend", 0.3f);
    }
    IEnumerator InviReset()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.id);
        var www = UnityWebRequest.Post(serverURL + "/invitationReset", form);
        yield return www.SendWebRequest();
    }
    void resetSend()
    {
        for (int i = 0; i < inviState.Count; i++)
        {
            if (inviState[i] == null)
            {
                inviContent.transform.GetChild(i).transform.GetChild(5).gameObject.SetActive(true);
                inviFlag = false;
                break;
            }
            else LobbyChat.instance.ws.Send("/inviRoom " + inviContent.transform.GetChild(i).transform.GetChild(0).GetComponent<Text>().text);
        }
    }
    public void U_InviRefusal(string id)
    {
        refusalid = id;
        StartCoroutine(InviRefusal(id));
        Invoke("refusalSend", 0.3f);
    }
    IEnumerator InviRefusal(string id)
    {
        var form = new WWWForm();
        form.AddField("id", id);
        form.AddField("inviId", ENB.id);
        var www = UnityWebRequest.Post(serverURL + "/invitationRefusal", form);
        yield return www.SendWebRequest();
    }
    void refusalSend()
    {
        LobbyChat.instance.ws.Send("/inviRoom " + refusalid);
        refusalid = null;
    }
    public void U_InviStateList()
    {
        inviState.Clear();
        childDestroy(inviContent);
        StartCoroutine(InviStateList());
        Invoke("InviStateUpdate", 0.3f);
    }
    IEnumerator InviStateList()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.id);
        var www = UnityWebRequest.Post(serverURL + "/invitationMe", form);
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
            GameObject tmp = GameObject.Instantiate(inviInstance, inviContent.transform);
            tmp.transform.parent = inviContent.transform;

            if (inviState[i] == null)
            {
                tmp.transform.GetChild(5).gameObject.SetActive(true);
                inviFlag = false;
                break;
            }
            else
            {
                string id = inviState[i].id;
                string room = inviState[i].room;
                tmp.transform.GetChild(0).GetComponent<Text>().text = id;
                tmp.transform.GetChild(1).GetComponent<Text>().text = room;
                tmp.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { InviOk(room, id, tmp); });
                tmp.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(delegate { InviCancle (id, tmp); });
            }
        }
    }
    void inviCountPrint()
    {
        for (int i = 0; i < inviState.Count; i++)
        {
            string data = inviState[i].inviDate.Replace("T", " ");
            data = data.Replace(".000Z", "");
            string[] dataSplit = data.Split(' ');
            string[] date = dataSplit[0].Split('-');
            string[] time = dataSplit[1].Split(':');

            var db = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]), 0).ToLocalTime();
            var now = DateTime.Now.ToLocalTime();
            string dif = (now - db).ToString();
            dif = dif.Replace("00:", "");
            float second = float.Parse(dif);

            second *= 0.1f;
            if (second >= 1)
            {
                inviState.Remove(inviState[i]);
                Destroy(inviContent.transform.GetChild(i).gameObject);
                U_InviRefusal(inviContent.transform.GetChild(i).transform.GetChild(0).GetComponent<Text>().text);

                if (inviState.Count <= 0) inviFlag = false;
            }
            else inviContent.transform.GetChild(i).transform.GetChild(2).GetComponent<Image>().fillAmount = 1 - second;
        }
    }
    void InviOk(string name, string id, GameObject obj)
    {
        Destroy(obj);
        if (NRunner.instance.RoomInfoRet(name))
        {
            U_InviReset();
            NRunner.instance.JoinRoom(runner, name);
            U_InviRefusal(id);

            ENB.gameinfo.room = "o";
            U_LRChange("o");
            SceneManager.LoadScene("RoomPVP");
        }
        else
        {
            U_InviRefusal(id);
        }
    }
    void InviCancle(string id, GameObject obj)
    {
        Destroy(obj);
        U_InviRefusal(id);
    }
    void childDestroy(GameObject obj)
    {
        Transform[] childList = obj.GetComponentsInChildren<Transform>();

        if (childList != null) for (int i = 1; i < childList.Length; i++) if (childList[i] != transform) Destroy(childList[i].gameObject);
    }

    /* Connected */
    public void U_UserConnected(string id) => StartCoroutine(userConnected(id));
    IEnumerator userConnected(string id)
    {
        var form = new WWWForm();
        form.AddField("id", id);
        var www = UnityWebRequest.Post(serverURL + "/userConnected", form);
        yield return www.SendWebRequest();
    }
    public void U_UserDisconnected() => StartCoroutine(userDisconnected());
    IEnumerator userDisconnected()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.id);
        var www = UnityWebRequest.Post(serverURL + "/userDisconnected", form);
        yield return www.SendWebRequest();
    }
    public void U_LRChange(string loc) => StartCoroutine(LRChange(loc));
    IEnumerator LRChange(string loc)
    {
        string id = ENB.id;

        var form = new WWWForm();
        form.AddField("id", id);
        form.AddField("loc", loc);
        var www = UnityWebRequest.Post(serverURL + "/userLRChange", form);
        yield return www.SendWebRequest();
    }

    /* UserInfoMation */
    public void userInfoClear()
    {
        ENB.id = null;
        ENB.userinfo = null;
        ENB.gameinfo = null;
        ENB.userlist.Clear();
        ENB.usecars.Clear();
        ENB.useguns.Clear();
        ENB.inviinfos.Clear();
        ENB.friends.Clear();
        ENB.tracks.Clear();
        ENB.carinfos.Clear();
        ENB.guninfos.Clear();
        ENB.myList.Clear();
    }
    public void U_UserStatusUpdate() => userStatus.text = "ID : " + ENB.userinfo.id + "         " + "MONEY : " + ENB.userinfo.money + "         " + "CASH : " + ENB.userinfo.cash;
}