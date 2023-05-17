using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using Fusion.Sockets;
using System;
using System.Threading.Tasks;

public class NRunner : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static NRunner instance = null;
    public NetworkRunner runner;

    [SerializeField] NetworkPrefabRef playerPrefab;

    public Dictionary<PlayerRef, NetworkObject> playerInfos = new Dictionary<PlayerRef, NetworkObject>();

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public StartGameResult loadState = null;

    public async Task JoinLobby(NetworkRunner runner)
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        loadState = await runner.JoinSessionLobby(SessionLobby.Shared, "Accro");
    }

    public async Task CreateRoom(NetworkRunner runner, string roomname, int mode)
    {
        var roomOpt = new Dictionary<string, SessionProperty>() {
            {"roomName", roomname}, {"mode", mode}, {"maxPlayer", 8}, {"State", "대기중"}
        };

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionProperties = roomOpt,
            CustomLobbyName = "Accro"
        });
    }

    public async Task CPURoom(NetworkRunner runner)
    {
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Single
        });
    }

    public async Task JoinRoom(NetworkRunner runner, string roomname)
    {
        var roomOpt = new Dictionary<string, SessionProperty>() {
            {"roomName", roomname}
        };

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionProperties = roomOpt,
            CustomLobbyName = "Accro"
        });
    }

    // 방 유효성 검사
    public SessionInfo RoomInfoRet(string roomname)
    {
        for(int i = 0; i < ENB.myList.Count; i++)
            if(ENB.myList[i].Properties["roomName"].PropertyValue.Equals(roomname)) return ENB.myList[i];

        return null;
    }

    // 네트워크 러너가 연결
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (ENB.nowSceneName.Equals("RoomPVP"))
        {
            if (RoomPVP.instance.joinCount == 1)
            {
                GameObject infoBox = GameObject.FindGameObjectWithTag("InfoBox");
                if (infoBox == null)
                    if (runner.IsSharedModeMasterClient)
                        RoomInfoBoxCreate(runner, player);

                GameObject roomObj = GameObject.FindGameObjectWithTag("RoomInfo");
                if (roomObj == null)
                    if (runner.IsSharedModeMasterClient) RoomInfoObjCreate(runner, player);

                RoomPVP.instance.joinCount = 0;

                PlayerInfoCreate(runner, player);
            }
        }
        if (ENB.nowSceneName.Equals("RoomCPU"))
        {
            if (RoomCPU.instance.joinCount == 1)
            {
                RoomCPU.instance.joinCount = 0;

                PlayerInfoCreate(runner, player);
                RoomInfoBoxCreate(runner, player);
                RoomInfoObjCreate(runner, player);
            }
        }

        if (ENB.nowSceneName.Equals("RoomPVP")) RoomPVP.instance.U_joinPlayer();
    }

    // 네트워크 러너가 연결 끊김
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (playerInfos.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            playerInfos.Remove(player);
        }

        if (!ENB.gamePlay) RoomPVP.instance.U_leftPlayer();
        else GameDirector.instance.IG_leftPlayer();
    }

    // 네트워크 러너의 사용자 입력을 잃음
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    // 네트워크 러너가 셧다운
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    // 네트워크 러너가 서버나 호스트에 연결 성공
    public void OnConnectedToServer(NetworkRunner runner) { }

    // 네트워크 러너가 서버나 호스트에서 연결 끊음
    public void OnDisconnectedFromServer(NetworkRunner runner) { }

    // 네트워크 러너가 리모트 클라이언트로부터 연결 요청 받음
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    // 네트워크 러너가 서버나 호스트에 연결 실패
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    // 리모트 클라이언트로부터 수동으로 전달된 메시지가 수신
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    // 세션 업데이트
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        int sessionCount = sessionList.Count;

        ENB.myList.Clear();

        for (int i = 0; i < sessionCount; i++) ENB.myList.Add(sessionList[i]);

        Lobby.instance.MyListRenewal();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    // 입력 부분
    public struct NetworkInputData : INetworkInput
    {
        public const uint ButtonAccelerate = 1 << 0;
        public const uint ButtonReverse = 1 << 1;
        public const uint ButtonLeft = 1 << 2;
        public const uint ButtonRight = 1 << 3;

        public const uint ButtonDrift = 1 << 4;
        public const uint ButtonShot = 1 << 5;
        public const uint UseItemOne = 1 << 6;
        public const uint UseItemTwo = 1 << 7;

        public const uint ButtonEnter = 1 << 8;
        public const uint ButtonReturn = 1 << 9;
        public const uint ButtonPause = 1 << 10;

        public uint Buttons;
        public uint OneShots;

        public bool IsUp(uint button) => IsDown(button) == false;
        public bool IsDown(uint button) => (Buttons & button) == button;

        public bool IsDownThisFrame(uint button) => (OneShots & button) == button;

        public bool IsAccelerate => IsDown(ButtonAccelerate);
        public bool IsReverse => IsDown(ButtonReverse);
        public bool IsLeft => IsDown(ButtonLeft);
        public bool IsRight => IsDown(ButtonRight);

        public bool IsDriftPressed => IsDown(ButtonDrift);
        public bool IsDriftPressedThisFrame => IsDownThisFrame(ButtonDrift);
        public bool IsShotPressed => IsDown(ButtonShot);
        public bool IsShotPressedThisFrame => IsDownThisFrame(ButtonShot);
    }

    [SerializeField] private InputAction accelerate;
    [SerializeField] private InputAction reverse;
    [SerializeField] private InputAction left;
    [SerializeField] private InputAction right;

    [SerializeField] private InputAction drift;
    [SerializeField] private InputAction shot;
    [SerializeField] private InputAction useItemone;
    [SerializeField] private InputAction useItemtwo;

    [SerializeField] private InputAction enter;
    [SerializeField] private InputAction btnreturn;
    [SerializeField] private InputAction pause;

    private bool _useItemOnePressed;
    private bool _useItemTwoPressed;
    private bool _driftPressed;
    private bool _shotPressed;

    private bool _enterPressed;
    private bool _btnReturnPressed;
    private bool _pausePressed;

    private void UseItemOnePressed(InputAction.CallbackContext ctx) => _useItemOnePressed = true;
    private void UseItemTwoPressed(InputAction.CallbackContext ctx) => _useItemTwoPressed = true;
    private void DriftPressed(InputAction.CallbackContext ctx) => _driftPressed = true;
    private void ShotPressed(InputAction.CallbackContext ctx) => _shotPressed = true;
    private void EnterPressed(InputAction.CallbackContext ctx) => _enterPressed = true;
    private void BtnReturnPressed(InputAction.CallbackContext ctx) => _btnReturnPressed = true;
    private void PausePressed(InputAction.CallbackContext ctx) => _pausePressed = true;

    private static bool ReadBool(InputAction action) => action.ReadValue<float>() != 0;

    // 네트워크 러너의 사용자 입력을 인식
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var userInput = new NetworkInputData();

        if (ReadBool(accelerate)) userInput.Buttons |= NetworkInputData.ButtonAccelerate;
        if (ReadBool(reverse)) userInput.Buttons |= NetworkInputData.ButtonReverse;
        if (ReadBool(left)) userInput.Buttons |= NetworkInputData.ButtonLeft;
        if (ReadBool(right)) userInput.Buttons |= NetworkInputData.ButtonRight;

        if (ReadBool(drift)) userInput.Buttons |= NetworkInputData.ButtonDrift;
        if (ReadBool(shot)) userInput.Buttons |= NetworkInputData.ButtonShot;

        if (_driftPressed) userInput.OneShots |= NetworkInputData.ButtonDrift;
        if (_shotPressed) userInput.OneShots |= NetworkInputData.ButtonShot;
        if (_useItemOnePressed) userInput.OneShots |= NetworkInputData.UseItemOne;
        if (_useItemTwoPressed) userInput.OneShots |= NetworkInputData.UseItemTwo;

        if (_enterPressed) userInput.OneShots |= NetworkInputData.ButtonEnter;
        if (_btnReturnPressed) userInput.OneShots |= NetworkInputData.ButtonReturn;
        if (_pausePressed) userInput.OneShots |= NetworkInputData.ButtonPause;


        input.Set(userInput);

        _driftPressed = false;
        _shotPressed = false;
        _useItemOnePressed = false;
        _useItemTwoPressed = false;
        _enterPressed = false;
        _btnReturnPressed = false;
        _pausePressed = false;
    }

    public override void Spawned()
    {
        base.Spawned();

        Runner.AddCallbacks(this);

        accelerate = accelerate.Clone();
        reverse = reverse.Clone();
        left = left.Clone();
        right = right.Clone();

        drift = drift.Clone();
        shot = shot.Clone();
        useItemone = useItemone.Clone();
        useItemtwo = useItemtwo.Clone();

        enter = enter.Clone();
        btnreturn = btnreturn.Clone();
        pause = pause.Clone();

        accelerate.Enable();
        reverse.Enable();
        left.Enable();
        right.Enable();

        drift.Enable();
        shot.Enable();
        useItemone.Enable();
        useItemtwo.Enable();

        enter.Enable();
        btnreturn.Enable();
        pause.Enable();

        drift.started += DriftPressed;
        shot.started += ShotPressed;
        useItemone.started += UseItemOnePressed;
        useItemtwo.started += UseItemTwoPressed;

        enter.started += EnterPressed;
        btnreturn.started += BtnReturnPressed;
        pause.started += PausePressed;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        DisposeInputs();
        Runner.RemoveCallbacks(this);
    }

    private void OnDestroy()
    {
        DisposeInputs();
    }

    private void DisposeInputs()
    {
        accelerate.Dispose();
        reverse.Dispose();
        left.Dispose();
        right.Dispose();

        drift.Dispose();
        shot.Dispose();
        useItemone.Dispose();
        useItemtwo.Dispose();

        enter.Dispose();
        btnreturn.Dispose();
        pause.Dispose();
    }

    // 네트워크 오브젝트 생성
    [SerializeField] NetworkPrefabRef InfoBox;
    public void RoomInfoBoxCreate(NetworkRunner runner, PlayerRef player)
    {
        NetworkObject roomInfoBox = runner.Spawn(InfoBox, gameObject.transform.position, Quaternion.identity, player);
    }
    [SerializeField] NetworkPrefabRef roomInfo;
    public void RoomInfoObjCreate(NetworkRunner runner, PlayerRef player)
    {
        NetworkObject roomInfoObj = runner.Spawn(roomInfo, gameObject.transform.position, Quaternion.identity, player);
        if (ENB.cpuPlay != true) RoomPVP.instance.roomInfoInit(roomInfoObj);
        else
        {
            roomInfoObj.gameObject.GetComponent<RoomInfo>().roomAdmin = ENB.id;
            roomInfoObj.gameObject.GetComponent<RoomInfo>().playerCount = 1;
        }
    }
    public void RoomPvpChangeAdmin(NetworkRunner runner, PlayerRef player)
    {
        NetworkObject obj = runner.Spawn(roomInfo, gameObject.transform.position, Quaternion.identity, ENB.myNO.GetComponent<PlayerInfo>().pid);
        RoomPVP.instance.roomInfoInit(obj);
        RoomPVP.instance.roomInfoSync(obj);
    }
    [SerializeField] NetworkPrefabRef playerInfo;
    public void PlayerInfoCreate(NetworkRunner runner, PlayerRef player)
    {
        if (!playerInfos.ContainsKey(player))
        {
            NetworkObject nPlayerInfo = runner.Spawn(playerInfo, gameObject.transform.position, Quaternion.identity, player);

            nPlayerInfo.GetComponent<PlayerInfo>().pid = player;
            nPlayerInfo.GetComponent<PlayerInfo>().id = ENB.gameinfo.id;
            nPlayerInfo.GetComponent<PlayerInfo>().roomNum = -1;
            nPlayerInfo.GetComponent<PlayerInfo>().car = ENB.gameinfo.usecar;
            nPlayerInfo.GetComponent<PlayerInfo>().gun = ENB.gameinfo.usegun;
            ENB.myNO = nPlayerInfo;
            ENB.pid = player;
            playerInfos.Add(player, nPlayerInfo);
        }
    }
}
