using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Garage : MonoBehaviour
{
    public static Garage instance;

    void Awake() => instance = this;

    [Header("SpawnPoint")]
    public GameObject carSpawn;
    public GameObject gunSpawn;

    [Header("Garage UI Component")]
    public Text itemTitle;
    public Text itemPrice;
    public Text itemCnM;
    public Text itemHpAtk;
    public Image HpAtkImg;
    public Text itemDefAmmo;
    public Image DefAmmoImg;
    public Text itemSpdRpm;
    public Image SpdRpmImg;
    public Text itemContent;

    [Header("Garage Item Scroll")]
    public GameObject itemScrollContent;
    public GameObject itemInstance;
    public Button carSelectBtn;
    public Button gunSelectBtn;

    int cargunFlag = 0;
    GameObject item = null;
    float itemRotate = 0.5f;

    GameObject prevCarBtn = null;
    GameObject prevGunBtn = null;
    DBClass.carinfo car = null;
    DBClass.guninfo gun = null;

    [Header("Menu List Component")]
    public Text carText;
    public Text gunText;
    public Button itemShop;
    public Button cash;

    void Start()
    {
        GameManager.instance.U_UserStatusUpdate();

        usingItem();
    }

    void Update()
    {
        if (item != null) item.transform.Rotate(new Vector3(0, itemRotate, 0));
    }
    void OnChatEnter(InputValue value)
    {
        if (LobbyChat.instance.ChatInput.isFocused == false) LobbyChat.instance.ChatInput.Select();
    }

    /* Menu Button */
    public void U_LobbyClick() => SceneManager.LoadScene("Lobby");
    public void U_ItemShopClick() => SceneManager.LoadScene("Shop");
    public void U_CashClick() => Application.OpenURL(GameManager.cashURL);

    public void U_SelectCarList()
    {
        gunSelectBtn.interactable = true;
        carSelectBtn.interactable = false;

        cargunFlag = 0;
        Transform[] childList = itemScrollContent.GetComponentsInChildren<Transform>();

        if (childList != null) for (int i = 1; i < childList.Length; i++) if (childList[i] != transform) Destroy(childList[i].gameObject);

        for (int i = 0; i < ENB.carinfos.Count; i++)
        {
            if (useCar(i) == 1)
            {
                GameObject tmp = GameObject.Instantiate(itemInstance, itemScrollContent.transform);
                tmp.transform.parent = itemScrollContent.transform;

                string name = ENB.carinfos[i].cname;

                if (car != null && car.cname.Equals(name))
                {
                    prevCarBtn = tmp;
                    tmp.GetComponent<Button>().interactable = false;
                }
                else if (car == null && ENB.gameinfo.usecar.Equals(name))
                {
                    prevCarBtn = tmp;
                    tmp.GetComponent<Button>().interactable = false;
                }

                tmp.transform.GetChild(0).GetComponent<Text>().text = name;
                tmp.GetComponent<Button>().onClick.AddListener(delegate { U_SelectItem(name, tmp); });
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
    public void U_SelectGunList()
    {
        gunSelectBtn.interactable = false;
        carSelectBtn.interactable = true;

        cargunFlag = 1;
        Transform[] childList = itemScrollContent.GetComponentsInChildren<Transform>();

        if (childList != null) for (int i = 1; i < childList.Length; i++) if (childList[i] != transform) Destroy(childList[i].gameObject);

        for (int i = 0; i < ENB.guninfos.Count; i++)
        {
            if (useGun(i) == 1)
            {
                GameObject tmp = GameObject.Instantiate(itemInstance, itemScrollContent.transform);
                tmp.transform.parent = itemScrollContent.transform;

                string name = ENB.guninfos[i].gname;

                if (gun != null && gun.gname.Equals(name))
                {
                    prevGunBtn = tmp;
                    tmp.GetComponent<Button>().interactable = false;
                }
                else if (gun == null && ENB.gameinfo.usegun.Equals(name))
                {
                    prevGunBtn = tmp;
                    tmp.GetComponent<Button>().interactable = false;
                }

                tmp.transform.GetChild(0).GetComponent<Text>().text = name;
                tmp.GetComponent<Button>().onClick.AddListener(delegate { U_SelectItem(name, tmp); });
            }
        }
    }
    int useGun(int num)
    {
        if (ENB.useguns[0] == null) return 0;

        for (int i = 0; i < ENB.useguns.Count; i++)
            if (ENB.useguns[i].gnum == num + 1) return 1;

        return 0;
    }

    public void U_SelectItem(string name, GameObject obj)
    {
        if (item != null) Destroy(item);

        if (cargunFlag == 0) // 0 : Car
        {
            if (prevCarBtn != null) prevCarBtn.GetComponent<Button>().interactable = true;
            prevCarBtn = obj;
            obj.GetComponent<Button>().interactable = false;
            GameObject tmp = Resources.Load<GameObject>("Car/" + name);
            item = Instantiate(tmp, carSpawn.transform);
            item.AddComponent<Rigidbody>();
            item.GetComponent<Rigidbody>().mass = 0.001f;
            item.transform.parent = carSpawn.transform;

            for (int i = 0; i < ENB.carinfos.Count; i++)
                if (ENB.carinfos[i].cname.Equals(name))
                {
                    car = ENB.carinfos[i];
                    break;
                }

            itemTitle.text = car.cname;
            if (car.cash.Equals("o")) itemCnM.text = "캐시";
            else itemCnM.text = "게임머니";
            itemPrice.text = car.price.ToString();
            itemContent.text = car.content;

            itemHpAtk.text = car.hp.ToString();
            HpAtkImg.fillAmount = (car.hp * 0.01f) / (ENB.maxHp * 0.01f);
            itemDefAmmo.text = car.def.ToString();
            DefAmmoImg.fillAmount = (car.def * 0.01f) / (ENB.maxDef * 0.01f);
            itemSpdRpm.text = car.speed.ToString();
            SpdRpmImg.fillAmount = (car.speed * 0.01f) / (ENB.maxSpeed * 0.01f);
        }
        else // 1 : Gun
        {
            if (prevGunBtn != null) prevGunBtn.GetComponent<Button>().interactable = true;
            prevGunBtn = obj;
            obj.GetComponent<Button>().interactable = false;

            GameObject tmp = Resources.Load("Gun/" + name) as GameObject;
            item = Instantiate(tmp, gunSpawn.transform);
            item.AddComponent<Rigidbody>();
            item.GetComponent<Rigidbody>().mass = 0.001f;
            item.transform.parent = gunSpawn.transform;

            for (int i = 0; i < ENB.guninfos.Count; i++)
                if (ENB.guninfos[i].gname.Equals(name))
                {
                    gun = ENB.guninfos[i];
                    break;
                }

            itemTitle.text = gun.gname;
            if (gun.cash.Equals("o")) itemCnM.text = "캐시";
            else itemCnM.text = "게임머니";
            itemPrice.text = gun.price.ToString();
            itemContent.text = gun.content;

            itemHpAtk.text = gun.atk.ToString();
            HpAtkImg.fillAmount = (gun.atk * 0.01f) / (ENB.maxAtk * 0.01f);
            itemDefAmmo.text = gun.ammo.ToString();
            DefAmmoImg.fillAmount = (gun.ammo * 0.01f) / (ENB.maxAmmo * 0.01f);
            itemSpdRpm.text = gun.rpm.ToString();
            SpdRpmImg.fillAmount = (gun.rpm * 0.01f) / (ENB.maxRpm * 0.01f);
        }
    }

    public void U_UseItem()
    {
        if (car != null) ENB.gameinfo.usecar = car.cname;
        if (gun != null) ENB.gameinfo.usegun = gun.gname;

        StartCoroutine(useItem());

        Invoke("usingItem", 0.3f);
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
    void usingItem()
    {
        carText.text = ENB.gameinfo.usecar;
        gunText.text = ENB.gameinfo.usegun;
    }
}
