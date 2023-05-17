using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpRestore : MonoBehaviour
{
    public static HpRestore instance = null;

    private void Awake()
    {
        instance = this;
    }

    public void OnClick()
    {
        PlayerController.isHpRestore = true;
        Invoke("HpRestoreoff", 2);
        ItemController.itemCount--;
        gameObject.SetActive(false);

    }

    void HpRestoreoff()
    {
        PlayerController.isHpRestore = false;
    }
}
