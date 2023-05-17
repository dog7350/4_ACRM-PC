using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPU_Explosion : MonoBehaviour
{
    float lifeTime = 2f;

    void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            lifeTime = 2f;
            gameObject.SetActive(false);
        }
    }
}
