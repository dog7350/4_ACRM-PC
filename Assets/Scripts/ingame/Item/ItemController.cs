using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemController : MonoBehaviour
{
    public static ItemController instance = null;

    private void Awake()
    {
        instance = this;
    }

    public GameObject itemSlot1;
    public GameObject itemSlot2;

    public GameObject Shielditem;
    public GameObject Berserkeritem;
    public GameObject HpRestoreitem;
    public GameObject AmmoRestoreitem;
    public GameObject Boosteritem;
    public static int LapsDone = 0;
    public static float itemCount = 0;
    
    void Update()
    {
        if (itemCount >= 2)
        {
            PlayerController.isFullItem = true;
            PlayerController.isGetItem = false;
        }
        else if (itemCount < 2 && itemCount > 0)
        {
            PlayerController.isFullItem = false;
        }
        if (itemCount < 0)
        {
            itemCount = 0;
        }
        else if (itemCount > 2)
        {
            itemCount = 2;
        }
    }

    public void ItemSlotPrint()
    {
        GameObject tmp = null;

        for (int i = 0; i < PlayerController.instance.itemList.Count; i++)
        {
            switch (PlayerController.instance.itemList[i])
            {
                case 1: tmp = HpRestoreitem; break;
                case 2: tmp = Berserkeritem; break;
                case 3: tmp = Boosteritem; break;
                case 4: tmp = AmmoRestoreitem; break;
                case 5: tmp = Shielditem; break;
            }

            if (i == 0)
            {
                tmp.transform.position = itemSlot1.transform.position;
            }
            else
            {
                tmp.transform.position = itemSlot2.transform.position;
            }
        }
    }

    public void ItemHpRestore() => HpRestore.instance.OnClick();
}
