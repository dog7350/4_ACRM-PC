using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AItarget : MonoBehaviour
{
    public float TargetNum;
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("AICar") && col.GetComponent<MoveAI>().TargetCount == TargetNum - 1)
        {
            col.GetComponent<MoveAI>().TargetCount = TargetNum;
        }
    }

}
