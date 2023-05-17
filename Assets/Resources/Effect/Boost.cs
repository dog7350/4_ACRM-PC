using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Boost : NetworkBehaviour
{
    public NetworkRunner runner;
    private void Awake() => runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();

    float lifeTime = 4f;

    void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            gameObject.transform.parent.transform.parent.transform.parent.transform.parent.GetComponent<GamePlayerInfo>().Boost = false;
            lifeTime = 4f;
        }
    }
}
