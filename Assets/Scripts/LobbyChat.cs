using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class LobbyChat : MonoBehaviour
{
    public static LobbyChat instance;

    void Awake() => instance = this;

    public WebSocket ws = null;

    [Header("Chat UI Component")]
    public InputField ChatInput;
    public RectTransform Content;
    public Text LobbyChatText = null;
    public Text RoomChatText = null;
    public ScrollRect ChatScrollRect;

    const float chatTime = 0.05f;
    float updateTime = chatTime;

    int wi;
    string wid;
    string cmd;
    string wcontent;
    bool whispering = false;

    void Start()
    {
        if (ws == null)
        {
            ws = new WebSocket("ws://" + GameManager.chatURL + ":" + GameManager.instance.chatPort);
            ws.Connect();
        }

        if (!ENB.nowSceneName.Equals("Login"))
        {
            if (LobbyChatText != null)
            {
                if (ENB.nowSceneName.Equals("Lobby") && ENB.loginFlag == 1)
                {
                    LobbyChatText.text += "[게임 로비]에 입장하셨습니다.";
                    ws.Send("/GameUserUpdate");
                    ENB.loginFlag = 0;
                }
                else if (ENB.nowSceneName.Equals("Friends")) LobbyChatText.text += "[친구 관리]에 입장하셨습니다.";
                else if (ENB.nowSceneName.Equals("Shop")) LobbyChatText.text += "[아이텝 샵]에 입장하셨습니다.";
            }
            else if (RoomChatText != null)
            {
                if (ENB.gamePlay == false)
                {
                    if (ENB.nowSceneName.Equals("Lobby") && ENB.loginFlag == 1)
                    {
                        RoomChatText.text += "[게임 로비]에 입장하셨습니다.";
                        ws.Send("/GameUserUpdate");
                        ENB.loginFlag = 0;
                    }
                    else if (ENB.nowSceneName.Equals("Friends")) RoomChatText.text += "[친구 관리]에 입장하셨습니다.";
                    else if (ENB.nowSceneName.Equals("Shop")) RoomChatText.text += "[아이텝 샵]에 입장하셨습니다.";
                }
            }
            U_RectRefresh();

            ws.OnMessage += Recv;
        }
    }

    void Update()
    {
        if (!ENB.nowSceneName.Equals("Login"))
        {
            updateTime -= Time.deltaTime;
            if (updateTime <= 0)
            {
                updateTime = chatTime;
                U_RectRefresh();
            }
        }
    }

    public void U_OnSendButton(InputField ChatInput)
    {
        // Mobile
        if (!Input.GetButtonDown("Submit")) return;
        ChatInput.ActivateInputField();
        if (ChatInput.text.Trim() == "")
        {
            if (ENB.gamePlay == false) return;
            else
            {
                GameDirector.instance.chatInputFlag = false;
                GameDirector.instance.ChatInput.SetActive(false);
                return;
            }
        }

        string roomTitle = "";
        if (ChatInput.text.StartsWith("/w")) // 귓속말
        {
            cmd = ChatInput.text.Substring(0, 3);
            wid = ChatInput.text.Substring(3);
            for (wi = 0; wi < wid.Length; wi++) if (wid[wi] == ' ') break;
            wid = ChatInput.text.Substring(3, wi);
            U_Whispering(wid);

            wcontent = ChatInput.text;
            ChatInput.text = "";
            Invoke("WhisperingSelect", 0.3f);
        }
        else if(ENB.gameinfo.room.Equals("o")) // 방 내부 채팅
        {
            roomTitle = "";
            if (!RoomPVP.instance.roomTitle.text.Equals("")) roomTitle = RoomPVP.instance.roomTitle.text;

            string msg = "/room " + roomTitle + " [" + ENB.userinfo.id + "] : " + ChatInput.text;
            ChatInput.text = "";
            ws.Send(msg);
        }
        else
        {
            string msg = "/lobby [" + ENB.id + "] : " + ChatInput.text;
            ChatInput.text = "";
            ws.Send(msg);
        }
    }

    void Recv(object sender, MessageEventArgs e)
    {
        string str = e.Data;
        string roomTitle = "";

        if (str.Equals("/GameUserUpdate")) ENB.userListUpdateFlag = 1;
        else if (str.StartsWith("/DuplicationLogin "))
        {
            str = str.Replace("/DuplicationLogin ", "");
            if (str.Equals(ENB.userinfo.id)) System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        else if (str.StartsWith("/inviRoom "))
        {
            str = str.Replace("/inviRoom ", "");
            if (str.Equals(ENB.id)) ENB.inviState = 1;
        }
        else if (str.StartsWith("/Invitation"))
        {
            str = str.Replace("/Invitation ", "");

            if (str.Equals(ENB.id)) ENB.invitation = 1;
        }
        else if (str.StartsWith("/w")) // 귓속말
        {
            int i;
            string id = str.Substring(3);
            for (i = 0; i < id.Length; i++) if (id[i] == ' ') break;
            id = str.Substring(3, i);

            string content = str.Substring(i + 4);

            if (ENB.id.Equals(id)) U_ShowMessage(content);
            if (ENB.id.Equals(id)) U_ShowRoomMessage(content, "");
        }
        else if (str.StartsWith("/room"))
        {
            int i;
            roomTitle = str.Substring(6);
            for (i = 0; i < roomTitle.Length; i++) if (roomTitle[i] == ' ') break;
            roomTitle = str.Substring(6, i);

            string content = str.Substring(i + 7);

            if (roomTitle.Equals(RoomPVP.instance.roomTitle.text)) U_ShowRoomMessage(content, roomTitle);
            //else if (roomTitle.Equals(RoomCPU.instance.roomTitle.text)) U_ShowRoomMessage(content);
        }
        else if (str.StartsWith("/lobby"))
        {
            string content = str.Substring(7);
            U_ShowMessage(content);
        }
    }

    void U_ShowMessage(string data)
    {
        if (LobbyChatText != null)
            LobbyChatText.text += LobbyChatText.text == "" ? data : "\n" + data;
    }
    void U_ShowRoomMessage(string data, string title)
    {
        if (ENB.gamePlay == false)
        {
            if (RoomChatText != null)
                RoomChatText.text += RoomChatText.text == "" ? data : "\n" + data;
        }
        else
        {
            GameDirector.instance.chatTime = 5;
            if (RoomChatText != null)
                RoomChatText.text += RoomChatText.text == " " ? "시스템 메시지 : [" + title + "] 방입니다.\n" + data : "\n" + data;
        }
    }
    void U_RectRefresh()
    {
        if (LobbyChatText != null) U_Fit(LobbyChatText.GetComponent<RectTransform>());
        if (RoomChatText != null) U_Fit(RoomChatText.GetComponent<RectTransform>());
        U_Fit(Content);

        Invoke("U_ScrollDelay", 0.03f);
    }
    void U_Fit(RectTransform Rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
    void U_ScrollDelay()
    {
        if (!ENB.gamePlay) ChatScrollRect.verticalScrollbar.value = 0;
    }

    void  U_Whispering(string id) => StartCoroutine(Whispering(id));
    IEnumerator Whispering(string id)
    {
        var form = new WWWForm();
        form.AddField("id", id);
        var www = UnityWebRequest.Post(GameManager.serverURL + "/whisperingConnect", form);
        yield return www.SendWebRequest();

        string str = www.downloadHandler.text;

        str = str.Replace("}]", "");
        str = str.Replace("[{", "");
        str = str.Replace("\"", "");
        str = str.Replace("connect", "");
        str = str.Replace(":", "");

        if (str.Equals("o")) whispering = true;
        else whispering = false;
    }
    void WhisperingSelect()
    {
        if (ENB.id.Equals(wid)) U_ShowMessage("자신에게는 귓속말을 할 수 없습니다.");
        else if (!whispering)
        {
            U_ShowMessage("상대방이 없거나 접속중이지 않습니다.");
            U_ShowRoomMessage("상대방이 없거나 접속중이지 않습니다.", "");
        }
        else
        {
            string content = " " + ENB.userinfo.id + " 님의 귓속말 >>> " + wcontent.Substring(wi + 4);
            ChatInput.text = "";
            ws.Send(cmd + wid + content);
        }
    }
}
