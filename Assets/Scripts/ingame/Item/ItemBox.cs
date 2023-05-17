using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ItemBox : NetworkBehaviour
{
    public static bool isBoxOff = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerCar") == true && isBoxOff == true)
        {
            gameObject.GetComponent<BoxCollider>().isTrigger = false;

            PlayerController playercon = other.GetComponent<PlayerController>();
            playercon.haveBox = 1;

            if (other.gameObject.GetComponent<NetworkObject>().HasStateAuthority == true) ItemListInsert(other);

            //Runner.Despawn(Object);
            Destroy(gameObject);
        }
    }

    void ItemListInsert(Collider other)
    {
        if (PlayerController.isGetItem == true && PlayerController.isFullItem == false)
        {
            if (ItemController.itemCount < 2 && PlayerController.isGetItem == true && PlayerController.isFullItem == false)
            {
                //int item = Random.Range(1, 6);
                int item = Random.Range(4, 6);
                int listnum = 0;

                switch (item)
                {
                    case 1:
                        if (ItemController.instance.Shielditem.activeSelf == true)
                        {
                            ItemController.instance.Berserkeritem.SetActive(true);
                            listnum = 2;
                        }
                        else
                        {
                            ItemController.instance.Shielditem.SetActive(true);
                            listnum = 5;
                        }
                        break;

                    case 2:
                        if (ItemController.instance.Berserkeritem.activeSelf == true)
                        {
                            ItemController.instance.HpRestoreitem.SetActive(true);
                            listnum = 1;
                        }
                        else
                        {
                            ItemController.instance.Berserkeritem.SetActive(true);
                            listnum = 2;
                        }
                        break;

                    case 3:
                        if (ItemController.instance.HpRestoreitem.activeSelf == true)
                        {
                            ItemController.instance.AmmoRestoreitem.SetActive(true);
                            listnum = 4;
                        }
                        else
                        {
                            ItemController.instance.HpRestoreitem.SetActive(true);
                            listnum = 1;
                        }
                        break;

                    case 4:
                        if (ItemController.instance.AmmoRestoreitem.activeSelf == true)
                        {
                            ItemController.instance.Boosteritem.SetActive(true);
                            listnum = 3;
                        }
                        else
                        {
                            ItemController.instance.AmmoRestoreitem.SetActive(true);
                            listnum = 4;
                        }
                        break;

                    case 5:
                        if (ItemController.instance.Boosteritem.activeSelf == true)
                        {
                            ItemController.instance.Shielditem.SetActive(true);
                            listnum = 5;
                        }
                        else
                        {
                            ItemController.instance.Boosteritem.SetActive(true);
                            listnum = 3;
                        }
                        break;
                }

                other.GetComponent<PlayerController>().itemList.Add(listnum);
                ItemController.itemCount++;
                PlayerController.isGetItem = false;
                ItemController.instance.ItemSlotPrint();
            }
        }
    }
}
