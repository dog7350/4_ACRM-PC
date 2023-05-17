using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class AIFire : NetworkBehaviour
{
    public static AIFire instance = null;
    public NetworkRunner runner;

    private void Awake()
    {
        instance = this;
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
    }

    public GameObject RoomInfoObj;

    public NetworkObject BulletSpawn;
    public GameObject FirePosition;
    public bool isAttack = false;
    public float atk;
    public float RpmTime = 0.5f;

    void Start()
    {
        RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");

        atk = RoomInfoObj.GetComponent<RoomInfo>().CpuAtk;
    }

    void Targeting()
    {
        float targetRadius = 3.5f;
        float targetRange = 1500f;

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));
        if(rayHits.Length > 0)
        {
            FireBullet();
        }
    }

    void FixedUpdate()
    {
        RpmTime -= Time.deltaTime;
        if (RpmTime <= 0)
        {
            isAttack = true;
        }
        if(isAttack == true)
        {
            Targeting();
        }

    }

    public void FireBullet()
    {
        NetworkObject bullet = Runner.Spawn(BulletSpawn);
        bullet.transform.position = FirePosition.transform.position;
        bullet.transform.forward = FirePosition.transform.forward;
        bullet.GetComponent<Bullet>().damage = atk;
        isAttack = false;
        RpmTime = 1;
    }
}
