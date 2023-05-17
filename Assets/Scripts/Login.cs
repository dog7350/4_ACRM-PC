using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.InputSystem;

public class Login : MonoBehaviour
{
    public static Login instance = null;

    public GameObject BGM;
    public AudioClip music;
    EventSystem system;

    private void Awake()
    {
        instance = this;
    }

    [Header("Login UI Component")]
    public Text Alarm;
    public GameObject ErrPanel;
    public Selectable ID;
    public Selectable PW;
    Selectable next;
    public Button LoginBtn;
    public Button Join;
    public Button Research;
    public InputField serverip;

    void Start()
    {
        ENB.nowSceneName = SceneManager.GetActiveScene().name;
        // Mobile
        SettingRead();
        system = EventSystem.current;
        ENB.loginFlag = 1;
        ID.Select();
    }

    void OnTabAction(InputValue value)
    {
        int num = int.Parse(value.Get().ToString());
        if (num == 1) // Tab
        {
            next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null) next.Select();
        }
        else if (num == 0) // Shift Tab
        {
            next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
            if (next != null) next.Select();
        }
    }
    void OnEnterAction(InputValue value) => LoginBtn.onClick.Invoke();

    public void U_AccountPage() => Application.OpenURL(GameManager.serverURL + "/main?rps");

    public void U_StartLogin() => StartCoroutine(LoginCheck());

    IEnumerator LoginCheck()
    {
        // Mobile
        // GameManager.chatURL = "13.125.77.214";
        // GameManager.serverURL = "http://13.125.77.214:" + GameManager.instance.middlePort;
        GameManager.instance.userInfoClear();

        var form = new WWWForm();
        form.AddField("id", ID.GetComponent<TMP_InputField>().text);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/loginchack", form);
        yield return www.SendWebRequest();
        
        string str = www.downloadHandler.text.Replace("[", "");
        var data = str.Split(']');
        data[1] = data[1].Substring(1);

        ENB.userinfo = JsonUtility.FromJson<DBClass.userinfo>(data[0]);
        ENB.gameinfo = JsonUtility.FromJson<DBClass.gameinfo>(data[1]);

        Invoke("gameLogin", 0.05f);
    }

    void gameLogin()
    {
        if (ENB.userinfo == null) Alarm.text = "ID를 확인하세요.";
        else if (!PW.GetComponent<TMP_InputField>().text.Equals(ENB.userinfo.pw)) Alarm.text = "PW를 확인하세요.";
        else if (ENB.gameinfo.connect.Equals("o")) ErrPanel.SetActive(true);
        else
        {
            Alarm.text = "";
            GameManager.instance.U_UserConnected(ENB.userinfo.id);
            SceneManager.LoadScene("Loading");
            return;
        }
        ENB.userinfo = null;
        ENB.gameinfo = null;
    }
    public void ErrClose()
    {
        ErrPanel.SetActive(false);
        ID.GetComponent<TMP_InputField>().text = "";
        PW.GetComponent<TMP_InputField>().text = "";
    }
    public void ErrOk()
    {
        // Mobile
        // GameManager.chatURL = "13.125.77.214";

        ErrPanel.SetActive(false);
        ENB.id = ID.GetComponent<TMP_InputField>().text;
        LobbyChat.instance.ws.Send("/DuplicationLogin " + ENB.id);
        GameManager.instance.U_UserDisconnected();
        GameManager.instance.U_InviReset();
        GameManager.instance.U_LRChange("x");
        Invoke("U_StartLogin", 2f);
    }

    public void U_ReSearchPage() => Application.OpenURL(GameManager.serverURL + "/main?fid");

    void SettingRead()
    {
        string f = @"init.txt";

        if (File.Exists(f) == false)
        {
            StreamWriter sw = new StreamWriter(f);
            sw.WriteLine("serverip:13.125.77.214");
            sw.Flush();
            sw.Close();
        }

        string[] text = File.ReadAllLines(f);
        string[] str;
        if (text.Length > 0)
        {
            for (int i = 0; i < text.Length; i++)
            {
                str = text[i].Split(':');

                if (str[0].Equals("serverip"))
                {
                    GameManager.chatURL = str[1];
                    GameManager.serverURL = "http://" + str[1] + ":" + GameManager.instance.middlePort;
                }
            }
        }
    }
}
