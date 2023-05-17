using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Explosion : NetworkBehaviour
{
    public NetworkRunner runner;
    private void Awake() => runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();

    float lifeTime = 2f;

    void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            gameObject.transform.parent.transform.parent.transform.parent.transform.parent.GetComponent<GamePlayerInfo>().Explosion = false;
            gameObject.transform.parent.transform.parent.transform.parent.transform.parent.GetComponent<PlayerController>().explosionActive = false;
            lifeTime = 2f;
        }
    }
}
