using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class Loading : MonoBehaviour
{
    public static Loading instance;

    void Awake() => instance = this;

    [Header("Loading UI Component")]
    public Sprite img1;
    public Sprite img2;
    public Image background;

    void Start()
    {
        int number = Random.Range(1, 3);
        if (number == 1) background.sprite = img1;
        else background.sprite = img2;

        LoadingClaer();

        StartCoroutine(carInfoList());
        StartCoroutine(gunInfoList());
        StartCoroutine(trackInfoList());
        StartCoroutine(useCarList());
        StartCoroutine(useGunList());
        StartCoroutine(friendList());
        StartCoroutine(enbList());

        NRunner.instance.JoinLobby(NRunner.instance.runner);
    }

    void Update()
    {
        if (NRunner.instance.loadState != null)
        {
            NRunner.instance.loadState = null;
            SceneManager.LoadScene("Lobby");
        }
    }

    IEnumerator carInfoList()
    {
        var www = UnityWebRequest.Get(GameManager.serverURL + "/carInfoList");
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            ENB.carinfos.Add(JsonUtility.FromJson<DBClass.carinfo>(data[i]));
    }

    IEnumerator gunInfoList()
    {
        var www = UnityWebRequest.Get(GameManager.serverURL + "/gunInfoList");
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            ENB.guninfos.Add(JsonUtility.FromJson<DBClass.guninfo>(data[i]));
    }

    IEnumerator trackInfoList()
    {
        var www = UnityWebRequest.Get(GameManager.serverURL + "/trackInfoList");
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            ENB.tracks.Add(JsonUtility.FromJson<DBClass.track>(data[i]));
    }

    IEnumerator useCarList()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.userinfo.id);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/useCarList", form);
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            ENB.usecars.Add(JsonUtility.FromJson<DBClass.usecar>(data[i]));
    }

    IEnumerator useGunList()
    {
        var form = new WWWForm();
        form.AddField("id", ENB.userinfo.id);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/useGunList", form);
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            ENB.useguns.Add(JsonUtility.FromJson<DBClass.usegun>(data[i]));
    }

    IEnumerator friendList()
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
    }

    IEnumerator enbList()
    {
        ENB.dbEnbs.Clear();

        var www = UnityWebRequest.Get(GameManager.serverURL + "/enbList");
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            ENB.dbEnbs.Add(JsonUtility.FromJson<DBClass.dbEnb>(data[i]));
        for (int i = 0; i < ENB.dbEnbs.Count; i++)
        {
            switch (ENB.dbEnbs[i].ename)
            {
                case "maxAmmo": ENB.maxAmmo = float.Parse(ENB.dbEnbs[i].evalue); break;
                case "maxAtk": ENB.maxAtk = float.Parse(ENB.dbEnbs[i].evalue); break;
                case "maxDef": ENB.maxDef = float.Parse(ENB.dbEnbs[i].evalue); break;
                case "maxHp": ENB.maxHp = float.Parse(ENB.dbEnbs[i].evalue); break;
                case "maxRpm": ENB.maxRpm = float.Parse(ENB.dbEnbs[i].evalue); break;
                case "maxSpeed": ENB.maxSpeed = float.Parse(ENB.dbEnbs[i].evalue); break;

                case "itemAmmo": ENB.itemAmmo = float.Parse(ENB.dbEnbs[i].evalue); break;
                case "itemBerserker": ENB.itemBerserker = float.Parse(ENB.dbEnbs[i].evalue); break;
                case "itemBooster": ENB.itemBooster = float.Parse(ENB.dbEnbs[i].evalue); break;
                case "itemHpRestore": ENB.itemHpRestore = float.Parse(ENB.dbEnbs[i].evalue); break;
                case "itemShield": ENB.itemShield = float.Parse(ENB.dbEnbs[i].evalue); break;
            }
        }
    }

    void LoadingClaer()
    {
        ENB.nowSceneName = "Loading";
        ENB.userlist.Clear();
        ENB.usecars.Clear();
        ENB.useguns.Clear();
        ENB.inviinfos.Clear();
        ENB.friends.Clear();
        ENB.tracks.Clear();
        ENB.carinfos.Clear();
        ENB.guninfos.Clear();
        ENB.myList.Clear();
        ENB.dbEnbs.Clear();
    }
}
