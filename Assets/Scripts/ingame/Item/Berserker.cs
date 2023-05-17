using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berserker : MonoBehaviour
{
    public static Berserker instance = null;

    private void Awake()
    {
        instance = this;
    }

    public void OnClick()
    {
        PlayerController.isBerserker = true;
        Invoke("Berserkeroff", 5);
        ItemController.itemCount--;
        gameObject.SetActive(false);
    }

    void Berserkeroff()
    {
        PlayerController.isBerserker = false;
    }
}
