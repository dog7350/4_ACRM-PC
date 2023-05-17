using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    public static Booster instance = null;

    private void Awake()
    {
        instance = this;
    }

    public void OnClick()
    {
        PlayerController.isBooster = true;
        Invoke("Boosteroff", 3);
        ItemController.itemCount--;
        gameObject.SetActive(false);

    }

    void Boosteroff()
    {
        PlayerController.isBooster = false;
    }
}
