using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ShieldEff : NetworkBehaviour
{
    float time = 0;
    int waitingTime = 5;
    // Start is called before the first frame update
    public override void FixedUpdateNetwork()
    {
        time += Time.deltaTime;
        if (time > waitingTime)
        {
            Runner.Despawn(Object);
        }
    }
}
