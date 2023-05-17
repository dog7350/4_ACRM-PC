using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerFire : NetworkBehaviour
{
    public static bool isAmmoRestore = false;
    public static PlayerFire instance = null;
    public NetworkRunner runner;

    public GameObject myInfo;
    public NetworkObject BulletSpawn;
    public NetworkObject BerBulletSpawn;
    public GameObject FirePosition;

    public float timer = 0;
    public static float MaxAmmo;
    public static float nowAmmo;
    public float atk; // 공격력
    public float rpm; // 연사 속도

    private void Awake()
    {
        instance = this;
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
        MaxAmmo = ENB.itemAmmo;
    }
    [Networked] private NRunner.NetworkInputData inputs { get; set; }

    public override void FixedUpdateNetwork() => PvpUpdate();
    void PvpUpdate()
    {
        if (myInfo == null) myInfo = GameDirector.instance.myInfoObj;

        if (isAmmoRestore == true)
        {
            nowAmmo = MaxAmmo;
        }
        timer += Time.deltaTime;

        if (PlayerController.health <= 0)
        {
            PlayerController.isbroken = true;
        }
        else
        {
            PlayerController.isbroken = false;
        }

        if (GetInput(out NRunner.NetworkInputData input))
        {
            inputs = input;
        }
        if (GameDirector.instance.chatInputFlag == false) Shot(inputs);
    }

    void Shot(NRunner.NetworkInputData input)
    {
        if (gameObject.GetComponent<NetworkObject>().HasStateAuthority)
        {
            var startShot = input.IsShotPressedThisFrame;

            if (startShot) InvokeRepeating("StartShot", 0, rpm);

            if (!input.IsShotPressed) CancelInvoke("StartShot");
        }
    }
    private void StartShot()
    {
        if(rpm <= timer && PlayerController.isbroken == false && PlayerController.isBerserker == true)
        {
            BerFireBullet();
            timer = 0;
        }
        else if (rpm <= timer && PlayerController.isbroken == false && PlayerController.isBerserker == false && nowAmmo > 0)
        {
            FireBullet();
            nowAmmo--;
            timer = 0;
        }
    }

    private void BerFireBullet()
    {
        NetworkObject bullet = Runner.Spawn(BerBulletSpawn);
        bullet.transform.position = FirePosition.transform.position;
        bullet.transform.forward = FirePosition.transform.forward;
        bullet.GetComponent<Bullet>().damage = atk + (atk * 0.2f);
        bullet.GetComponent<Bullet>().team = myInfo.GetComponent<PlayerInfo>().team;
    }
    public void FireBullet()
    {
        NetworkObject bullet = Runner.Spawn(BulletSpawn);
        bullet.transform.position = FirePosition.transform.position;
        bullet.transform.forward = FirePosition.transform.forward;
        bullet.GetComponent<Bullet>().damage = atk;
        bullet.GetComponent<Bullet>().team = myInfo.GetComponent<PlayerInfo>().team;
    }
}
