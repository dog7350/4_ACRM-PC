using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoRestore : MonoBehaviour
{
    public static AmmoRestore instance = null;

    private void Awake()
    {
        instance = this;
    }

    public void OnClick()
    {
        PlayerFire.isAmmoRestore = true;
        Invoke("AmmoRestoreoff", 2);
        ItemController.itemCount--;
        gameObject.SetActive(false);
    }

    void AmmoRestoreoff()
    {
        PlayerFire.isAmmoRestore = false;
    }
}
