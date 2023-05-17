using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : NetworkBehaviour
{
    public NetworkRunner runner;
    private void Awake() => runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();

    public float speed = 1000;
    public float damage; // °ø°Ý·Â
    [Networked] public string team { get; set; }
    float time = 0;
    float waitingTime = 2;
    public void Start()
    {
        damage = PlayerFire.instance.atk;
        GetComponent<Rigidbody>().AddForce(transform.forward * speed, ForceMode.Impulse);

        if (ENB.cpuPlay == true) waitingTime = 5f;
    }
    public override void FixedUpdateNetwork()
    {
        time += Time.deltaTime;
        if(time > waitingTime)
        {
            runner.Despawn(Object);
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;

        if (obj.GetComponent<NetworkObject>() != null)
        {
            if (obj.GetComponent<NetworkObject>().HasInputAuthority == false)
            {
                if (obj.CompareTag("PlayerCar") || obj.CompareTag("AICar"))
                {
                    gameObject.GetComponent<SphereCollider>().isTrigger = false;
                    gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    time = 0;
                    waitingTime = 0.5f;
                }
            }
        }
        else
        {
            if (obj.CompareTag("Wall"))
            {
                gameObject.GetComponent<SphereCollider>().isTrigger = false;
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                time = 0;
                waitingTime = 0.5f;
            }
        }
    }
}
