using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class shield : MonoBehaviour
{
    public static shield instance = null;

    private void Awake()
    {
        instance = this;
    }

    public void OnClick()
    {
        PlayerController.isShield = true;
        if (PlayerController.isShield == true)
        {
            PlayerController.def += ENB.itemShield;
        }
        Invoke("Shieldoff", 5);
        ItemController.itemCount--;
        gameObject.SetActive(false);
        
    }

    void Shieldoff()
    {
        if(PlayerController.isShield == true)
        {
            PlayerController.def -= ENB.itemShield;
        }
        PlayerController.isShield = false;
    }
}
