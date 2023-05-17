using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ListSelection : MonoBehaviour
{
    public static ListSelection instance;

    void Awake() => instance = this;

    public List<DBClass.gameinfo> userlist = new List<DBClass.gameinfo>();

    public void U_ListUpdate() => StartCoroutine(userListUpdate());

    IEnumerator userListUpdate()
    {
        var www = UnityWebRequest.Get(GameManager.serverURL + "/lobbyUserList");
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace("},{", "}/{");
        var data = str.Split('/');

        for (int i = 0; i < data.Length; i++)
            userlist.Add(JsonUtility.FromJson<DBClass.gameinfo>(data[i]));
    }
}
