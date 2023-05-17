using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using WebSocketSharp;
using Fusion;

public class RoomCPU : MonoBehaviour
{
    public static RoomCPU instance = null;
    public NetworkRunner runner;

    public AudioClip music;
    private void Awake()
    {
        instance = this;
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
    }

    float[,] itemPosition = new float[,]
    {
        { 220, 20, -250 }, { -220, 20, -250 }
    };
    float[,] itemRotation = new float[,]
    {
        { 10, -180, 0 }, { 10, 180, 0 }
    };
    float[,] itemScale = new float[,]
    {
        { 25, 25, 25 }, { 25, 25, 25 }
    };

    [Header("Chat UI Component")]
    public InputField ChatInput;

    [Header("Room Info Component")]
    public GameObject CpuRoomObj;
    int init = 1;
    public GameObject InfoBox;
    public Image mapImage;
    public Text roomTitle;
    public int difficult;
    bool ready = false;
    public int joinCount = 1;

    [Header("Room Info Setting")]
    public GameObject roomSetPanel;
    public Dropdown roomDifficult;
    public Text roomDefaultLevel;

    [Header("User Info Component")]
    public Text carText;
    public Text gunText;

    [Header("Player Panel")]
    public GameObject myInfo;
    public GameObject ReadyPanel;
    public Button[] Playerinfos;
    public Image[] PlayerCarStats;
    public Image[] PlayerGunStats;

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

    void Start()
    {
        init = 1;

        GameManager.instance.U_UserStatusUpdate();
        DifficultSetting();

        ENB.nowSceneName = SceneManager.GetActiveScene().name;

        carText.text = ENB.gameinfo.usecar;
        gunText.text = ENB.gameinfo.usegun;
    }

    void Update()
    {
        if (myInfo == null) myInfo = GameObject.FindGameObjectWithTag("PlayerInfo");
        if (InfoBox == null) InfoBox = GameObject.FindGameObjectWithTag("InfoBox");
        if (CpuRoomObj == null) CpuRoomObj = GameObject.FindGameObjectWithTag("RoomInfo");

        if (InfoBox != null || myInfo != null || init == 1)
        {
            myInfo.transform.parent = InfoBox.transform;
            myInfo.GetComponent<PlayerInfo>().roomNum = 0;
            myInfo.GetComponent<PlayerInfo>().team = "i";
            init = 0;
        }

        if (CpuRoomObj != null)
        {
            cpuInfoPrint();
            playerInfoPrint();
        }

        if (ready == true)
        {
            int mapTime = int.Parse(Random.Range(1, 6).ToString());

            ready = false;

            DontDestroyOnLoad(InfoBox);
            DontDestroyOnLoad(myInfo);
            DontDestroyOnLoad(CpuRoomObj);

            ENB.gamePlay = true;
            ENB.cpuPlay = true;

            ReadyPanel.SetActive(false);

            SceneManager.LoadScene("G_RaceTrack");
            /*
            int mapNum = difficult % 10;
            if (mapNum == 0 || mapNum == 1 || mapNum == 4 || mapNum == 7) SceneManager.LoadScene("G_RaceTrack");
            else if (mapNum == 2 || mapNum == 5 || mapNum == 8) SceneManager.LoadScene("G_City");
            else if (mapNum == 3 || mapNum == 6 || mapNum == 9) SceneManager.LoadScene("G_Mountain");
            */
        }
    }
    void OnChatEnter(InputValue value)
    {
        if (ChatInput.isFocused == false) ChatInput.Select();
    }

    public void U_LeaveRoom()
    {
        GameManager.instance.U_InviReset();
        ENB.gameinfo.room = "x";

        runner.Shutdown();

        GameManager.instance.U_LRChange("x");
        SceneManager.LoadScene("Loading");
    }
    void DifficultSetting()
    {
        difficult = int.Parse(ENB.gameinfo.difficult);
        roomDifficult.options.Clear();
        roomDefaultLevel.text = difficult.ToString();
        for (int i = 1; i <= difficult; i++)
        {
            Dropdown.OptionData opt = new Dropdown.OptionData();
            opt.text = i.ToString();
            roomDifficult.options.Add(opt);
        }
        roomDifficult.value = difficult;
    }

    /* Room Setting */
    public void U_RoomSettingOpne()
    {
        unSetCarGun();
        roomSetPanel.SetActive(true);
    }
    public void U_RoomSettingClose()
    {
        setCarGun();
        roomSetPanel.SetActive(false);
    }
    void unSetCarGun()
    {
        for (int i = 0; i < 2; i++)
            Playerinfos[i].transform.GetChild(3).gameObject.SetActive(false);
    }
    void setCarGun()
    {
        for (int i = 0; i < 2; i++)
            Playerinfos[i].transform.GetChild(3).gameObject.SetActive(true);
    }
    public void U_RoomSettingApply() => roomInfoSetting();
    void roomInfoSetting()
    {
        difficult = roomDifficult.value + 1;
        cpuInfoPrint();
        playerInfoPrint();
        // 난이도 설정 반영
    }
    void cpuInfoPrint()
    {
        slotClean(1);

        int cpuDifficult = difficult;
        string cpuCar = "";
        string cpuGun = "";
        if (cpuDifficult % 10 == 0) // 페가시, 캐논
        {
            cpuCar = "페가시 젠토르노";
            cpuGun = "캐논";
        }
        else if (cpuDifficult % 10 >= 7) // 람파다티, 발칸
        {
            cpuCar = "람파다티 카스코";
            cpuGun = "발칸";
        }
        else if (cpuDifficult % 10 >= 4) // 알바니, 쇼커
        {
            cpuCar = "알바니 프리모";
            cpuGun = "쇼커";
        }
        else if (cpuDifficult % 10 >= 1) // 베네팩터, 곡사포
        {
            cpuCar = "베네팩터 덥스타";
            cpuGun = "곡사포";
        }
        mapImage.sprite = Resources.Load<Sprite>("Sprites/더트스노우 서킷");
        /*
        int mapNum = difficult % 10;
        if (mapNum == 0 || mapNum == 1 || mapNum == 4 || mapNum == 7) mapImage.sprite = Resources.Load<Sprite>("Sprites/더트스노우 서킷");
        else if (mapNum == 2 || mapNum == 5 || mapNum == 8) mapImage.sprite = Resources.Load<Sprite>("Sprites/시카도 그랑프리");
        else if (mapNum == 3 || mapNum == 6 || mapNum == 9) mapImage.sprite = Resources.Load<Sprite>("Sprites/하포가하라");
        */

        Playerinfos[1].transform.GetChild(1).GetComponent<Text>().text = cpuCar;
        Playerinfos[1].transform.GetChild(2).GetComponent<Text>().text = cpuGun;
        GameObject tmp = Resources.Load("Car/" + cpuCar) as GameObject;
        GameObject item = Instantiate(tmp, Playerinfos[1].transform.GetChild(3).transform);

        item.transform.parent = Playerinfos[1].transform.GetChild(3).transform;
        item.transform.localPosition = new Vector3(itemPosition[1, 0], itemPosition[1, 1], itemPosition[1, 2]);
        item.transform.rotation = Quaternion.Euler(itemRotation[1, 0], itemRotation[1, 1], itemRotation[1, 2]);
        item.transform.localScale = new Vector3(itemScale[1, 0], itemScale[1, 1], itemScale[1, 2]);

        float hp = 0, def = 0, speed = 0;
        float atk = 0, ammo = 0, rpm = 0;
        for (int i = 0; i < ENB.carinfos.Count; i++)
            if (ENB.carinfos[i].cname.Equals(cpuCar))
            {
                hp = ENB.carinfos[i].hp;
                hp = hp + (hp * (difficult * 0.1f));
                def = ENB.carinfos[i].def;
                def = def + (def * (difficult * 0.1f));
                speed = ENB.carinfos[i].speed;
                speed = speed + (speed * (difficult * 0.1f));
                break;
            }
        for (int i = 0; i < ENB.guninfos.Count; i++)
            if (ENB.guninfos[i].gname.Equals(cpuGun))
            {
                atk = ENB.guninfos[i].atk;
                atk = atk + (atk * (difficult * 0.1f));
                ammo = ENB.guninfos[i].ammo;
                ammo = ammo + (int)(ammo * (difficult * 0.1f));
                rpm = ENB.guninfos[i].rpm;
                rpm = rpm + (rpm * (difficult * 0.1f));
                break;
            }

        CpuRoomObj.GetComponent<RoomInfo>().CpuCar = cpuCar;
        CpuRoomObj.GetComponent<RoomInfo>().CpuGun = cpuGun;
        CpuRoomObj.GetComponent<RoomInfo>().CpuHp = hp;
        CpuRoomObj.GetComponent<RoomInfo>().CpuDef = def;
        CpuRoomObj.GetComponent<RoomInfo>().CpuSpeed = speed;
        CpuRoomObj.GetComponent<RoomInfo>().CpuAtk = atk;
        CpuRoomObj.GetComponent<RoomInfo>().CpuAmmo = ammo;
        CpuRoomObj.GetComponent<RoomInfo>().CpuRpm = rpm;

        PlayerCarStats[3].fillAmount = hp / difficultStat(ENB.maxHp);
        PlayerCarStats[4].fillAmount = def / difficultStat(ENB.maxDef);
        PlayerCarStats[5].fillAmount = speed / difficultStat(ENB.maxSpeed);

        PlayerGunStats[3].fillAmount = atk / difficultStat(ENB.maxAtk);
        PlayerGunStats[4].fillAmount = ammo / difficultStat(ENB.maxAmmo);
        PlayerGunStats[5].fillAmount = rpm / difficultStat(ENB.maxRpm);
    }
    void playerInfoPrint()
    {
        slotClean(0);

        int cpuDifficult = difficult;
        Playerinfos[0].transform.GetChild(0).GetComponent<Text>().text = ENB.id;
        Playerinfos[0].transform.GetChild(1).GetComponent<Text>().text = ENB.gameinfo.usecar;
        Playerinfos[0].transform.GetChild(2).GetComponent<Text>().text = ENB.gameinfo.usegun;
        GameObject tmp = Resources.Load("Car/" + ENB.gameinfo.usecar) as GameObject;
        GameObject item = Instantiate(tmp, Playerinfos[0].transform.GetChild(3).transform);

        item.transform.parent = Playerinfos[0].transform.GetChild(3).transform;
        item.transform.localPosition = new Vector3(itemPosition[0, 0], itemPosition[0, 1], itemPosition[0, 2]);
        item.transform.rotation = Quaternion.Euler(itemRotation[0, 0], itemRotation[0, 1], itemRotation[0, 2]);
        item.transform.localScale = new Vector3(itemScale[0, 0], itemScale[0, 1], itemScale[0, 2]);

        float hp = 0, def = 0, speed = 0;
        float atk = 0, ammo = 0, rpm = 0;
        for (int i = 0; i < ENB.carinfos.Count; i++)
            if (ENB.carinfos[i].cname.Equals(ENB.gameinfo.usecar))
            {
                hp = ENB.carinfos[i].hp;
                def = ENB.carinfos[i].def;
                speed = ENB.carinfos[i].speed;
                break;
            }
        for (int i = 0; i < ENB.guninfos.Count; i++)
            if (ENB.guninfos[i].gname.Equals(ENB.gameinfo.usegun))
            {
                atk = ENB.guninfos[i].atk;
                ammo = ENB.guninfos[i].ammo;
                rpm = ENB.guninfos[i].rpm;
                break;
            }

        PlayerCarStats[0].fillAmount = hp  / difficultStat(ENB.maxHp);
        PlayerCarStats[1].fillAmount = def / difficultStat(ENB.maxDef);
        PlayerCarStats[2].fillAmount = speed / difficultStat(ENB.maxSpeed);

        PlayerGunStats[0].fillAmount = atk / difficultStat(ENB.maxAtk);
        PlayerGunStats[1].fillAmount = ammo / difficultStat(ENB.maxAmmo);
        PlayerGunStats[2].fillAmount = rpm / difficultStat(ENB.maxRpm);
    }
    void slotClean(int num)
    {
        if (num != -1)
        {
            Playerinfos[num].transform.GetChild(1).GetComponent<Text>().text = "";
            Playerinfos[num].transform.GetChild(2).GetComponent<Text>().text = "";

            Transform[] child = Playerinfos[num].transform.GetChild(3).GetComponentsInChildren<Transform>();
            if (child != null) for (int j = 1; j < child.Length; j++) if (child[j] != transform) Destroy(child[j].gameObject);
        }
        else
        {
            for (int i = 0; i < Playerinfos.Length; i++)
            {
                Playerinfos[i].transform.GetChild(1).GetComponent<Text>().text = "";
                Playerinfos[i].transform.GetChild(2).GetComponent<Text>().text = "";

                Transform[] child = Playerinfos[i].transform.GetChild(3).GetComponentsInChildren<Transform>();
                if (child != null) for (int j = 1; j < child.Length; j++) if (child[j] != transform) Destroy(child[j].gameObject);
            }
        }
    }
    float difficultStat(float stat) => (stat + (stat * (difficult * 0.1f)));

    /* Ready */
    public void U_ReadyBtn()
    {
        ReadyPanel.SetActive(true);
        if (ready == true) ready = false;
        else ready = true;
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
        playerInfoPrint();
        GameManager.instance.U_UserStatusUpdate();
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
    void childDestroy(GameObject obj)
    {
        Transform[] childList = obj.GetComponentsInChildren<Transform>();

        if (childList != null) for (int i = 1; i < childList.Length; i++) if (childList[i] != transform) Destroy(childList[i].gameObject);
    }
}
