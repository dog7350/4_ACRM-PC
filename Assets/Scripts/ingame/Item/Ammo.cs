using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ammo : MonoBehaviour
{
    public static Ammo instance = null;

    private void Awake()
    {
        instance = this;
    }
    public Text AmmoText;

    public void Start()
    {
        AmmoText.text = PlayerFire.nowAmmo.ToString() + "/" + PlayerFire.MaxAmmo.ToString();
    }

    public void Update()
    {
        AmmoText.text = PlayerFire.nowAmmo.ToString() + "/" + PlayerFire.MaxAmmo.ToString();
    }

}
