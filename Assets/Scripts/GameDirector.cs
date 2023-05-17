using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Fusion;

public class GameDirector : NetworkBehaviour
{
    public static GameDirector instance = null;
    public NetworkRunner runner;

    public AudioClip music;

    private void Awake()
    {
        instance = this;
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
    }

    Color redTeam = new Color(255 / 255f, 165 / 255f, 165 / 255f);
    Color blueTeam = new Color(165 / 255f, 165 / 255f, 255 / 255f);

    [Header("Chatting")]
    public GameObject ScrollView;
    public GameObject ChatInput;
    public bool chatInputFlag;
    public float chatTime;

    [SerializeField] NetworkPrefabRef roomInfo;
    [SerializeField] NetworkPrefabRef Lampadati; // 스포츠카
    [SerializeField] NetworkPrefabRef Benefacter; // 밴
    [SerializeField] NetworkPrefabRef Alvani; // 승용차
    [SerializeField] NetworkPrefabRef Pegasi; // 슈퍼카
    [SerializeField] NetworkPrefabRef AILampadati; // 스포츠카 AI
    [SerializeField] NetworkPrefabRef AIBenefacter; // 밴 AI
    [SerializeField] NetworkPrefabRef AIAlvani; // 승용차 AI
    [SerializeField] NetworkPrefabRef AIPegasi; // 슈퍼카 AI

    [SerializeField] NetworkPrefabRef Howitzer; // 곡사포
    [SerializeField] NetworkPrefabRef Vulcan; // 발칸
    [SerializeField] NetworkPrefabRef Shoker; // 쇼커
    [SerializeField] NetworkPrefabRef Canon; // 캐논
    [SerializeField] NetworkPrefabRef AIHowitzer; // 곡사포 AI
    [SerializeField] NetworkPrefabRef AIVulcan; // 발칸 AI
    [SerializeField] NetworkPrefabRef AIShoker; // 쇼커 AI
    [SerializeField] NetworkPrefabRef AICanon; // 캐논 AI

    [Header("DefaultData")]
    public Camera MainCamera;
    public GameObject RoomInfoObj;
    int nowPlayer;
    int adminChange;
    public GameObject mainLight;
    public GameObject mainCamera;
    public GameObject MiniMapCamera;
    public GameObject infoBox;
    public GameObject SpawnBox;
    public GameObject PlayerObjList;

    public GameObject ExitPanel;
    public GameObject BrokenPanel;
    public GameObject LoadingPanel;
    public int loadPlayer = 0;
    public bool loadComplate = false;

    [Header("PlayerInfo")]
    GameObject[] PlayerCarList;
    GameObject[] PlayerGunList;
    public NetworkObject myCar;
    public NetworkObject myGun;
    public NetworkObject myInfoDes;
    public GameObject myInfoObj;

    [Header("CPU Room Data")]
    public NetworkObject CpuPlayerCar;
    public NetworkObject CpuPlayerGun;
    public NetworkObject CpuAiCar;
    public NetworkObject CpuAiGun;

    void Start()
    {
        if (ENB.cpuPlay != true) PvpStart();
        else CpuStart();
    }
    void Update() => PvpUpdate();
    void PvpStart()
    {
        // 방 설정
        RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");

        // 플레이어 생성
        infoBox = GameObject.FindGameObjectWithTag("InfoBox");
        Transform[] infoList = infoBox.GetComponentsInChildren<Transform>();
        for (int i = 1; i < infoList.Length; i++)
        {
            if (infoList[i].GetComponent<PlayerInfo>().id.Equals(ENB.id))
            {
                myInfoDes = infoList[i].gameObject.GetComponent<NetworkObject>();
                myInfoObj = infoList[i].gameObject;
                CarSpawn(myInfoObj.GetComponent<PlayerInfo>().car, myInfoObj.GetComponent<PlayerInfo>().roomNum);
                MiniMapCamera.GetComponent<SmoothFollow>().target = myCar.gameObject.transform;
                GunSpawn(myInfoObj.GetComponent<PlayerInfo>().gun, myInfoObj.GetComponent<PlayerInfo>().roomNum);
            }
        }

        int mapTime = RoomInfoObj.GetComponent<RoomInfo>().mapTime;
        if (mapTime == 1) mainLight.transform.rotation = Quaternion.Euler(25, 0, 0);
        else if (mapTime == 2) mainLight.transform.rotation = Quaternion.Euler(45, 0, 0);
        else if (mapTime == 3) mainLight.transform.rotation = Quaternion.Euler(90, 0, 0);
        else if (mapTime == 4) mainLight.transform.rotation = Quaternion.Euler(135, 0, 0);
        else if (mapTime == 5) mainLight.transform.rotation = Quaternion.Euler(155, 0, 0);
    }
    void PvpUpdate()
    {
        if (loadComplate == false)
        {
            Invoke("loadingPlayer", 0.3f);
        }

        if (RoomInfoObj == null) RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");

        if (RoomInfoObj.GetComponent<RoomInfo>().ingameExitAdmin == true) Destroy(RoomInfoObj);

        if (chatTime >= 0.01)
        {
            chatTime -= Time.deltaTime;
            ScrollView.SetActive(true);
        }
        else
        {
            if (chatInputFlag == false)
            {
                chatTime = 0;
                ScrollView.SetActive(false);
            }
            else chatTime = 5;
        }
    }
    void CpuStart()
    {
        RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");
        infoBox = GameObject.FindGameObjectWithTag("InfoBox");
        myInfoObj = GameObject.FindGameObjectWithTag("PlayerInfo");
        myInfoDes = myInfoObj.gameObject.GetComponent<NetworkObject>();

        int mapTime = Random.Range(1, 6);
        if (mapTime == 1) mainLight.transform.rotation = Quaternion.Euler(25, 0, 0);
        else if (mapTime == 2) mainLight.transform.rotation = Quaternion.Euler(45, 0, 0);
        else if (mapTime == 3) mainLight.transform.rotation = Quaternion.Euler(90, 0, 0);
        else if (mapTime == 4) mainLight.transform.rotation = Quaternion.Euler(135, 0, 0);
        else if (mapTime == 5) mainLight.transform.rotation = Quaternion.Euler(155, 0, 0);

        // 플레이어 생성
        CpuCarSpawn(ENB.gameinfo.usecar, 0);
        MiniMapCamera.GetComponent<SmoothFollow>().target = CpuPlayerCar.gameObject.transform;
        CpuGunSpawn(CpuPlayerCar, ENB.gameinfo.usegun, 0);

        // CPU 생성
        CpuCarSpawn(RoomInfoObj.GetComponent<RoomInfo>().CpuCar, 1);
        CpuGunSpawn(CpuAiCar, RoomInfoObj.GetComponent<RoomInfo>().CpuGun, 1);

        LoadingPanel.SetActive(false);
        CpuSetCarGunObject();
        PlayerCarId();
        loadComplate = true;
    }

    // Effect
    public void BoostEffect()
    {
        if (ENB.cpuPlay != true)
        {
            for (int i = 0; i < PlayerCarList.Length; i++)
            {
                GameObject tmp = PlayerCarList[i];
                Transform[] spList = tmp.GetComponentsInChildren<Transform>();

                for (int j = 10; j < spList.Length; j++)
                {
                    if (spList[j].name.Equals("Boost"))
                    {
                        if (tmp.GetComponent<GamePlayerInfo>().Boost == true)
                        {
                            spList[j].GetChild(0).gameObject.SetActive(true);
                            break;
                        }
                        else
                        {
                            spList[j].GetChild(0).gameObject.SetActive(false);
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            Transform[] spList = CpuPlayerCar.GetComponentsInChildren<Transform>();
            for (int j = 10; j < spList.Length; j++)
            {
                if (spList[j].name.Equals("Boost"))
                {
                    if (CpuPlayerCar.GetComponent<GamePlayerInfo>().Boost == true)
                    {
                        spList[j].GetChild(0).gameObject.SetActive(true);
                        break;
                    }
                    else
                    {
                        spList[j].GetChild(0).gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }
    }
    public void ExplosionEffect()
    {
        if (ENB.cpuPlay != true)
        {
            for (int i = 0; i < PlayerCarList.Length; i++)
            {
                GameObject tmp = PlayerCarList[i];
                Transform[] spList = tmp.GetComponentsInChildren<Transform>();

                for (int j = 14; j < spList.Length; j++)
                {
                    if (spList[j].name.Equals("Explosion"))
                    {
                        if (tmp.GetComponent<GamePlayerInfo>().Explosion == true)
                        {
                            spList[j].GetChild(0).gameObject.SetActive(true);
                            break;
                        }
                        else
                        {
                            spList[j].GetChild(0).gameObject.SetActive(false);
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            Transform[] spList = CpuPlayerCar.GetComponentsInChildren<Transform>();
            for (int j = 10; j < spList.Length; j++)
            {
                if (spList[j].name.Equals("Explosion"))
                {
                    if (CpuPlayerCar.GetComponent<GamePlayerInfo>().Boost == true)
                    {
                        spList[j].GetChild(0).gameObject.SetActive(true);
                        break;
                    }
                    else
                    {
                        spList[j].GetChild(0).gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }
    }

    // PVP
    void loadingPlayer()
    {
        int playerCount;
        playerCount = GameObject.FindGameObjectsWithTag("PlayerCar").Length;
        if (playerCount >= RoomInfoObj.GetComponent<RoomInfo>().playerCount)
        {
            SetPlayerObject(playerCount);
            Invoke("PlayerCarId", 0.3f);
            LoadingPanel.SetActive(false);
            loadComplate = true;
        }
    }

    void CarSpawn(string name, int spNum)
    {
        GameObject SpawnPoint;
        Transform[] spList = SpawnBox.GetComponentsInChildren<Transform>();
        SpawnPoint = spList[spNum + 1].gameObject;

        if (name.Equals("람파다티 카스코"))
            myCar = runner.Spawn(Lampadati, SpawnPoint.transform.position, SpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
        else if (name.Equals("베네팩터 덥스타"))
            myCar = runner.Spawn(Benefacter, SpawnPoint.transform.position, SpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
        else if (name.Equals("알바니 프리모"))
            myCar = runner.Spawn(Alvani, SpawnPoint.transform.position, SpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
        else if (name.Equals("페가시 젠토르노"))
            myCar = runner.Spawn(Pegasi, SpawnPoint.transform.position, SpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);

        myCar.gameObject.GetComponent<GamePlayerInfo>().createId = ENB.id;
        myCar.gameObject.GetComponent<GamePlayerInfo>().spNum = spNum + 1;

        MainCamera.GetComponent<SmoothFollow>().target = myCar.transform;
    }
    void GunSpawn(string name, int spNum)
    {
        GameObject gunSpawnPoint = null;
        Transform[] spList = myCar.GetComponentsInChildren<Transform>();
        for (int i = 0; i < spList.Length; i++) if (spList[i].name.Equals("GunSpawn"))
            {
                gunSpawnPoint = spList[i].gameObject;
                break;
            }

        if (name.Equals("곡사포"))
            myGun = runner.Spawn(Howitzer, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
        else if (name.Equals("발칸"))
            myGun = runner.Spawn(Vulcan, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
        else if (name.Equals("쇼커"))
            myGun = runner.Spawn(Shoker, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
        else if (name.Equals("캐논"))
            myGun = runner.Spawn(Canon, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);

        myGun.gameObject.GetComponent<GamePlayerInfo>().createId = ENB.id;
        myGun.gameObject.GetComponent<GamePlayerInfo>().spNum = spNum + 1;
    }

    void SetPlayerObject(int count)
    {
        string carCreateId;
        string gunCreateId;
        PlayerCarList = GameObject.FindGameObjectsWithTag("PlayerCar");
        PlayerGunList = GameObject.FindGameObjectsWithTag("PlayerGun");
        Transform[] spList = SpawnBox.GetComponentsInChildren<Transform>();

        for (int i = 0; i < count; i++)
        {
            carCreateId = PlayerCarList[i].GetComponent<GamePlayerInfo>().createId;

            for (int j = 0; j < count; j++)
            {
                gunCreateId = PlayerGunList[j].GetComponent<GamePlayerInfo>().createId;

                if (carCreateId.Equals(gunCreateId))
                {
                    Transform[] CarChildList = PlayerCarList[i].GetComponentsInChildren<Transform>();
                    GameObject gunSpawnPoint = null;
                    for (int k = 0; k < CarChildList.Length; k++) if (CarChildList[k].name.Equals("GunSpawn"))
                        {
                            gunSpawnPoint = CarChildList[k].gameObject;
                            break;
                        }

                    PlayerCarList[i].transform.parent = PlayerObjList.transform;
                    PlayerGunList[j].transform.localScale = new Vector3(1, 1, 1);
                    PlayerGunList[j].transform.position = gunSpawnPoint.transform.position;
                    PlayerGunList[j].transform.parent = gunSpawnPoint.transform;
                }
            }
        }
    }
    void PlayerCarId()
    {
        infoBox = GameObject.FindGameObjectWithTag("InfoBox");
        Transform[] infoList = infoBox.GetComponentsInChildren<Transform>();
        PlayerCarList = GameObject.FindGameObjectsWithTag("PlayerCar");

        for (int i = 0; i < PlayerCarList.Length; i++)
        {
            string id = PlayerCarList[i].GetComponent<GamePlayerInfo>().createId;
            string team = "";

            for (int j = 1; j < infoList.Length; j++)
            {
                if (infoList[j].GetComponent<PlayerInfo>().id.Equals(id))
                {
                    team = infoList[j].GetComponent<PlayerInfo>().team;
                    break;
                }
            }
            GameObject myCarID = PlayerCarList[i].GetComponentsInChildren<Transform>()[6].gameObject;
            myCarID.GetComponent<TextMesh>().text = id;

            if (ENB.cpuPlay != true)
            {
                if (team.Equals("r")) myCarID.GetComponent<TextMesh>().color = redTeam;
                else if (team.Equals("b")) myCarID.GetComponent<TextMesh>().color = blueTeam;
                else myCarID.GetComponent<TextMesh>().color = Color.white;
            }
            else
            {
                myCarID.GetComponent<TextMesh>().color = new Color(200 / 255f, 200 / 255f, 0 / 255f);
            }
        }
    }

    public void enableSuspension()
    {
        int WC;
        Transform[] tmp = myCar.GetComponentsInChildren<Transform>();
        Transform[] carTmp = tmp[1].GetComponentsInChildren<Transform>();
        for (WC = 0; WC < carTmp.Length; WC++)
            if (carTmp[WC].name.Equals("WC"))
                break;
        WC++;

        JointSpring js = new JointSpring();
        js.spring = 350;
        js.damper = 4500;
        js.targetPosition = 0.5f;
        for (int i = 1; i < carTmp.Length; i++)
        {
            carTmp[i].gameObject.GetComponent<WheelCollider>().suspensionDistance = 0.3f;
            carTmp[i].gameObject.GetComponent<WheelCollider>().suspensionSpring = js;
        }
    }
    public void ExitOkBtn() => myObjDest();
    public void ExitCancleBtn()
    {
        ExitPanel.SetActive(false);
    }

    void roomInfoSync(NetworkObject obj)
    {
        RoomInfoObj.GetComponent<RoomInfo>().playerCount = nowPlayer;
        RoomInfoObj.GetComponent<RoomInfo>().map = MyRoomInfo.Instance.map;
        RoomInfoObj.GetComponent<RoomInfo>().mapTime = MyRoomInfo.Instance.mapTime;
        RoomInfoObj.GetComponent<RoomInfo>().s0 = MyRoomInfo.Instance.s0;
        RoomInfoObj.GetComponent<RoomInfo>().s1 = MyRoomInfo.Instance.s1;
        RoomInfoObj.GetComponent<RoomInfo>().s2 = MyRoomInfo.Instance.s2;
        RoomInfoObj.GetComponent<RoomInfo>().s3 = MyRoomInfo.Instance.s3;
        RoomInfoObj.GetComponent<RoomInfo>().s4 = MyRoomInfo.Instance.s4;
        RoomInfoObj.GetComponent<RoomInfo>().s5 = MyRoomInfo.Instance.s5;
        RoomInfoObj.GetComponent<RoomInfo>().s6 = MyRoomInfo.Instance.s6;
        RoomInfoObj.GetComponent<RoomInfo>().s7 = MyRoomInfo.Instance.s7;

        RoomInfoObj.GetComponent<RoomInfo>().WorldMinute = MyRoomInfo.Instance.WorldMinute;
        RoomInfoObj.GetComponent<RoomInfo>().WorldSecond = MyRoomInfo.Instance.WorldSecond;
        RoomInfoObj.GetComponent<RoomInfo>().WorldMilli = MyRoomInfo.Instance.WorldMilli;
        RoomInfoObj.GetComponent<RoomInfo>().EndRace = MyRoomInfo.Instance.EndRace;
        RoomInfoObj.GetComponent<RoomInfo>().EndTime = MyRoomInfo.Instance.EndTime;

        RoomInfoObj.GetComponent<RoomInfo>().Top1 = MyRoomInfo.Instance.Top1;
        RoomInfoObj.GetComponent<RoomInfo>().Top2 = MyRoomInfo.Instance.Top2;
        RoomInfoObj.GetComponent<RoomInfo>().Top3 = MyRoomInfo.Instance.Top3;
        RoomInfoObj.GetComponent<RoomInfo>().Top4 = MyRoomInfo.Instance.Top4;
        RoomInfoObj.GetComponent<RoomInfo>().Top5 = MyRoomInfo.Instance.Top5;
        RoomInfoObj.GetComponent<RoomInfo>().Top6 = MyRoomInfo.Instance.Top6;
        RoomInfoObj.GetComponent<RoomInfo>().Top7 = MyRoomInfo.Instance.Top7;
        RoomInfoObj.GetComponent<RoomInfo>().Top8 = MyRoomInfo.Instance.Top8;
    }
    public void IG_leftPlayer() => playerLeft();
    public void playerLeft()
    {
        Transform[] localPlayerList = infoBox.GetComponentsInChildren<Transform>();
        nowPlayer = localPlayerList.Length - 1;
        adminChange = 1;

        if (runner.IsSharedModeMasterClient)
        {
            if (!RoomInfoObj.GetComponent<RoomInfo>().roomAdmin.Equals(ENB.id))
            {
                if (adminChange == 1)
                {
                    Destroy(RoomInfoObj);

                    createRoomObj();
                    adminChange = 0;
                }
            }
            else adminChange = 0;
        }

        GameObject[] RoomInfoObjects = GameObject.FindGameObjectsWithTag("RoomInfo");
        for (int i = 0; i < RoomInfoObjects.Length; i++)
        {
            if (RoomInfoObjects[i].GetComponent<NetworkObject>().StateAuthority.IsNone) Destroy(RoomInfoObjects[i].gameObject);
            if (RoomInfoObjects[i].GetComponent<NetworkObject>().StateAuthority != RoomInfoObjects[i].GetComponent<NetworkObject>().InputAuthority) Destroy(RoomInfoObjects[i].gameObject);
        }

        for (int i = 1; i < localPlayerList.Length; i++)
        {
            if (localPlayerList[i].GetComponent<NetworkObject>().StateAuthority.IsNone) Destroy(localPlayerList[i].gameObject);
            if (localPlayerList[i].GetComponent<NetworkObject>().StateAuthority != localPlayerList[i].GetComponent<NetworkObject>().InputAuthority) Destroy(localPlayerList[i].gameObject);
        }

        GameObject[] localPlayerCarList = GameObject.FindGameObjectsWithTag("PlayerCar");
        for (int i = 0; i < localPlayerCarList.Length; i++)
        {
            if (localPlayerCarList[i].GetComponent<NetworkObject>().StateAuthority.IsNone) Destroy(localPlayerCarList[i].gameObject);
            if (localPlayerCarList[i].GetComponent<NetworkObject>().StateAuthority != localPlayerCarList[i].GetComponent<NetworkObject>().InputAuthority) Destroy(localPlayerCarList[i].gameObject);
        }
    }

    void createRoomObj()
    {
        NetworkObject obj = runner.Spawn(roomInfo, gameObject.transform.position, Quaternion.identity, ENB.myNO.GetComponent<PlayerInfo>().pid);
        obj.GetComponent<RoomInfo>().roomAdmin = ENB.id;
        obj.GetComponent<RoomInfo>().ingameExitAdmin = false;
        roomInfoSync(obj);
        DontDestroyOnLoad(obj);
    }
    void myObjDest()
    {
        if (runner.IsSharedModeMasterClient) RoomInfoObj.GetComponent<RoomInfo>().ingameExitAdmin = true;

        Destroy(infoBox);
        Destroy(RoomInfoObj);

        runner.Despawn(myCar);
        runner.Despawn(myGun);
        runner.Despawn(myInfoDes);

        ENB.gamePlay = false;

        GameManager.instance.U_InviReset();
        ENB.gameinfo.room = "x";

        runner.Shutdown();

        GameManager.instance.U_LRChange("x");
        SceneManager.LoadScene("Loading");
    }

    // CPU AI
    void CpuCarSpawn(string name, int num)
    {
        GameObject SpawnPoint;
        Transform[] spList = SpawnBox.GetComponentsInChildren<Transform>();
        SpawnPoint = spList[num + 1].gameObject;

        if (num == 0)
        {
            if (name.Equals("람파다티 카스코"))
                CpuPlayerCar = runner.Spawn(Lampadati, SpawnPoint.transform.position, SpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
            else if (name.Equals("베네팩터 덥스타"))
                CpuPlayerCar = runner.Spawn(Benefacter, SpawnPoint.transform.position, SpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
            else if (name.Equals("알바니 프리모"))
                CpuPlayerCar = runner.Spawn(Alvani, SpawnPoint.transform.position, SpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
            else if (name.Equals("페가시 젠토르노"))
                CpuPlayerCar = runner.Spawn(Pegasi, SpawnPoint.transform.position, SpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);

            CpuPlayerCar.gameObject.GetComponent<GamePlayerInfo>().createId = ENB.id;
            CpuPlayerCar.gameObject.GetComponent<GamePlayerInfo>().spNum = num + 1;

            MainCamera.GetComponent<SmoothFollow>().target = CpuPlayerCar.transform;
        }
        else
        {
            if (name.Equals("람파다티 카스코"))
                CpuAiCar = runner.Spawn(AILampadati, SpawnPoint.transform.position, SpawnPoint.transform.rotation, 1);
            else if (name.Equals("베네팩터 덥스타"))
                CpuAiCar = runner.Spawn(AIBenefacter, SpawnPoint.transform.position, SpawnPoint.transform.rotation, 1);
            else if (name.Equals("알바니 프리모"))
                CpuAiCar = runner.Spawn(AIAlvani, SpawnPoint.transform.position, SpawnPoint.transform.rotation, 1);
            else if (name.Equals("페가시 젠토르노"))
                CpuAiCar = runner.Spawn(AIPegasi, SpawnPoint.transform.position, SpawnPoint.transform.rotation, 1);

            CpuAiCar.GetComponent<GamePlayerInfo>().createId = "CPU";
            CpuAiCar.gameObject.GetComponent<GamePlayerInfo>().spNum = num + 1;

            // CpuAiCar.gameObject.AddComponent<스크립트명>();
        }
    }
    void CpuGunSpawn(NetworkObject car, string name, int num)
    {
        GameObject gunSpawnPoint = null;
        Transform[] spList = car.GetComponentsInChildren<Transform>();
        for (int i = 0; i < spList.Length; i++) if (spList[i].name.Equals("GunSpawn"))
            {
                gunSpawnPoint = spList[i].gameObject;
                break;
            }

        if (num == 0)
        {
            if (name.Equals("곡사포"))
                CpuPlayerGun = runner.Spawn(Howitzer, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
            else if (name.Equals("발칸"))
                CpuPlayerGun = runner.Spawn(Vulcan, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
            else if (name.Equals("쇼커"))
                CpuPlayerGun = runner.Spawn(Shoker, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);
            else if (name.Equals("캐논"))
                CpuPlayerGun = runner.Spawn(Canon, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, ENB.myNO.GetComponent<PlayerInfo>().pid);

            CpuPlayerGun.gameObject.GetComponent<GamePlayerInfo>().createId = ENB.id;
            CpuPlayerGun.gameObject.GetComponent<GamePlayerInfo>().spNum = num + 1;
        }
        else
        {
            if (name.Equals("곡사포"))
                CpuAiGun = runner.Spawn(AIHowitzer, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, 1);
            else if (name.Equals("발칸"))
                CpuAiGun = runner.Spawn(AIVulcan, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, 1);
            else if (name.Equals("쇼커"))
                CpuAiGun = runner.Spawn(AIShoker, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, 1);
            else if (name.Equals("캐논"))
                CpuAiGun = runner.Spawn(AICanon, gunSpawnPoint.transform.position, gunSpawnPoint.transform.rotation, 1);

            CpuAiGun.GetComponent<GamePlayerInfo>().createId = "CPU";
            CpuAiGun.gameObject.GetComponent<GamePlayerInfo>().spNum = num + 1;
        }
    }

    void CpuSetCarGunObject()
    {
        for (int i = 0; i < 2; i++)
        {
            if (i == 0)
            {
                Transform[] CarChildList = CpuPlayerCar.GetComponentsInChildren<Transform>();
                GameObject gunSpawnPoint = null;
                for (int k = 0; k < CarChildList.Length; k++) if (CarChildList[k].name.Equals("GunSpawn"))
                    {
                        gunSpawnPoint = CarChildList[k].gameObject;
                        break;
                    }
                CpuPlayerCar.transform.parent = PlayerObjList.transform;
                CpuPlayerGun.transform.localScale = new Vector3(1, 1, 1);
                CpuPlayerGun.transform.position = gunSpawnPoint.transform.position;
                CpuPlayerGun.transform.parent = gunSpawnPoint.transform;
            }
            else
            {
                Transform[] CarChildList = CpuAiCar.GetComponentsInChildren<Transform>();
                GameObject gunSpawnPoint = null;
                for (int k = 0; k < CarChildList.Length; k++) if (CarChildList[k].name.Equals("GunSpawn"))
                    {
                        gunSpawnPoint = CarChildList[k].gameObject;
                        break;
                    }
                CpuAiCar.transform.parent = PlayerObjList.transform;
                CpuAiGun.transform.localScale = new Vector3(1, 1, 1);
                CpuAiGun.transform.position = gunSpawnPoint.transform.position;
                CpuAiGun.transform.parent = gunSpawnPoint.transform;
            }
        }
    }
}